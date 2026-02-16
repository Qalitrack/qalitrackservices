using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Qalitrack.Services;

public class RfidOptions
{
    public string Host { get; set; } = "172.16.0.117";
    public int Port { get; set; } = 2022;
}

public class RfidReaderBackgroundService : BackgroundService
{
    private readonly RfidTagStreamService _streamService;
    private readonly ILogger<RfidReaderBackgroundService> _logger;
    private readonly RfidOptions _options;

    public RfidReaderBackgroundService(
        RfidTagStreamService streamService,
        ILogger<RfidReaderBackgroundService> logger,
        IOptions<RfidOptions> options)
    {
        _streamService = streamService;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunReaderLoopAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "RFID reader loop failed - reconnecting in 5s");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task RunReaderLoopAsync(CancellationToken ct)
    {
        using var client = new TcpClient();
        _logger.LogInformation("Connecting to RFID reader {Host}:{Port}", _options.Host, _options.Port);
        await client.ConnectAsync(_options.Host, _options.Port, ct);

        await using var stream = client.GetStream();

        // Stop any running inventory (CF protocol)
        var stopCmd = BuildCommand(0xFF, 0x0002, Array.Empty<byte>());
        await stream.WriteAsync(stopCmd, ct);
        await Task.Delay(500, ct);
        
        // Clear buffer
        var clearBuffer = new byte[1024];
        while (stream.DataAvailable)
            await stream.ReadAsync(clearBuffer, ct);

        // Start continuous inventory
        var startCmd = BuildCommand(0xFF, 0x0001, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00 });
        await stream.WriteAsync(startCmd, ct);

        var seen = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            if (!stream.DataAvailable)
            {
                await Task.Delay(100, ct);
                continue;
            }

            int bytesRead = await stream.ReadAsync(buffer, ct);
            if (bytesRead == 0) continue;

            var tags = ParseTags(buffer, bytesRead);

            foreach (var tag in tags)
            {
                if (!seen.Contains(tag.Tid))
                {
                    seen.Add(tag.Tid);
                    _logger.LogInformation("New tag detected → TID: {Tid}, RSSI: {Rssi} dBm, Ant: {Ant}", 
                        tag.Tid, tag.Rssi, tag.Antenna);

                    await _streamService.PublishTagAsync(tag.Tid, ct);
                }
            }
        }

        // Stop inventory on exit
        await stream.WriteAsync(stopCmd, ct);
    }

    private byte[] BuildCommand(byte addr, ushort cmd, byte[] data)
    {
        var frame = new byte[6 + data.Length];
        frame[0] = 0xCF;
        frame[1] = addr;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(2), cmd);
        frame[4] = (byte)data.Length;
        data.CopyTo(frame, 5);

        var crc = CalcCrc16(frame.AsSpan(1, 4 + data.Length));
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(5 + data.Length), crc);

        return frame;
    }

    private ushort CalcCrc16(ReadOnlySpan<byte> data)
    {
        const ushort PRESET = 0xFFFF;
        const ushort POLY = 0x8408;
        ushort crc = PRESET;

        foreach (var b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                crc = (crc & 0x0001) != 0 ? (ushort)((crc >> 1) ^ POLY) : (ushort)(crc >> 1);
            }
        }

        return crc;
    }

    private System.Collections.Generic.List<(string Tid, double Rssi, byte Antenna, byte Channel)> ParseTags(byte[] data, int length)
    {
        var tags = new System.Collections.Generic.List<(string, double, byte, byte)>();
        int pos = 0;

        while (pos < length)
        {
            if (pos + 5 > length || data[pos] != 0xCF)
            {
                pos++;
                continue;
            }

            try
            {
                int frameLen = data[pos + 4];

                if (pos + 5 + frameLen + 2 > length)
                {
                    pos++;
                    continue;
                }

                // Check if tag response (cmd = 0x0001)
                if (data[pos + 2] == 0x00 && data[pos + 3] == 0x01)
                {
                    short rssiRaw = BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(pos + 6, 2));
                    double rssi = rssiRaw / 10.0;
                    byte antenna = data[pos + 8];
                    byte channel = data[pos + 9];
                    byte dataLen = data[pos + 10];

                    if (dataLen > 0 && pos + 11 + dataLen <= length)
                    {
                        var tid = BitConverter.ToString(data, pos + 11, dataLen).Replace("-", "");
                        tags.Add((tid, rssi, antenna, channel));
                    }
                }

                pos += 5 + frameLen + 2;
            }
            catch
            {
                pos++;
            }
        }

        return tags;
    }
}