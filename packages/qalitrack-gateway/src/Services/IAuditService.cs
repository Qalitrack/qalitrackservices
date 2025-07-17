using QaliTrackGateway.Models;

namespace QaliTrackGateway.Services;

/// <summary>
/// Interface for audit service integration with transaction service
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Log audit event for API gateway access
    /// </summary>
    Task<bool> LogAuditEventAsync(AuditEvent auditEvent);
    
    /// <summary>
    /// Log batch audit events for performance optimization
    /// </summary>
    Task<bool> LogBatchAuditEventsAsync(IEnumerable<AuditEvent> auditEvents);
    
    /// <summary>
    /// Generate correlation ID for request tracking
    /// </summary>
    string GenerateCorrelationId();
    
    /// <summary>
    /// Log authorization event (success/failure)
    /// </summary>
    Task<bool> LogAuthorizationEventAsync(AuthorizationAuditEvent authEvent);
    
    /// <summary>
    /// Log service health check events
    /// </summary>
    Task<bool> LogHealthCheckEventAsync(HealthCheckAuditEvent healthEvent);
}