using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Qalitrack.Services;

public class RfidTagStreamService : IDisposable
{
    private readonly ConcurrentDictionary<string, (Channel<string> Channel, DateTime LastActivity)> _streams = new();
    private readonly ILogger<RfidTagStreamService> _logger;
    private readonly StreamingMetrics _metrics; // you can keep or remove metrics
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private bool _disposed;
    private readonly TimeSpan _inactiveTimeout = TimeSpan.FromMinutes(5);

    public RfidTagStreamService(
        ILogger<RfidTagStreamService> logger,
        StreamingMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _ = CleanupInactiveConnectionsAsync();
    }

    public async Task PublishTagAsync(string epc, CancellationToken ct = default)
    {
        if (_disposed) return;

        var payload = new { epc = epc.Trim(), timestamp = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(payload, _jsonOptions);

        var tasks = _streams.Select(async kvp =>
        {
            var (clientId, (channel, _)) = kvp;
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(150);

                if (await channel.Writer.WaitToWriteAsync(cts.Token))
                {
                    await channel.Writer.WriteAsync(json, cts.Token);
                    _metrics?.MessageProcessed(clientId, json.Length, 0);
                    _streams[clientId] = (channel, DateTime.UtcNow);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        });

        var results = await Task.WhenAll(tasks);
        var failed = results.Where(r => !r).ToList();

        // You can clean failed clients here if desired
    }

    public IAsyncEnumerable<string> SubscribeAsync(string clientId, CancellationToken ct = default)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RfidTagStreamService));

        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        _streams[clientId] = (channel, DateTime.UtcNow);
        _metrics?.ConnectionStarted(clientId);
        _logger.LogInformation("Client {ClientId} subscribed to RFID stream", clientId);

        ct.Register(() =>
        {
            if (_streams.TryRemove(clientId, out var info))
            {
                info.Channel.Writer.TryComplete();
                _metrics?.ConnectionEnded(clientId, "Cancelled");
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
        }
    }

    private async Task CleanupInactiveConnectionsAsync()
    {
        while (!_disposed)
        {
            await Task.Delay(60000);
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
                    _logger.LogInformation("Removed inactive RFID client {ClientId}", id);
                }
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (_, (channel, _)) in _streams)
            channel.Writer.TryComplete();

        _streams.Clear();
        _metrics?.Dispose();
        GC.SuppressFinalize(this);
    }
}