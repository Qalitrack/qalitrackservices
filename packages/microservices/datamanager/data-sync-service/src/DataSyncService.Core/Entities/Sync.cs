using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class Sync : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string SyncName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public SyncType SyncType { get; set; }
    
    public SyncStatus Status { get; set; } = SyncStatus.Pending;
    
    // Orchestration Configuration
    public SyncMode Mode { get; set; } = SyncMode.Incremental;
    public SyncDirection Direction { get; set; } = SyncDirection.Bidirectional;
    public SyncPriority Priority { get; set; } = SyncPriority.Normal;
    
    [StringLength(50)]
    public string? SourceSiteId { get; set; }
    
    [StringLength(50)]
    public string? TargetSiteId { get; set; }
    
    [StringLength(1000)]
    public string? SourceFilter { get; set; } // JSON filter criteria
    
    [StringLength(1000)]
    public string? TargetFilter { get; set; } // JSON filter criteria
    
    // Scheduling
    public bool IsScheduled { get; set; } = false;
    
    [StringLength(100)]
    public string? CronExpression { get; set; }
    
    public DateTime? NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    public DateTime? LastSuccessfulRunDate { get; set; }
    
    public int SyncIntervalMinutes { get; set; } = 60;
    
    // Data Selection
    [StringLength(1000)]
    public string? TableSelection { get; set; } // JSON array of tables/entities to sync
    
    [StringLength(1000)]
    public string? FieldMappings { get; set; } // JSON object for field mappings
    
    [StringLength(1000)]
    public string? TransformationRules { get; set; } // JSON array of transformation rules
    
    public bool SyncSchema { get; set; } = false;
    public bool SyncData { get; set; } = true;
    public bool SyncIndexes { get; set; } = false;
    public bool SyncConstraints { get; set; } = false;
    
    // Conflict Resolution
    public ConflictResolutionStrategy ConflictResolutionStrategy { get; set; } = ConflictResolutionStrategy.LastWriteWins;
    
    [StringLength(1000)]
    public string? ConflictResolutionRules { get; set; } // JSON array of custom rules
    
    public bool AutoResolveConflicts { get; set; } = true;
    public bool NotifyOnConflicts { get; set; } = true;
    
    [StringLength(1000)]
    public string? ConflictNotificationEmails { get; set; } // JSON array
    
    // Performance Configuration
    public int BatchSize { get; set; } = 1000;
    public int MaxConcurrentBatches { get; set; } = 5;
    public int TimeoutMinutes { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMinutes { get; set; } = 5;
    
    // Bandwidth and Throttling
    public long? MaxBandwidthKbps { get; set; }
    public int? ThrottleDelayMs { get; set; }
    public bool EnableCompression { get; set; } = true;
    
    // Quality Control
    public bool EnableValidation { get; set; } = true;
    public bool EnableIntegrityChecks { get; set; } = true;
    public bool EnableChecksums { get; set; } = true;
    
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array
    
    public decimal? QualityThreshold { get; set; } = 95.0m; // Minimum success rate %
    
    // Monitoring and Alerting
    public bool EnableMonitoring { get; set; } = true;
    public bool EnableAlerting { get; set; } = true;
    
    [StringLength(1000)]
    public string? MonitoringMetrics { get; set; } // JSON array of metrics to track
    
    [StringLength(1000)]
    public string? AlertConditions { get; set; } // JSON array of alert conditions
    
    [StringLength(1000)]
    public string? AlertRecipients { get; set; } // JSON array of email addresses
    
    // Execution Statistics
    public int TotalExecutions { get; set; } = 0;
    public int SuccessfulExecutions { get; set; } = 0;
    public int FailedExecutions { get; set; } = 0;
    
    public TimeSpan? AverageExecutionDuration { get; set; }
    public TimeSpan? LastExecutionDuration { get; set; }
    
    public long TotalRecordsSynced { get; set; } = 0;
    public long TotalConflictsDetected { get; set; } = 0;
    public long TotalConflictsResolved { get; set; } = 0;
    
    // Error Handling
    [StringLength(1000)]
    public string? LastErrorMessage { get; set; }
    
    public DateTime? LastErrorDate { get; set; }
    public int ConsecutiveFailures { get; set; } = 0;
    
    public bool PauseOnError { get; set; } = false;
    public bool EnableErrorRecovery { get; set; } = true;
    
    // Security and Compliance
    public bool EncryptInTransit { get; set; } = true;
    public bool EncryptAtRest { get; set; } = false;
    
    [StringLength(100)]
    public string? EncryptionMethod { get; set; }
    
    public bool RequireAuthentication { get; set; } = true;
    public bool EnableAuditLogging { get; set; } = true;
    
    [StringLength(1000)]
    public string? ComplianceRequirements { get; set; } // JSON array
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    [StringLength(1000)]
    public string? Tags { get; set; } // JSON array for search and categorization
    
    // Maintenance and Lifecycle
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    
    public DateTime? ArchivedDate { get; set; }
    
    [StringLength(50)]
    public string? ArchivedBy { get; set; }
    
    public DateTime? MaintenanceWindow { get; set; }
    public int? MaintenanceDurationMinutes { get; set; }
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? TableSelectionList
    {
        get => string.IsNullOrEmpty(TableSelection) ? null : JsonConvert.DeserializeObject<List<string>>(TableSelection);
        set => TableSelection = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, string>? FieldMappingsObject
    {
        get => string.IsNullOrEmpty(FieldMappings) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldMappings);
        set => FieldMappings = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? TransformationRulesList
    {
        get => string.IsNullOrEmpty(TransformationRules) ? null : JsonConvert.DeserializeObject<List<object>>(TransformationRules);
        set => TransformationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ConflictResolutionRulesList
    {
        get => string.IsNullOrEmpty(ConflictResolutionRules) ? null : JsonConvert.DeserializeObject<List<object>>(ConflictResolutionRules);
        set => ConflictResolutionRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ConflictNotificationEmailsList
    {
        get => string.IsNullOrEmpty(ConflictNotificationEmails) ? null : JsonConvert.DeserializeObject<List<string>>(ConflictNotificationEmails);
        set => ConflictNotificationEmails = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? MonitoringMetricsList
    {
        get => string.IsNullOrEmpty(MonitoringMetrics) ? null : JsonConvert.DeserializeObject<List<string>>(MonitoringMetrics);
        set => MonitoringMetrics = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AlertConditionsList
    {
        get => string.IsNullOrEmpty(AlertConditions) ? null : JsonConvert.DeserializeObject<List<object>>(AlertConditions);
        set => AlertConditions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AlertRecipientsList
    {
        get => string.IsNullOrEmpty(AlertRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(AlertRecipients);
        set => AlertRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ComplianceRequirementsList
    {
        get => string.IsNullOrEmpty(ComplianceRequirements) ? null : JsonConvert.DeserializeObject<List<string>>(ComplianceRequirements);
        set => ComplianceRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? TagsList
    {
        get => string.IsNullOrEmpty(Tags) ? null : JsonConvert.DeserializeObject<List<string>>(Tags);
        set => Tags = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public decimal GetSuccessRate()
    {
        if (TotalExecutions == 0) return 0;
        return (decimal)SuccessfulExecutions / TotalExecutions * 100;
    }
    
    public bool IsHealthy()
    {
        return Status == SyncStatus.Completed && ConsecutiveFailures == 0;
    }
    
    public bool RequiresAttention()
    {
        return ConsecutiveFailures >= 3 || 
               Status == SyncStatus.Failed || 
               (QualityThreshold.HasValue && GetSuccessRate() < QualityThreshold.Value);
    }
    
    // Navigation Properties
    public virtual ICollection<SyncSession> SyncSessions { get; set; } = new List<SyncSession>();
    public virtual ICollection<Replication> Replications { get; set; } = new List<Replication>();
    public virtual ICollection<SyncConflict> SyncConflicts { get; set; } = new List<SyncConflict>();
    public virtual ICollection<Status> StatusHistory { get; set; } = new List<Status>();
    public virtual SyncSite? SourceSite { get; set; }
    public virtual SyncSite? TargetSite { get; set; }
}

public enum SyncType
{
    DatabaseSync = 1,
    FileSync = 2,
    ServiceSync = 3,
    RealtimeSync = 4,
    BatchSync = 5,
    EventDrivenSync = 6,
    CustomSync = 7
}