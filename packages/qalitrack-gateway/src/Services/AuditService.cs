using System.Text;
using System.Text.Json;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;
using Microsoft.Extensions.Options;

namespace QaliTrackGateway.Services;

/// <summary>
/// Audit service implementation for integration with transaction service
/// </summary>
public class AuditService : IAuditService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditService> _logger;
    private readonly AuditSettings _auditSettings;
    private readonly Queue<AuditEvent> _auditQueue = new();
    private readonly Timer _batchTimer;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditService(
        HttpClient httpClient,
        ILogger<AuditService> logger,
        IOptions<AuditSettings> auditSettings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _auditSettings = auditSettings.Value;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // Setup batch processing timer
        _batchTimer = new Timer(ProcessBatchAsync, null, 
            TimeSpan.FromSeconds(_auditSettings.BatchIntervalSeconds), 
            TimeSpan.FromSeconds(_auditSettings.BatchIntervalSeconds));
    }

    public async Task<bool> LogAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            if (!_auditSettings.EnableAuditLogging)
            {
                return true;
            }

            // Add to batch queue if batching is enabled
            if (_auditSettings.EnableBatchProcessing)
            {
                await _semaphore.WaitAsync();
                try
                {
                    _auditQueue.Enqueue(auditEvent);
                    
                    // Process immediately if queue is full
                    if (_auditQueue.Count >= _auditSettings.BatchSize)
                    {
                        await ProcessBatchInternalAsync();
                    }
                }
                finally
                {
                    _semaphore.Release();
                }
                return true;
            }

            // Send immediately if batching is disabled
            return await SendAuditEventAsync(auditEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit event {EventId}", auditEvent.EventId);
            return false;
        }
    }

    public async Task<bool> LogBatchAuditEventsAsync(IEnumerable<AuditEvent> auditEvents)
    {
        try
        {
            if (!_auditSettings.EnableAuditLogging)
            {
                return true;
            }

            var events = auditEvents.ToList();
            if (!events.Any())
            {
                return true;
            }

            var json = JsonSerializer.Serialize(events, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_auditSettings.BatchAuditEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully sent batch of {Count} audit events", events.Count);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to send batch audit events. Status: {StatusCode}", response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log batch audit events");
            return false;
        }
    }

    public string GenerateCorrelationId()
    {
        return $"gtw-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
    }

    public async Task<bool> LogAuthorizationEventAsync(AuthorizationAuditEvent authEvent)
    {
        try
        {
            authEvent.EventType = "Authorization";
            authEvent.Action = authEvent.AuthorizationResult ? "Allow" : "Deny";
            
            return await LogAuditEventAsync(authEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log authorization audit event");
            return false;
        }
    }

    public async Task<bool> LogHealthCheckEventAsync(HealthCheckAuditEvent healthEvent)
    {
        try
        {
            healthEvent.EventType = "HealthCheck";
            healthEvent.Action = "Monitor";
            
            return await LogAuditEventAsync(healthEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log health check audit event");
            return false;
        }
    }

    private async Task<bool> SendAuditEventAsync(AuditEvent auditEvent)
    {
        try
        {
            var json = JsonSerializer.Serialize(auditEvent, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_auditSettings.AuditEndpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Successfully sent audit event {EventId}", auditEvent.EventId);
                return true;
            }
            else
            {
                _logger.LogWarning("Failed to send audit event {EventId}. Status: {StatusCode}", 
                    auditEvent.EventId, response.StatusCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send audit event {EventId}", auditEvent.EventId);
            return false;
        }
    }

    private async void ProcessBatchAsync(object? state)
    {
        await _semaphore.WaitAsync();
        try
        {
            await ProcessBatchInternalAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task ProcessBatchInternalAsync()
    {
        if (_auditQueue.Count == 0)
        {
            return;
        }

        var batchEvents = new List<AuditEvent>();
        
        // Dequeue up to batch size
        int batchSize = Math.Min(_auditQueue.Count, _auditSettings.BatchSize);
        for (int i = 0; i < batchSize; i++)
        {
            if (_auditQueue.TryDequeue(out var auditEvent))
            {
                batchEvents.Add(auditEvent);
            }
        }

        if (batchEvents.Any())
        {
            await LogBatchAuditEventsAsync(batchEvents);
        }
    }

    public void Dispose()
    {
        _batchTimer?.Dispose();
        _semaphore?.Dispose();
    }
}