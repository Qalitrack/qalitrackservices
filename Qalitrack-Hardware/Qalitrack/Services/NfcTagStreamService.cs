using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Qalitrack.Services;

public class NfcTagStreamService : IDisposable
{
    private readonly ConcurrentDictionary<string, (Channel<string> Channel, DateTime LastActivity)> _streams = new();
    private readonly ILogger<NfcTagStreamService> _logger;
    private readonly StreamingMetrics _metrics;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private bool _disposed;
    private readonly TimeSpan _inactiveTimeout = TimeSpan.FromMinutes(5);
    private readonly CancellationTokenSource _cleanupCts = new();

    public NfcTagStreamService(
        ILogger<NfcTagStreamService> logger,
        StreamingMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _ = CleanupInactiveConnectionsAsync(_cleanupCts.Token);
    }

    // UID is already extracted by NfcReaderBackgroundService — just publish it directly,
    // same as RfidTagStreamService does with the TID string.
    public async Task PublishTagAsync(string uid, CancellationToken ct = default)
    {
        if (_disposed) return;

        var json = JsonSerializer.Serialize(uid, _jsonOptions);

        foreach (var kvp in _streams)
        {
            var (clientId, (channel, _)) = (kvp.Key, kvp.Value);
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(150);

                if (await channel.Writer.WaitToWriteAsync(cts.Token))
                {
                    await channel.Writer.WriteAsync(json, cts.Token);
                    _metrics?.MessageProcessed(clientId, json.Length, 0);
                    _streams[clientId] = (channel, DateTime.UtcNow);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send NFC tag to client {ClientId}", clientId);
            }
        }
    }

    public IAsyncEnumerable<string> SubscribeAsync(string clientId, CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(NfcTagStreamService));

        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
        {
            FullMode     = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        _streams[clientId] = (channel, DateTime.UtcNow);
        _metrics?.ConnectionStarted(clientId);
        _logger.LogInformation("Client {ClientId} subscribed to NFC stream", clientId);

        ct.Register(() =>
        {
            if (_streams.TryRemove(clientId, out var info))
            {
                info.Channel.Writer.TryComplete();
                _metrics?.ConnectionEnded(clientId, "Cancelled");
                _logger.LogInformation("Client {ClientId} cancelled NFC subscription", clientId);
            }
        }, useSynchronizationContext: false);

        return channel.Reader.ReadAllAsync(ct);
    }

    public void Unsubscribe(string clientId)
    {
        if (_disposed) return;
        if (_streams.TryRemove(clientId, out var info))
        {
            _metrics?.ConnectionEnded(clientId, "Unsubscribed");
            info.Channel.Writer.TryComplete();
            _logger.LogInformation("Client {ClientId} unsubscribed from NFC", clientId);
        }
    }

    private async Task CleanupInactiveConnectionsAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, ct);
                var now = DateTime.UtcNow;
                var inactive = _streams
                    .Where(k => (now - k.Value.LastActivity) > _inactiveTimeout)
                    .Select(k => k.Key)
                    .ToList();

                foreach (var id in inactive)
                {
                    if (_streams.TryRemove(id, out var info))
                    {
                        info.Channel.Writer.TryComplete();
                        _metrics?.ConnectionEnded(id, "Inactive");
                        _logger.LogInformation("Removed inactive NFC client {ClientId}", id);
                    }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in NFC cleanup task");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cleanupCts.Cancel();
        _cleanupCts.Dispose();

        foreach (var (_, (channel, _)) in _streams)
            channel.Writer.TryComplete();

        _streams.Clear();
        _metrics?.Dispose();
        GC.SuppressFinalize(this);
    }
}