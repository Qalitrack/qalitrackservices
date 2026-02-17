using System;
using System.Buffers.Binary;
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
        _logger.LogInformation("Connecting to RFID reader {Host}:{Port}", _settings.Host, _settings.Port);
        await client.ConnectAsync(_settings.Host, _settings.Port, ct);
        _logger.LogInformation("Connected to RFID reader - listening for tags...");

        await using var stream = client.GetStream();

        var buffer = new byte[4096];

        while (!ct.IsCancellationRequested)
        {
            try
            {
                int bytesRead = await stream.ReadAsync(buffer, ct);
                if (bytesRead == 0)
                {
                    _logger.LogWarning("Connection closed by RFID reader");
                    break;
                }

                var tags = ParseTags(buffer, bytesRead);

                foreach (var tag in tags)
                {
                    _logger.LogInformation("Tag detected → TID: {Tid}, RSSI: {Rssi} dBm, Ant: {Ant}", 
                        tag.Tid, tag.Rssi, tag.Antenna);

                    await _streamService.PublishTagAsync(tag.Tid, ct);
                }
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogError(ex, "Error reading from RFID reader");
                throw;
            }
        }
    }

    private System.Collections.Generic.List<(string Tid, double Rssi, byte Antenna, byte Channel)> ParseTags(byte[] data, int length)
    {
        var tags = new System.Collections.Generic.List<(string, double, byte, byte)>();
        int pos = 0;

        while (pos < length)
        {
            // Look for frame start marker CF
            if (pos + 11 > length || data[pos] != 0xCF)
            {
                pos++;
                continue;
            }

            try
            {
                // CF [addr] [cmd_h] [cmd_l] [len] [status] [rssi_h] [rssi_l] [ant] [channel] [datalen] [data...] [crc_h] [crc_l]
                //  0    1      2       3      4      5         6        7       8      9        10
                
                byte frameLen = data[pos + 4];
                
                // Check if this is a tag response (cmd = 0x0001)
                if (data[pos + 2] != 0x00 || data[pos + 3] != 0x01)
                {
                    pos++;
                    continue;
                }

                // Get data length
                byte dataLen = data[pos + 10];
                
                // Verify we have enough bytes for complete frame
                if (pos + 11 + dataLen + 2 > length)
                {
                    pos++;
                    continue;
                }

                // Extract tag info
                short rssiRaw = BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(pos + 6, 2));
                double rssi = rssiRaw / 10.0;
                byte antenna = data[pos + 8];
                byte channel = data[pos + 9];

                if (dataLen > 0)
                {
                    var tid = BitConverter.ToString(data, pos + 11, dataLen).Replace("-", "");
                    tags.Add((tid, rssi, antenna, channel));
                }

                // Move to next frame
                pos += 11 + dataLen + 2;
            }
            catch
            {
                pos++;
            }
        }

        return tags;
    }
}