using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;


public class DataStreamService
{
    private readonly ConcurrentDictionary<string, Channel<string>> _streams = new();
    private readonly ILogger<DataStreamService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private long _totalMessagesPublished = 0;
    private long _totalClientsServed = 0;
    private readonly object _statsLock = new();

    public DataStreamService(ILogger<DataStreamService> logger)
    {
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        _logger.LogInformation("🔧 DataStreamService initialized");
    }

    public async Task PublishRawAsync(string raw, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(raw)) return;
        var tasks = new List<Task>();
        foreach (var (clientId, channel) in _streams)
        {
            try
            {
                tasks.Add(channel.Writer.WriteAsync(raw, cancellationToken).AsTask()
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted) _streams.TryRemove(clientId, out _);
                        else Interlocked.Increment(ref _totalMessagesPublished);
                    }, cancellationToken));
            }
            catch { _streams.TryRemove(clientId, out _); }
        }
        if (tasks.Count > 0) await Task.WhenAll(tasks);
    }

    public async Task PublishAsync<T>(T data, CancellationToken cancellationToken = default) where T : class
    {
        if (data == null)
        {
            _logger.LogWarning("⚠️ Attempted to publish null data");
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        string json;
        try
        {
            json = JsonSerializer.Serialize(data, _jsonOptions);
            _logger.LogTrace("📦 Serialized data to JSON: {JsonData}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to serialize data to JSON");
            return;
        }

        var activeClients = _streams.Count;
        _logger.LogDebug("📤 Preparing to publish data to {ClientCount} active clients", activeClients);

        var publishTasks = new List<Task>();
        int successfulPublishes = 0;
        int failedPublishes = 0;

        foreach (var (clientId, channel) in _streams)
        {
            try
            {
                _logger.LogTrace("📨 Sending data to client {ClientId}", clientId);
                var publishTask = channel.Writer.WriteAsync(json, cancellationToken).AsTask()
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _logger.LogError(t.Exception, "❌ Failed to send data to client {ClientId}", clientId);
                            Interlocked.Increment(ref failedPublishes);
                            _streams.TryRemove(clientId, out _);
                        }
                        else
                        {
                            Interlocked.Increment(ref successfulPublishes);
                            Interlocked.Increment(ref _totalMessagesPublished);
                            _logger.LogTrace("✅ Data sent to client {ClientId}", clientId);
                        }
                    }, cancellationToken);

                publishTasks.Add(publishTask);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "❌ Unexpected error while queuing data for client {ClientId}", clientId);
                Interlocked.Increment(ref failedPublishes);
                _streams.TryRemove(clientId, out _);
            }
        }

        try
        {
            await Task.WhenAll(publishTasks);
            stopwatch.Stop();

            if (activeClients > 0)
            {
                _logger.LogInformation(
                    "📊 Published data to {SuccessfulPublishes}/{TotalClients} clients in {ElapsedMs}ms. " +
                    "Total messages published: {TotalMessages}",
                    successfulPublishes, activeClients, stopwatch.ElapsedMilliseconds, _totalMessagesPublished);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "❌ Error during batch publish operation");
        }
    }

    public IAsyncEnumerable<string> SubscribeAsync(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogError("❌ Cannot subscribe client with null or empty ID");
            throw new ArgumentException("Client ID cannot be null or empty", nameof(clientId));
        }

        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var added = _streams.TryAdd(clientId, channel);
        if (!added)
        {
            _logger.LogWarning("⚠️ Client {ClientId} already exists in the subscription list", clientId);
            _streams[clientId] = channel; // Update the existing channel
        }

        Interlocked.Increment(ref _totalClientsServed);
        var activeClients = _streams.Count;
        
        _logger.LogInformation("➕ Client {ClientId} subscribed. Active clients: {ActiveClients}, Total clients served: {TotalClients}", 
            clientId, activeClients, _totalClientsServed);

        // Log subscription statistics periodically
        if (activeClients % 5 == 0) // Log every 5th subscription
        {
            _logger.LogInformation("📈 Subscription stats - Active: {ActiveClients}, Total served: {TotalClients}", 
                activeClients, _totalClientsServed);
        }

        // Remove the channel when the client disconnects
        cancellationToken.Register(() =>
        {
            _logger.LogDebug("🔌 Client {ClientId} cancellation token triggered", clientId);
            Unsubscribe(clientId);
        });

        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    /// Unsubscribes a client from the data stream
    /// </summary>
    /// <param name="clientId">The ID of the client to unsubscribe</param>
    public void Unsubscribe(string clientId)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            _logger.LogWarning("⚠️ Attempted to unsubscribe with null or empty client ID");
            return;
        }

        if (_streams.TryRemove(clientId, out var channel))
        {
            var remainingClients = _streams.Count;
            _logger.LogInformation("➖ Client {ClientId} unsubscribed. Remaining clients: {RemainingClients}", 
                clientId, remainingClients);
            
            try
            {
                channel.Writer.TryComplete();
                _logger.LogDebug("✅ Successfully completed channel for client {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error completing channel for client {ClientId}", clientId);
            }
        }
        else
        {
            _logger.LogDebug("ℹ️ Client {ClientId} not found in active subscriptions", clientId);
        }
    }
    
    /// <summary>
    /// Gets the number of active client connections
    /// </summary>
    public int GetClientCount()
    {
        var count = _streams.Count;
        _logger.LogTrace("📊 Current active client count: {ClientCount}", count);
        return count;
    }

    /// <summary>
    /// Gets statistics about the data streaming service
    /// </summary>
    public object GetStatistics()
    {
        return new
        {
            Timestamp = DateTime.UtcNow,
            ActiveClients = _streams.Count,
            TotalMessagesPublished = _totalMessagesPublished,
            TotalClientsServed = _totalClientsServed,
            ClientIds = _streams.Keys.ToList()
        };
    }
}
