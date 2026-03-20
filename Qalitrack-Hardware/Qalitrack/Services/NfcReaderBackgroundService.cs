using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qalitrack.Models;

namespace Qalitrack.Services;

public class NfcReaderBackgroundService : BackgroundService
{
    private readonly NfcTagStreamService _streamService;
    private readonly ILogger<NfcReaderBackgroundService> _logger;
    private readonly NfcSettings _settings;
    private SerialPort? _serialPort;

    public NfcReaderBackgroundService(
        NfcTagStreamService streamService,
        ILogger<NfcReaderBackgroundService> logger,
        NfcSettings settings)
    {
        _streamService = streamService;
        _logger = logger;
        _settings = settings;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogWarning("NFC reader is DISABLED in settings — set Enabled=true to activate");
            return;
        }

        _logger.LogInformation("NfcReaderBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunReaderLoopAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "NFC loop crashed — reconnecting in 8s");
                await Task.Delay(8000, stoppingToken);
            }
        }
    }

    private async Task RunReaderLoopAsync(CancellationToken ct)
    {
        _logger.LogInformation("=== NFC READER STARTING ===");
        _logger.LogInformation("Opening {Port} at {Baud} baud", _settings.Port, _settings.BaudRate);

        _serialPort = new SerialPort
        {
            PortName     = _settings.Port,
            BaudRate     = _settings.BaudRate,
            DataBits     = 8,
            Parity       = Parity.None,
            StopBits     = StopBits.One,
            Handshake    = Handshake.XOnXOff,
            ReadTimeout  = 1000,
            WriteTimeout = 1000,
            DtrEnable    = false,
            RtsEnable    = false,
        };

        try
        {
            _serialPort.Open();
            _logger.LogInformation("✅ Serial port OPEN — listening for NFC tags...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "✗ Failed to open {Port}", _settings.Port);
            throw;
        }

        // Frame is always exactly 9 bytes: AA BB 06 01 [UID0 UID1 UID2 UID3] [checksum]
        const int FrameLength = 9;
        var frameBuffer       = new byte[FrameLength];
        int bytesRead         = 0;
        var lastHeartbeat     = DateTime.UtcNow;

        while (!ct.IsCancellationRequested && _serialPort.IsOpen)
        {
            try
            {
                if (_serialPort.BytesToRead > 0)
                {
                    int b = _serialPort.ReadByte();
                    if (b < 0) continue;

                    // Sync to frame start — discard anything before 0xAA
                    if (bytesRead == 0 && b != 0xAA)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"│ SKIP: {b:X2} (waiting for AA)");
                        Console.ResetColor();
                        continue;
                    }

                    frameBuffer[bytesRead++] = (byte)b;

                    if (bytesRead == FrameLength)
                    {
                        bytesRead = 0;

                        var frameHex = BitConverter.ToString(frameBuffer).Replace("-", " ");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"│ FRAME: {frameHex}");
                        Console.ResetColor();

                        // UID = bytes 4,5,6,7
                        var uid = $"{frameBuffer[4]:X2}{frameBuffer[5]:X2}{frameBuffer[6]:X2}{frameBuffer[7]:X2}";
                        _logger.LogInformation("🏷️ NFC tag → UID: {Uid}", uid);
                        await _streamService.PublishTagAsync(uid, ct);

                        lastHeartbeat = DateTime.UtcNow;
                    }
                }
                else
                {
                    if ((DateTime.UtcNow - lastHeartbeat).TotalSeconds >= 30)
                    {
                        _logger.LogInformation("NFC reader alive on {Port} — waiting for tags", _settings.Port);
                        lastHeartbeat = DateTime.UtcNow;
                    }

                    await Task.Delay(10, ct);
                }
            }
            catch (TimeoutException)
            {
                continue;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error reading from {Port} — will reconnect", _settings.Port);
                throw;
            }
        }
    }


    public override Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _logger.LogInformation("Serial port {Port} closed cleanly", _settings.Port);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing serial port {Port}", _settings.Port);
        }

        return base.StopAsync(cancellationToken);
    }
}

public class NfcSettings
{
    public bool   Enabled  { get; set; } = true;
    public string Port     { get; set; } = "COM7";
    public int    BaudRate { get; set; } = 38400;
}