using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class Replication : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string SyncId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string ReplicationName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Replication Strategy Configuration
    [Required]
    public ReplicationStrategy Strategy { get; set; }
    
    public ReplicationStatus Status { get; set; } = ReplicationStatus.Pending;
    
    public ReplicationMode Mode { get; set; } = ReplicationMode.Asynchronous;
    
    public ReplicationDirection Direction { get; set; } = ReplicationDirection.SourceToTarget;
    
    // Source Configuration
    [Required]
    [StringLength(50)]
    public string SourceSiteId { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? SourceConnectionString { get; set; }
    
    [StringLength(100)]
    public string? SourceDatabase { get; set; }
    
    [StringLength(100)]
    public string? SourceTable { get; set; }
    
    [StringLength(1000)]
    public string? SourceQuery { get; set; }
    
    [StringLength(1000)]
    public string? SourceFilter { get; set; } // JSON filter criteria
    
    // Target Configuration
    [Required]
    [StringLength(50)]
    public string TargetSiteId { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? TargetConnectionString { get; set; }
    
    [StringLength(100)]
    public string? TargetDatabase { get; set; }
    
    [StringLength(100)]
    public string? TargetTable { get; set; }
    
    [StringLength(1000)]
    public string? TargetQuery { get; set; }
    
    [StringLength(1000)]
    public string? TargetFilter { get; set; } // JSON filter criteria
    
    // Data Processing Configuration
    [StringLength(1000)]
    public string? FieldMappings { get; set; } // JSON object mapping source to target fields
    
    [StringLength(1000)]
    public string? TransformationRules { get; set; } // JSON array of transformation rules
    
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array of validation rules
    
    public bool EnableDataValidation { get; set; } = true;
    public bool EnableDataTransformation { get; set; } = false;
    public bool EnableDataCleansing { get; set; } = false;
    
    // Change Detection Configuration
    public ChangeDetectionMethod ChangeDetectionMethod { get; set; } = ChangeDetectionMethod.Timestamp;
    
    [StringLength(100)]
    public string? ChangeDetectionColumn { get; set; } // Column to track changes (timestamp, version, etc.)
    
    [StringLength(100)]
    public string? PrimaryKeyColumns { get; set; } // Comma-separated list of primary key columns
    
    public DateTime? LastChangeDetected { get; set; }
    public DateTime? LastReplicationRun { get; set; }
    
    // Performance Configuration
    public int BatchSize { get; set; } = 1000;
    public int MaxConcurrentBatches { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 300;
    public int RetryAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 30;
    
    // Bandwidth and Resource Management
    public long? MaxBandwidthBytesPerSecond { get; set; }
    public int? MaxMemoryUsageMB { get; set; }
    public int? MaxCpuUsagePercentage { get; set; }
    
    public bool EnableThrottling { get; set; } = false;
    public int? ThrottleDelayMs { get; set; }
    
    // Compression and Optimization
    public bool EnableCompression { get; set; } = true;
    public CompressionMethod CompressionMethod { get; set; } = CompressionMethod.GZip;
    
    public bool EnableBulkOperations { get; set; } = true;
    public bool EnableParallelProcessing { get; set; } = true;
    
    // Scheduling Configuration
    public bool IsScheduled { get; set; } = false;
    
    [StringLength(100)]
    public string? CronExpression { get; set; }
    
    public int? IntervalMinutes { get; set; }
    public DateTime? NextRunTime { get; set; }
    
    public bool RunOnStartup { get; set; } = false;
    public bool RunOnDataChange { get; set; } = true;
    
    // Quality Control
    public bool EnableIntegrityChecks { get; set; } = true;
    public bool EnableChecksumValidation { get; set; } = true;
    public bool EnableRowCountValidation { get; set; } = true;
    
    public decimal? AcceptableErrorRate { get; set; } = 1.0m; // Percentage
    public int? MaxErrorsBeforeStop { get; set; } = 100;
    
    // Error Handling
    public ErrorHandlingStrategy ErrorHandlingStrategy { get; set; } = ErrorHandlingStrategy.SkipAndContinue;
    
    public bool LogErrors { get; set; } = true;
    public bool NotifyOnErrors { get; set; } = true;
    
    [StringLength(1000)]
    public string? ErrorNotificationEmails { get; set; } // JSON array
    
    // Statistics and Monitoring
    public long TotalRecordsProcessed { get; set; } = 0;
    public long TotalRecordsSuccessful { get; set; } = 0;
    public long TotalRecordsFailed { get; set; } = 0;
    public long TotalRecordsSkipped { get; set; } = 0;
    
    public TimeSpan? LastExecutionDuration { get; set; }
    public TimeSpan? AverageExecutionDuration { get; set; }
    
    public long TotalBytesTransferred { get; set; } = 0;
    public decimal? AverageTransferRate { get; set; } // Bytes per second
    
    public int TotalExecutions { get; set; } = 0;
    public int SuccessfulExecutions { get; set; } = 0;
    public int FailedExecutions { get; set; } = 0;
    
    // Recent Execution Information
    public DateTime? LastExecutionStart { get; set; }
    public DateTime? LastExecutionEnd { get; set; }
    public ReplicationStatus LastExecutionStatus { get; set; } = ReplicationStatus.NotStarted;
    
    [StringLength(1000)]
    public string? LastExecutionError { get; set; }
    
    public long? LastExecutionRecordsProcessed { get; set; }
    public decimal? LastExecutionProgressPercentage { get; set; }
    
    // Security Configuration
    public bool EncryptInTransit { get; set; } = true;
    public bool EncryptAtRest { get; set; } = false;
    
    [StringLength(100)]
    public string? EncryptionAlgorithm { get; set; }
    
    public bool RequireAuthentication { get; set; } = true;
    public bool EnableAuditLogging { get; set; } = true;
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    public ReplicationPriority Priority { get; set; } = ReplicationPriority.Normal;
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    // Maintenance and Lifecycle
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    
    public DateTime? ArchivedDate { get; set; }
    
    [StringLength(50)]
    public string? ArchivedBy { get; set; }
    
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
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? SourceFilterObject
    {
        get => string.IsNullOrEmpty(SourceFilter) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(SourceFilter);
        set => SourceFilter = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? TargetFilterObject
    {
        get => string.IsNullOrEmpty(TargetFilter) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(TargetFilter);
        set => TargetFilter = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ErrorNotificationEmailsList
    {
        get => string.IsNullOrEmpty(ErrorNotificationEmails) ? null : JsonConvert.DeserializeObject<List<string>>(ErrorNotificationEmails);
        set => ErrorNotificationEmails = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public decimal GetSuccessRate()
    {
        if (TotalExecutions == 0) return 0;
        return (decimal)SuccessfulExecutions / TotalExecutions * 100;
    }
    
    public decimal GetDataSuccessRate()
    {
        if (TotalRecordsProcessed == 0) return 0;
        return (decimal)TotalRecordsSuccessful / TotalRecordsProcessed * 100;
    }
    
    public bool IsHealthy()
    {
        return Status == ReplicationStatus.Completed && 
               LastExecutionStatus == ReplicationStatus.Completed &&
               (AcceptableErrorRate == null || GetDataSuccessRate() >= (100 - AcceptableErrorRate.Value));
    }
    
    public TimeSpan? GetEstimatedTimeRemaining()
    {
        if (!LastExecutionStart.HasValue || LastExecutionProgressPercentage == null || LastExecutionProgressPercentage == 0)
            return null;
            
        var elapsed = DateTime.UtcNow - LastExecutionStart.Value;
        var progressRatio = LastExecutionProgressPercentage.Value / 100;
        
        if (progressRatio >= 1) return TimeSpan.Zero;
        
        var estimatedTotal = TimeSpan.FromTicks((long)(elapsed.Ticks / progressRatio));
        return estimatedTotal - elapsed;
    }
    
    // Navigation Properties
    public virtual Sync Sync { get; set; } = null!;
    public virtual SyncSite SourceSite { get; set; } = null!;
    public virtual SyncSite TargetSite { get; set; } = null!;
    public virtual ICollection<ChangeRecord> ChangeRecords { get; set; } = new List<ChangeRecord>();
    public virtual ICollection<SyncConflict> SyncConflicts { get; set; } = new List<SyncConflict>();
}

public enum ReplicationStrategy
{
    FullRefresh = 1,
    IncrementalUpdate = 2,
    DeltaSync = 3,
    MasterSlave = 4,
    MasterMaster = 5,
    PeerToPeer = 6,
    EventDriven = 7,
    CustomStrategy = 8
}

public enum ReplicationStatus
{
    NotStarted = 1,
    Pending = 2,
    Initializing = 3,
    Running = 4,
    Paused = 5,
    Completed = 6,
    Failed = 7,
    Cancelled = 8,
    PartialSuccess = 9,
    RequiresIntervention = 10
}

public enum ReplicationMode
{
    Synchronous = 1,
    Asynchronous = 2,
    SemiSynchronous = 3
}

public enum ReplicationDirection
{
    SourceToTarget = 1,
    TargetToSource = 2,
    Bidirectional = 3
}

public enum ChangeDetectionMethod
{
    Timestamp = 1,
    Version = 2,
    Checksum = 3,
    TriggerBased = 4,
    LogBased = 5,
    FullComparison = 6
}

public enum CompressionMethod
{
    None = 1,
    GZip = 2,
    Deflate = 3,
    LZ4 = 4,
    Snappy = 5
}

public enum ErrorHandlingStrategy
{
    StopOnError = 1,
    SkipAndContinue = 2,
    RetryAndStop = 3,
    RetryAndSkip = 4,
    Manual = 5
}

public enum ReplicationPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4
}