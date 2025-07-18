using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class ChangeRecord : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ChangeId { get; set; } = Guid.NewGuid().ToString();
    
    [StringLength(50)]
    public string? SessionId { get; set; }
    
    [StringLength(50)]
    public string? SyncId { get; set; }
    
    [StringLength(50)]
    public string? ReplicationId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    // Change Identification
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string RecordId { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? RecordIdentifiers { get; set; } // JSON object with primary key values
    
    [Required]
    public ChangeOperation Operation { get; set; }
    
    public ChangeSource ChangeSource { get; set; } = ChangeSource.User;
    
    // Change Data
    public string? OldDataJson { get; set; }
    public string? NewDataJson { get; set; }
    public string? DeltaJson { get; set; } // Only the changed fields
    
    [StringLength(1000)]
    public string? ChangedFields { get; set; } // JSON array of field names that changed
    
    // Timing Information
    public DateTime ChangeTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? DetectedTimestamp { get; set; }
    public DateTime? ProcessedTimestamp { get; set; }
    public DateTime? AppliedTimestamp { get; set; }
    
    // Source and Target Information
    [Required]
    [StringLength(50)]
    public string SourceSiteId { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? TargetSiteId { get; set; }
    
    [StringLength(50)]
    public string? SourceUser { get; set; }
    
    [StringLength(50)]
    public string? TargetUser { get; set; }
    
    [StringLength(100)]
    public string? SourceApplication { get; set; }
    
    [StringLength(100)]
    public string? TargetApplication { get; set; }
    
    // Status and Processing
    public ChangeStatus Status { get; set; } = ChangeStatus.Pending;
    
    public ProcessingPriority Priority { get; set; } = ProcessingPriority.Normal;
    
    // Error Handling
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    [StringLength(5000)]
    public string? ErrorDetails { get; set; }
    
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    public DateTime? LastRetryTime { get; set; }
    
    // Data Integrity
    [StringLength(100)]
    public string? ChecksumHash { get; set; }
    
    [StringLength(100)]
    public string? SourceChecksum { get; set; }
    
    [StringLength(100)]
    public string? TargetChecksum { get; set; }
    
    public bool IntegrityVerified { get; set; } = false;
    
    // Sequencing and Ordering
    public long? SequenceNumber { get; set; }
    public long? GlobalSequenceNumber { get; set; }
    
    [StringLength(50)]
    public string? BatchId { get; set; }
    
    public int? BatchSequence { get; set; }
    
    // Versioning
    [StringLength(50)]
    public string? SourceVersion { get; set; }
    
    [StringLength(50)]
    public string? TargetVersion { get; set; }
    
    public long? SourceRowVersion { get; set; }
    public long? TargetRowVersion { get; set; }
    
    // Dependencies
    [StringLength(1000)]
    public string? DependentChangeIds { get; set; } // JSON array of change IDs this depends on
    
    [StringLength(1000)]
    public string? PrerequisiteChangeIds { get; set; } // JSON array of prerequisite changes
    
    public bool HasDependencies { get; set; } = false;
    public bool DependenciesResolved { get; set; } = false;
    
    // Conflict Information
    public bool HasConflicts { get; set; } = false;
    public int ConflictCount { get; set; } = 0;
    
    [StringLength(50)]
    public string? PrimaryConflictId { get; set; }
    
    // Transformation and Mapping
    [StringLength(1000)]
    public string? TransformationRules { get; set; } // JSON array of transformation rules applied
    
    [StringLength(1000)]
    public string? FieldMappings { get; set; } // JSON object for field mappings
    
    public bool TransformationApplied { get; set; } = false;
    public bool MappingApplied { get; set; } = false;
    
    // Validation
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array of validation rules
    
    [StringLength(1000)]
    public string? ValidationErrors { get; set; } // JSON array of validation errors
    
    public bool ValidationPassed { get; set; } = false;
    public bool ValidationRequired { get; set; } = true;
    
    // Business Context
    [StringLength(100)]
    public string? BusinessTransaction { get; set; }
    
    [StringLength(100)]
    public string? BusinessProcess { get; set; }
    
    [StringLength(1000)]
    public string? BusinessReason { get; set; }
    
    // Audit and Compliance
    public bool RequiresAudit { get; set; } = false;
    
    [StringLength(1000)]
    public string? AuditTrail { get; set; } // JSON array of audit events
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    public bool ComplianceVerified { get; set; } = false;
    
    // Performance Metrics
    public TimeSpan? ProcessingDuration { get; set; }
    public long? DataSizeBytes { get; set; }
    public decimal? ProcessingCost { get; set; }
    
    // JSON properties for flexible data storage
    public string? MetadataJson { get; set; }
    public string? ConfigurationJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? RecordIdentifiersObject
    {
        get => string.IsNullOrEmpty(RecordIdentifiers) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(RecordIdentifiers);
        set => RecordIdentifiers = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? ChangedFieldsList
    {
        get => string.IsNullOrEmpty(ChangedFields) ? null : JsonConvert.DeserializeObject<List<string>>(ChangedFields);
        set => ChangedFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DependentChangeIdsList
    {
        get => string.IsNullOrEmpty(DependentChangeIds) ? null : JsonConvert.DeserializeObject<List<string>>(DependentChangeIds);
        set => DependentChangeIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? PrerequisiteChangeIdsList
    {
        get => string.IsNullOrEmpty(PrerequisiteChangeIds) ? null : JsonConvert.DeserializeObject<List<string>>(PrerequisiteChangeIds);
        set => PrerequisiteChangeIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? TransformationRulesList
    {
        get => string.IsNullOrEmpty(TransformationRules) ? null : JsonConvert.DeserializeObject<List<object>>(TransformationRules);
        set => TransformationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, string>? FieldMappingsObject
    {
        get => string.IsNullOrEmpty(FieldMappings) ? null : JsonConvert.DeserializeObject<Dictionary<string, string>>(FieldMappings);
        set => FieldMappings = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationErrorsList
    {
        get => string.IsNullOrEmpty(ValidationErrors) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationErrors);
        set => ValidationErrors = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AuditTrailList
    {
        get => string.IsNullOrEmpty(AuditTrail) ? null : JsonConvert.DeserializeObject<List<object>>(AuditTrail);
        set => AuditTrail = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsProcessed => Status == ChangeStatus.Applied || Status == ChangeStatus.Failed;
    public bool IsSuccessful => Status == ChangeStatus.Applied;
    public bool IsFailed => Status == ChangeStatus.Failed;
    public bool CanRetry => IsFailed && RetryCount < MaxRetries;
    public bool IsStale => ChangeTimestamp < DateTime.UtcNow.AddHours(-24) && Status == ChangeStatus.Pending;
    
    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - ChangeTimestamp;
    }
    
    public decimal GetSuccessRate()
    {
        if (RetryCount == 0) return IsSuccessful ? 100 : 0;
        return IsSuccessful ? 100 : 0;
    }
    
    // Navigation properties
    public virtual SyncSession? SyncSession { get; set; }
    public virtual Sync? Sync { get; set; }
    public virtual Replication? Replication { get; set; }
    public virtual SyncSite SourceSite { get; set; } = null!;
    public virtual SyncSite? TargetSite { get; set; }
    public virtual ICollection<SyncConflict> RelatedConflicts { get; set; } = new List<SyncConflict>();
}

public enum ChangeSource
{
    User = 1,
    System = 2,
    Application = 3,
    Import = 4,
    Sync = 5,
    Migration = 6,
    Automated = 7,
    External = 8
}

public enum ChangeStatus
{
    Pending = 1,
    Validated = 2,
    Queued = 3,
    Processing = 4,
    Applied = 5,
    Failed = 6,
    Retrying = 7,
    Skipped = 8,
    Cancelled = 9,
    Conflicted = 10
}

public enum ProcessingPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Critical = 4,
    Immediate = 5
}