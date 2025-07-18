namespace QaliTrackGateway.Models;

/// <summary>
/// Audit event model for gateway-level logging
/// </summary>
public class AuditEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long? ResponseTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
}

/// <summary>
/// Authorization-specific audit event
/// </summary>
public class AuthorizationAuditEvent : AuditEvent
{
    public string[] RequiredRoles { get; set; } = Array.Empty<string>();
    public string[] RequiredPermissions { get; set; } = Array.Empty<string>();
    public string[] UserRoles { get; set; } = Array.Empty<string>();
    public string[] UserPermissions { get; set; } = Array.Empty<string>();
    public bool AuthorizationResult { get; set; }
    public string AuthorizationReason { get; set; } = string.Empty;
    public string CacheHit { get; set; } = "No";
}

/// <summary>
/// Health check audit event
/// </summary>
public class HealthCheckAuditEvent : AuditEvent
{
    public string[] CheckedServices { get; set; } = Array.Empty<string>();
    public string[] HealthyServices { get; set; } = Array.Empty<string>();
    public string[] UnhealthyServices { get; set; } = Array.Empty<string>();
    public int TotalServices { get; set; }
    public int HealthyCount { get; set; }
    public double OverallHealthScore { get; set; }
}