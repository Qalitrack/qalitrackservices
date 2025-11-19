using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Qalitrack.Services;

public class StreamingMetrics : IDisposable
{
    private readonly ILogger<StreamingMetrics> _logger;
    private readonly Meter _meter;
    private readonly ConcurrentDictionary<string, (DateTime StartTime, int MessageCount)> _activeConnections = new();
    private readonly ConcurrentBag<long> _messageProcessingTimes = new();
    
    // Counters
    private readonly Counter<long> _totalConnections;
    private readonly Counter<long> _totalMessages;
    private readonly Counter<long> _totalErrors;
    private readonly Histogram<double> _messageLatency;
    private readonly object _latencyLock = new();
    
    // Gauges
    private int _activeConnectionsCount = 0;
    private int _messagesPerSecond = 0;
    
    public StreamingMetrics(ILogger<StreamingMetrics> logger)
    {
        _logger = logger;
        _meter = new Meter("Qalitrack.Streaming");
        
        // Initialize counters
        _totalConnections = _meter.CreateCounter<long>("streaming.connections.total", "Total number of connections");
        _totalMessages = _meter.CreateCounter<long>("streaming.messages.total", "Total number of messages processed");
        _totalErrors = _meter.CreateCounter<long>("streaming.errors.total", "Total number of errors");
        _messageLatency = _meter.CreateHistogram<double>("streaming.latency", "ms", "Message processing latency in milliseconds");
        
        // Start background task to calculate messages per second
        _ = CalculateMetricsAsync();
    }
    
 
    public void ConnectionEnded(string clientId, string reason = "Normal")
    {
        if (_activeConnections.TryRemove(clientId, out var connection))
        {
            var duration = DateTime.UtcNow - connection.StartTime;
            Interlocked.Decrement(ref _activeConnectionsCount);
            
            _logger.LogInformation("Client {ClientId} disconnected. " +
                                "Reason: {Reason}, Active connections: {ActiveConnections}",
                clientId, duration, connection.MessageCount, reason, _activeConnectionsCount);
        }
    }
    
   
    // Add these fields at the class level
private long _totalConnectionsCount = 0;
private long _totalMessagesCount = 0;
private long _totalErrorsCount = 0;

// Update the ConnectionStarted method
public void ConnectionStarted(string clientId)
{
    _activeConnections[clientId] = (DateTime.UtcNow, 0);
    _totalConnections.Add(1);
    Interlocked.Increment(ref _totalConnectionsCount);
    Interlocked.Increment(ref _activeConnectionsCount);
    _logger.LogInformation("Client {ClientId} connected. Active connections: {Count}", 
        clientId, _activeConnectionsCount);
}
private void RecordLatency(double milliseconds)
{
    try
    {
        // Use a tag to provide additional context
        var tags = new[] { new KeyValuePair<string, object?>("source", "MessageProcessed") };
        _messageLatency.Record(milliseconds, tags);
        
        // Add to processing times for average calculation
        lock (_latencyLock)
        {
            _messageProcessingTimes.Add((long)milliseconds);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error recording latency measurement");
    }
}
// Update the MessageProcessed method
public void MessageProcessed(string clientId, int messageSize, double processingTimeMs)
{
    if (_activeConnections.TryGetValue(clientId, out var connection))
    {
        connection.MessageCount++;
        _activeConnections[clientId] = connection;
        _totalMessages.Add(1);
        Interlocked.Increment(ref _totalMessagesCount);
        RecordLatency(processingTimeMs);
    }
}

// Update the RecordError method
public void RecordError(string connectionId, Exception exception)
{
    _totalErrors.Add(1);
    Interlocked.Increment(ref _totalErrorsCount);
    _logger.LogError(exception, "Error in connection {ConnectionId}", connectionId);
}

// Update the GetMetrics method
public object GetMetrics()
{
    var now = DateTime.UtcNow;
    
    // Get active client information
    var activeClients = _activeConnections.ToDictionary(
        c => c.Key,
        c => new 
        {
            Duration = now - c.Value.StartTime,
            c.Value.MessageCount
        });
        
    // Calculate average latency
    long[] processingTimes;
    lock (_latencyLock)
    {
        processingTimes = _messageProcessingTimes.ToArray();
    }
    var avgLatency = processingTimes.Length > 0 ? processingTimes.Average() : 0;
    
    return new
    {
        Timestamp = now,
        ActiveConnections = _activeConnectionsCount,
        TotalConnections = _totalConnectionsCount,
        TotalMessages = _totalMessagesCount,
        TotalErrors = _totalErrorsCount,
        AverageLatencyMs = Math.Round(avgLatency, 2),
        MessagesPerSecond = _messagesPerSecond,
        ActiveClients = activeClients,
        Metrics = new
        {
            Connections = new
            {
                Active = _activeConnectionsCount,
                Total = _totalConnectionsCount
            },
            Messages = new
            {
                Total = _totalMessagesCount,
                Rate = _messagesPerSecond
            },
            Latency = new
            {
                AverageMs = Math.Round(avgLatency, 2),
                SampleSize = processingTimes.Length
            },
            Errors = _totalErrorsCount
        }
    };
}
    
   private async Task CalculateMetricsAsync()
{
    var lastMessageCount = 0L;
    var lastCheck = DateTime.UtcNow;
    var cancellationToken = new CancellationTokenSource();
    
    try
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken.Token);
                
                var now = DateTime.UtcNow;
                var elapsed = (now - lastCheck).TotalSeconds;
                var currentCount = _totalMessagesCount; // Use our tracking variable
                
                if (elapsed > 0.1) // Ensure we have a minimum elapsed time
                {
                    var newMessages = currentCount - lastMessageCount;
                    Interlocked.Exchange(ref _messagesPerSecond, (int)(newMessages / elapsed));
                    
                    lastMessageCount = currentCount;
                    lastCheck = now;
                    
                    // Log summary every 30 seconds
                    if (now.Second % 30 == 0)
                    {
                        double avgLatency = 0;
                        
                        // Get the current histogram measurements
                        try
                        {
                            long[] processingTimes;
                            lock (_latencyLock)
                            {
                                processingTimes = _messageProcessingTimes.ToArray();
                            }
                            
                            if (processingTimes.Length > 0)
                            {
                                avgLatency = processingTimes.Average();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error calculating average latency");
                        }
                        
                        _logger.LogInformation(
                            "Streaming metrics - Active: {Active}, Msg/s: {MsgPerSec}, " +
                            "Avg Latency: {AvgLatency:F2}ms, Total Messages: {TotalMessages}, " +
                            "Total Errors: {TotalErrors}",
                            _activeConnectionsCount, 
                            _messagesPerSecond, 
                            avgLatency,
                            currentCount, 
                            _totalErrorsCount); // Use our tracking variable
                    }
                    
                    // Clear old measurements periodically to prevent memory growth
                    if (now.Minute % 5 == 0 && now.Second < 5) // Every 5 minutes
                    {
                        lock (_latencyLock)
                        {
                            _messageProcessingTimes.Clear();
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in metrics calculation");
                // Add a small delay to prevent tight error loops
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
            }
        }
    }
    finally
    {
        cancellationToken.Cancel();
        cancellationToken.Dispose();
    }
}
    
    public void Dispose()
    {
        _meter?.Dispose();
        _messageProcessingTimes.Clear();
        _activeConnections.Clear();
    }
}
