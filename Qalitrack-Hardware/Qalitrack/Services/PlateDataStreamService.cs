
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;


namespace Qalitrack.Services;

public class PlateDataStreamService : IDisposable
{
    private readonly ConcurrentDictionary<string, (Channel<string> Channel, DateTime LastActivity, string? CameraFilter)> _streams = new();
    private readonly ILogger<PlateDataStreamService> _logger;
    private readonly StreamingMetrics _metrics;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultBufferSize = 4096
    };
    private bool _disposed;
    private readonly TimeSpan _inactiveTimeout = TimeSpan.FromMinutes(5);

    public PlateDataStreamService(
        ILogger<PlateDataStreamService> logger,
        StreamingMetrics metrics)
    {
        _logger = logger;
        _metrics = metrics;
        _ = CleanupInactiveConnectionsAsync();
    }

    public async Task PublishPlateAsync(object plateData, CancellationToken cancellationToken = default)
    {
        if (_disposed) return;

        var stopwatch = Stopwatch.StartNew();
        var json = string.Empty;

        try
        {
            json = JsonSerializer.Serialize(plateData, _jsonOptions);

            // Extract camera ID from the plate data
            string? cameraId = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("cameraId", out var cameraIdElement))
                {
                    cameraId = cameraIdElement.GetString();
                }
            }
            catch
            {
                // Ignore parse errors
            }

            var tasks = _streams
                .Where(kvp =>
                {
                    var (_, (_, _, cameraFilter)) = kvp;
                    // Send to client if:
                    // 1. Client has no filter (receives all cameras)
                    // 2. Client's filter matches the camera ID
                    return cameraFilter == null || cameraFilter == cameraId;
                })
                .Select(async kvp =>
                {
                    var (clientId, (channel, _, cameraFilter)) = kvp;

                    try
                    {
                        var sw = Stopwatch.StartNew();
                        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

                        if (await channel.Writer.WaitToWriteAsync(cts.Token).ConfigureAwait(false))
                        {
                            await channel.Writer.WriteAsync(json, cts.Token).ConfigureAwait(false);
                            _metrics.MessageProcessed(clientId, json.Length, sw.Elapsed.TotalMilliseconds);
                            _streams[clientId] = (channel, DateTime.UtcNow, cameraFilter);
                            return (clientId, Success: true);
                        }

                        return (clientId, Success: false);
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("Timeout writing to channel for client {ClientId}", clientId);
                        return (clientId, Success: false);
                    }
                    catch (Exception ex) when (ex is ChannelClosedException or ObjectDisposedException)
                    {
                        _logger.LogDebug("Channel closed for client {ClientId}", clientId);
                        return (clientId, Success: false);
                    }
                    catch (Exception ex)
                    {
                        _metrics.RecordError(clientId, ex);
                        return (clientId, Success: false);
                    }
                });

            var results = await Task.WhenAll(tasks);

            var disconnectedClients = results
                .Where(r => !r.Success)
                .Select(r => r.clientId)
                .ToList();

            foreach (var clientId in disconnectedClients)
            {
                if (_streams.TryRemove(clientId, out _))
                {
                    _metrics.ConnectionEnded(clientId, "WriteFailed");
                }
            }
        }
        catch (Exception ex)
        {
            _metrics.RecordError("System", new Exception("PublishError", ex));
            throw;
        }
        finally
        {
            stopwatch.Stop();
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("PublishPlateAsync took {ElapsedMs}ms for {MessageSize} bytes",
                    stopwatch.ElapsedMilliseconds, json?.Length ?? 0);
            }
        }
    }

    public IAsyncEnumerable<string> SubscribeAsync(string clientId, string? cameraFilter = null, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PlateDataStreamService));

        var channel = Channel.CreateBounded<string>(new BoundedChannelOptions(500)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        var now = DateTime.UtcNow;
        _streams[clientId] = (channel, now, cameraFilter);
        _metrics.ConnectionStarted(clientId);

        var filterInfo = cameraFilter != null ? $" (filtered to camera: {cameraFilter})" : " (all cameras)";
        _logger.LogInformation("Client {ClientId} subscribed to plate recognition stream{FilterInfo}. Active clients: {ClientCount}",
            clientId, filterInfo, _streams.Count);

        cancellationToken.Register(() =>
        {
            if (_streams.TryRemove(clientId, out _))
            {
                channel.Writer.TryComplete();
                _metrics.ConnectionEnded(clientId, "Cancelled");
                _logger.LogDebug("Client {ClientId} unsubscribed via cancellation token", clientId);
            }
        }, useSynchronizationContext: false);

        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    public void Unsubscribe(string clientId)
    {
        if (_disposed) return;

        if (_streams.TryRemove(clientId, out var channelInfo))
        {
            _metrics.ConnectionEnded(clientId, "Unsubscribed");
            _logger.LogInformation("Client {ClientId} unsubscribed from plate recognition stream. Active clients: {ClientCount}",
                clientId, _streams.Count);
            channelInfo.Channel.Writer.TryComplete();
        }
    }

    private async Task CleanupInactiveConnectionsAsync()
    {
        while (!_disposed)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var now = DateTime.UtcNow;
                var inactiveClients = _streams
                    .Where(kvp => (now - kvp.Value.LastActivity) > _inactiveTimeout)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var clientId in inactiveClients)
                {
                    if (_streams.TryRemove(clientId, out var channelInfo))
                    {
                        _metrics.ConnectionEnded(clientId, "InactiveTimeout");
                        channelInfo.Channel.Writer.TryComplete();
                        _logger.LogInformation("Disconnected inactive client {ClientId} after {Timeout} of inactivity",
                            clientId, _inactiveTimeout);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CleanupInactiveConnectionsAsync");
            }
        }
    }

    public int GetClientCount() => _streams.Count;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var (clientId, (channel, _, _)) in _streams)
        {
            try
            {
                channel.Writer.TryComplete();
                _metrics.ConnectionEnded(clientId, "ServiceDisposed");
                _logger.LogDebug("Completed channel for client {ClientId} during disposal", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing channel for client {ClientId}", clientId);
            }
        }
        _streams.Clear();

        _metrics.Dispose();
        GC.SuppressFinalize(this);
    }
}
