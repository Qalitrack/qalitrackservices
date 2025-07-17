namespace QaliTrackGateway.Configuration;

/// <summary>
/// Configuration settings for service monitoring
/// </summary>
public class MonitoringSettings
{
    public const string SectionName = "Monitoring";

    /// <summary>
    /// Enable service health monitoring
    /// </summary>
    public bool EnableHealthMonitoring { get; set; } = true;

    /// <summary>
    /// Default health check interval in seconds
    /// </summary>
    public int DefaultHealthCheckIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Default timeout for health checks in seconds
    /// </summary>
    public int DefaultTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Response time threshold for degraded status in milliseconds
    /// </summary>
    public int DegradedResponseTimeMs { get; set; } = 5000;

    /// <summary>
    /// Maximum consecutive failures before marking service as unhealthy
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 3;

    /// <summary>
    /// Enable health check audit logging
    /// </summary>
    public bool EnableHealthCheckAudit { get; set; } = false;

    /// <summary>
    /// Health check audit interval in seconds
    /// </summary>
    public int HealthCheckAuditIntervalSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum health history records per service
    /// </summary>
    public int MaxHealthHistoryRecords { get; set; } = 1000;

    /// <summary>
    /// Health data retention period in hours
    /// </summary>
    public int HealthDataRetentionHours { get; set; } = 168; // 7 days

    /// <summary>
    /// Enable service dependency tracking
    /// </summary>
    public bool EnableDependencyTracking { get; set; } = true;

    /// <summary>
    /// Enable performance metrics collection
    /// </summary>
    public bool EnablePerformanceMetrics { get; set; } = true;

    /// <summary>
    /// Enable alerting for unhealthy services
    /// </summary>
    public bool EnableAlerting { get; set; } = true;

    /// <summary>
    /// Alert threshold for system health score
    /// </summary>
    public double AlertHealthScoreThreshold { get; set; } = 80.0;

    /// <summary>
    /// Enable automatic service discovery
    /// </summary>
    public bool EnableAutoDiscovery { get; set; } = false;

    /// <summary>
    /// Auto-discovery service patterns (comma-separated)
    /// </summary>
    public string AutoDiscoveryPatterns { get; set; } = "*-service";

    /// <summary>
    /// Enable circuit breaker pattern
    /// </summary>
    public bool EnableCircuitBreaker { get; set; } = true;

    /// <summary>
    /// Circuit breaker failure threshold
    /// </summary>
    public int CircuitBreakerFailureThreshold { get; set; } = 5;

    /// <summary>
    /// Circuit breaker timeout in seconds
    /// </summary>
    public int CircuitBreakerTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Enable load balancing based on health
    /// </summary>
    public bool EnableHealthBasedLoadBalancing { get; set; } = true;

    /// <summary>
    /// Default service registration settings
    /// </summary>
    public ServiceRegistrationDefaults ServiceDefaults { get; set; } = new();
}

/// <summary>
/// Default settings for service registration
/// </summary>
public class ServiceRegistrationDefaults
{
    /// <summary>
    /// Default environment
    /// </summary>
    public string Environment { get; set; } = "development";

    /// <summary>
    /// Default health check path
    /// </summary>
    public string HealthCheckPath { get; set; } = "/health";

    /// <summary>
    /// Default service port
    /// </summary>
    public int DefaultPort { get; set; } = 80;

    /// <summary>
    /// Default service protocol
    /// </summary>
    public string DefaultProtocol { get; set; } = "http";

    /// <summary>
    /// Default tags to apply to all services
    /// </summary>
    public string[] DefaultTags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Auto-register services found during discovery
    /// </summary>
    public bool AutoRegisterDiscoveredServices { get; set; } = true;
}