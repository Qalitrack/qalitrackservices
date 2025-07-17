namespace QaliTrackGateway.Configuration;

/// <summary>
/// Configuration settings for audit service integration
/// </summary>
public class AuditSettings
{
    public const string SectionName = "Audit";

    /// <summary>
    /// Enable/disable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Transaction service base URL
    /// </summary>
    public string TransactionServiceBaseUrl { get; set; } = "http://transaction-service";

    /// <summary>
    /// Audit endpoint for single events
    /// </summary>
    public string AuditEndpoint { get; set; } = "/api/audit/gateway";

    /// <summary>
    /// Batch audit endpoint for multiple events
    /// </summary>
    public string BatchAuditEndpoint { get; set; } = "/api/audit/gateway/batch";

    /// <summary>
    /// Enable batch processing for performance optimization
    /// </summary>
    public bool EnableBatchProcessing { get; set; } = true;

    /// <summary>
    /// Batch size for audit events
    /// </summary>
    public int BatchSize { get; set; } = 50;

    /// <summary>
    /// Batch processing interval in seconds
    /// </summary>
    public int BatchIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// HTTP client timeout in seconds
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Retry attempts for failed audit requests
    /// </summary>
    public int RetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Enable audit logging for health checks
    /// </summary>
    public bool EnableHealthCheckAudit { get; set; } = false;

    /// <summary>
    /// Enable audit logging for authorization events
    /// </summary>
    public bool EnableAuthorizationAudit { get; set; } = true;

    /// <summary>
    /// Enable audit logging for API gateway requests
    /// </summary>
    public bool EnableGatewayRequestAudit { get; set; } = true;

    /// <summary>
    /// Minimum log level for audit events
    /// </summary>
    public string MinimumLogLevel { get; set; } = "Information";

    /// <summary>
    /// Enable correlation ID generation
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Enable audit event filtering
    /// </summary>
    public bool EnableEventFiltering { get; set; } = true;

    /// <summary>
    /// Exclude paths from audit logging (comma-separated)
    /// </summary>
    public string ExcludePaths { get; set; } = "/health,/swagger,/metrics";

    /// <summary>
    /// Include only specific HTTP methods (comma-separated, empty = all)
    /// </summary>
    public string IncludeHttpMethods { get; set; } = "";

    /// <summary>
    /// Computed property for excluded paths
    /// </summary>
    public string[] ExcludedPaths => ExcludePaths.Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(p => p.Trim().ToLowerInvariant())
        .ToArray();

    /// <summary>
    /// Computed property for included HTTP methods
    /// </summary>
    public string[] IncludedHttpMethods => string.IsNullOrEmpty(IncludeHttpMethods) ? 
        Array.Empty<string>() : 
        IncludeHttpMethods.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(m => m.Trim().ToUpperInvariant())
            .ToArray();
}