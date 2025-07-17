using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class Status : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? SyncId { get; set; }
    
    [StringLength(50)]
    public string? ReplicationId { get; set; }
    
    [StringLength(50)]
    public string? SessionId { get; set; }
    
    [StringLength(50)]
    public string? SiteId { get; set; }
    
    // Status Information
    [Required]
    public StatusType StatusType { get; set; }
    
    [Required]
    public StatusLevel StatusLevel { get; set; }
    
    [Required]
    [StringLength(100)]
    public string StatusName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? StatusDescription { get; set; }
    
    [StringLength(1000)]
    public string? StatusDetails { get; set; }
    
    // Timing Information
    public DateTime StatusTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? StatusStartTime { get; set; }
    public DateTime? StatusEndTime { get; set; }
    
    public TimeSpan? Duration
    {
        get => StatusStartTime.HasValue && StatusEndTime.HasValue ? StatusEndTime.Value - StatusStartTime.Value : null;
    }
    
    // Source Information
    [Required]
    [StringLength(100)]
    public string Source { get; set; } = string.Empty; // Service, Component, or System that reported the status
    
    [StringLength(100)]
    public string? SourceVersion { get; set; }
    
    [StringLength(100)]
    public string? SourceLocation { get; set; } // Server, Node, or Location
    
    // Performance Metrics
    public decimal? ProgressPercentage { get; set; }
    
    [StringLength(500)]
    public string? ProgressMessage { get; set; }
    
    public long? RecordsProcessed { get; set; }
    public long? TotalRecords { get; set; }
    public long? BytesProcessed { get; set; }
    public long? TotalBytes { get; set; }
    
    // Resource Usage
    public long? MemoryUsedBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    public long? DiskUsedBytes { get; set; }
    public long? NetworkBytesTransferred { get; set; }
    
    // Quality Metrics
    public decimal? DataQualityScore { get; set; }
    public int? ErrorCount { get; set; }
    public int? WarningCount { get; set; }
    public decimal? SuccessRate { get; set; }
    
    // Throughput Metrics
    public decimal? RecordsPerSecond { get; set; }
    public decimal? BytesPerSecond { get; set; }
    public decimal? TransactionsPerSecond { get; set; }
    
    // Health Indicators
    public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;
    
    [StringLength(1000)]
    public string? HealthIndicators { get; set; } // JSON array of health indicators
    
    public decimal? HealthScore { get; set; } // 0-100 health score
    
    // Business Context
    [StringLength(100)]
    public string? BusinessProcess { get; set; }
    
    [StringLength(100)]
    public string? BusinessUnit { get; set; }
    
    [StringLength(1000)]
    public string? BusinessImpact { get; set; }
    
    // Alert Information
    public bool IsAlert { get; set; } = false;
    public AlertSeverity? AlertSeverity { get; set; }
    
    [StringLength(1000)]
    public string? AlertMessage { get; set; }
    
    public bool AlertAcknowledged { get; set; } = false;
    
    [StringLength(50)]
    public string? AlertAcknowledgedBy { get; set; }
    
    public DateTime? AlertAcknowledgedAt { get; set; }
    
    // Notification Information
    public bool NotificationSent { get; set; } = false;
    public DateTime? NotificationSentAt { get; set; }
    
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array
    
    // Error Information
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    [StringLength(5000)]
    public string? ErrorDetails { get; set; }
    
    [StringLength(2000)]
    public string? StackTrace { get; set; }
    
    // Recovery Information
    public bool RequiresIntervention { get; set; } = false;
    public bool AutoRecoveryAttempted { get; set; } = false;
    public bool AutoRecoverySuccessful { get; set; } = false;
    
    [StringLength(1000)]
    public string? RecoveryActions { get; set; } // JSON array of recovery actions
    
    public int RecoveryAttempts { get; set; } = 0;
    public DateTime? LastRecoveryAttempt { get; set; }
    
    // Correlation Information
    [StringLength(50)]
    public string? CorrelationId { get; set; }
    
    [StringLength(50)]
    public string? ParentStatusId { get; set; }
    
    [StringLength(1000)]
    public string? RelatedStatusIds { get; set; } // JSON array of related status IDs
    
    // Trend Analysis
    public TrendDirection? TrendDirection { get; set; }
    public decimal? TrendValue { get; set; }
    
    [StringLength(500)]
    public string? TrendAnalysis { get; set; }
    
    // Compliance and Audit
    public bool RequiresAudit { get; set; } = false;
    
    [StringLength(1000)]
    public string? ComplianceNotes { get; set; }
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    // Custom Fields
    [StringLength(1000)]
    public string? CustomFields { get; set; } // JSON object for custom fields
    
    [StringLength(1000)]
    public string? Tags { get; set; } // JSON array for tagging and categorization
    
    // Data Retention
    public DateTime? ExpirationDate { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedDate { get; set; }
    
    // JSON properties for flexible data storage
    public string? MetricsJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Metrics
    {
        get => string.IsNullOrEmpty(MetricsJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetricsJson);
        set => MetricsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? HealthIndicatorsList
    {
        get => string.IsNullOrEmpty(HealthIndicators) ? null : JsonConvert.DeserializeObject<List<object>>(HealthIndicators);
        set => HealthIndicators = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? RecoveryActionsList
    {
        get => string.IsNullOrEmpty(RecoveryActions) ? null : JsonConvert.DeserializeObject<List<object>>(RecoveryActions);
        set => RecoveryActions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RelatedStatusIdsList
    {
        get => string.IsNullOrEmpty(RelatedStatusIds) ? null : JsonConvert.DeserializeObject<List<string>>(RelatedStatusIds);
        set => RelatedStatusIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? CustomFieldsObject
    {
        get => string.IsNullOrEmpty(CustomFields) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(CustomFields);
        set => CustomFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? TagsList
    {
        get => string.IsNullOrEmpty(Tags) ? null : JsonConvert.DeserializeObject<List<string>>(Tags);
        set => Tags = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsActive => StatusType == StatusType.Active || StatusType == StatusType.InProgress;
    public bool IsCompleted => StatusType == StatusType.Completed || StatusType == StatusType.Success;
    public bool IsFailed => StatusType == StatusType.Failed || StatusType == StatusType.Error;
    public bool IsHealthy => HealthStatus == HealthStatus.Healthy;
    public bool RequiresAttention => IsAlert || RequiresIntervention || HealthStatus == HealthStatus.Unhealthy;
    
    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - StatusTimestamp;
    }
    
    public decimal GetCompletionPercentage()
    {
        if (TotalRecords.HasValue && TotalRecords.Value > 0 && RecordsProcessed.HasValue)
        {
            return (decimal)RecordsProcessed.Value / TotalRecords.Value * 100;
        }
        return ProgressPercentage ?? 0;
    }
    
    public bool IsStale(TimeSpan threshold)
    {
        return GetAge() > threshold && IsActive;
    }
    
    // Navigation Properties
    public virtual Sync? Sync { get; set; }
    public virtual Replication? Replication { get; set; }
    public virtual SyncSession? SyncSession { get; set; }
    public virtual SyncSite? Site { get; set; }
    public virtual Status? ParentStatus { get; set; }
    public virtual ICollection<Status> ChildStatuses { get; set; } = new List<Status>();
}

public enum StatusType
{
    Created = 1,
    Initialized = 2,
    Queued = 3,
    Active = 4,
    InProgress = 5,
    Paused = 6,
    Resumed = 7,
    Completed = 8,
    Success = 9,
    Failed = 10,
    Error = 11,
    Cancelled = 12,
    Timeout = 13,
    Warning = 14,
    Information = 15,
    Debug = 16
}

public enum StatusLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5,
    Fatal = 6
}

public enum HealthStatus
{
    Unknown = 0,
    Healthy = 1,
    Warning = 2,
    Unhealthy = 3,
    Critical = 4,
    Degraded = 5
}

public enum AlertSeverity
{
    Information = 1,
    Warning = 2,
    Minor = 3,
    Major = 4,
    Critical = 5
}

public enum TrendDirection
{
    Stable = 0,
    Improving = 1,
    Degrading = -1,
    Volatile = 2
}