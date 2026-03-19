using System;
using System.Buffers.Binary;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qalitrack.Models;

namespace Qalitrack.Services;

public class RfidReaderBackgroundService : BackgroundService
{
    private readonly RfidTagStreamService _streamService;
    private readonly ILogger<RfidReaderBackgroundService> _logger;
    private readonly RfidSettings _settings;

    private TcpClient? _client;
    private NetworkStream? _stream;

    public RfidReaderBackgroundService(
        RfidTagStreamService streamService,
        ILogger<RfidReaderBackgroundService> logger,
        RfidSettings settings)
    {
        _streamService = streamService;
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RfidReaderBackgroundService started (pure C# version)");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunReaderLoopAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "RFID loop crashed — reconnecting in 8s");
                await Task.Delay(8000, stoppingToken);
            }
        }
    }

    private async Task RunReaderLoopAsync(CancellationToken ct)
    {
        string host = _settings.Host;
        int port    = _settings.Port;

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogError("RFID host is not configured — set RfidSettings.Host in config file");
            throw new InvalidOperationException("RFID host is not configured");
        }

        if (port <= 0 || port > 65535)
        {
            _logger.LogError("RFID port is not configured — set RfidSettings.Port in config file");
            throw new InvalidOperationException("RFID port is not configured");
        }

        _logger.LogInformation("=== RFID READER STARTING ===");
        _logger.LogInformation("Connecting to {Host}:{Port}", host, port);

        _client = new TcpClient();
        await _client.ConnectAsync(host, port, ct);
        _stream = _client.GetStream();
        _logger.LogInformation("✅ TCP connected");

        // Give the reader a moment to send the ack (important!)
        await Task.Delay(500, ct);

        try
        {
            // 1. Read 2-byte ack with timeout — OPTIONAL, reader does not always send it
            _logger.LogInformation("Waiting for 2-byte ack from reader (optional)...");
            try
            {
                var ack = await ReadExactWithTimeoutAsync(2, TimeSpan.FromSeconds(5), ct);
                _logger.LogInformation("Reader ack received: {Ack}", BitConverter.ToString(ack));
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("No ack from reader — continuing anyway");
            }

            // 2. Send INIT
            var initCmd = new byte[] { 0xCF, 0xFF, 0x00, 0x70, 0x00, 0x24, 0x15 };
            await _stream.WriteAsync(initCmd, ct);
            _logger.LogInformation("Sent INIT command");

            // 3. Read banner
            _logger.LogInformation("Waiting for banner...");
            var banner = await ReadFrameAsync(ct);
            _logger.LogInformation("Banner received ({0} bytes)", banner.Length);

            // 4. Send GET CONFIG
            var cfgCmd = new byte[] { 0xCF, 0xFF, 0x00, 0x72, 0x00, 0x17, 0xA5 };
            await _stream.WriteAsync(cfgCmd, ct);
            _logger.LogInformation("Sent GET CONFIG");

            // 5. Read config
            _logger.LogInformation("Waiting for config response...");
            var configFrame = await ReadFrameAsync(ct);
            _logger.LogInformation("Config received ({0} bytes)", configFrame.Length);

            _logger.LogInformation("✅ FULL HANDSHAKE COMPLETE — Now listening for tags...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handshake failed");
            throw;
        }

        // Live tag loop
        while (!ct.IsCancellationRequested)
        {
            var frame = await ReadFrameAsync(ct);
            if (frame.Length == 0) continue;

            var tag = ParseTag(frame);
            if (tag is { } t)
            {
                _logger.LogInformation("🏷️ Tag detected → EPC: {Epc} | RSSI: {Rssi}dBm | Count: {Count}", t.Epc, t.RssiDbm, t.Count);
                await _streamService.PublishTagAsync(t.Epc, ct);
            }
        }
    }

    private async Task<byte[]> ReadFrameAsync(CancellationToken ct)
    {
        // Sync to 0xCF
        var b = new byte[1];
        while (true)
        {
            if (await _stream!.ReadAsync(b, ct) == 0) return Array.Empty<byte>();
            if (b[0] == 0xCF) break;
        }

        var header = new byte[4];
        await _stream.ReadAsync(header, ct);
        int len = header[3];

        var tail = new byte[len + 2];
        await _stream.ReadAsync(tail, ct);

        var frame = new byte[5 + len + 2];
        frame[0] = 0xCF;
        Buffer.BlockCopy(header, 0, frame, 1, 4);
        Buffer.BlockCopy(tail, 0, frame, 5, tail.Length);
        return frame;
    }

    private (string Epc, double RssiDbm, byte Count)? ParseTag(byte[] frame)
    {
        if (frame.Length < 21 || frame[2] != 0x00 || frame[3] != 0x01 || frame[4] != 0x0E)
            return null;

        // FIX: offset=0, length=frame.Length-2 — CRC must cover entire frame
        // including the 0xCF start byte, matching the working client implementation.
        // The previous offset=1 / length=frame.Length-3 skipped the CF byte,
        // causing every CRC check to fail and all tags to be silently dropped.
        ushort calc = CalcCrc16(frame, 0, frame.Length - 2);
        ushort recv = (ushort)((frame[^2] << 8) | frame[^1]);
        if (calc != recv)
        {
            _logger.LogWarning("⚠ CRC mismatch on tag frame — calc=0x{Calc:X4} recv=0x{Recv:X4}", calc, recv);
            return null;
        }

        sbyte rssiRaw = (sbyte)frame[7];
        double rssi = rssiRaw > 127 ? rssiRaw - 256 : rssiRaw;

        // Frame[9] = prefix byte (e.g. 0x04), frame[10] = TID length, frame[11..] = TID bytes
        // e.g. raw "04 08 11 05 20 00 81 5A A1 4F 2D 27" → we want "11 05 20 00 81 5A A1 4F"
        int tidLenOffset = 10;
        int tidLen = frame[tidLenOffset];
        int tidStart = tidLenOffset + 1;
        if (tidStart + tidLen > frame.Length - 2)
        {
            _logger.LogWarning("TID length {TidLen} exceeds frame bounds", tidLen);
            return null;
        }
        string epc = string.Join(" ", Enumerable.Range(tidStart, tidLen)
                                                .Select(i => frame[i].ToString("X2")));
        return (epc, rssi, frame[8]);
    }

    private async Task<byte[]> ReadExactWithTimeoutAsync(int count, TimeSpan timeout, CancellationToken ct)
    {
        var buffer = new byte[count];
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        int total = 0;
        while (total < count)
        {
            int read = await _stream!.ReadAsync(buffer.AsMemory(total, count - total), timeoutCts.Token);
            if (read == 0) throw new Exception("Reader disconnected during handshake");
            total += read;
        }
        return buffer;
    }

    private static ushort CalcCrc16(byte[] data, int offset, int length)
    {
        ushort crc = 0xFFFF;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
                crc = (ushort)((crc & 1) != 0 ? (crc >> 1) ^ 0x8408 : crc >> 1);
        }
        return crc;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _stream?.Close();
        _client?.Close();
        return base.StopAsync(cancellationToken);
    }
}