using System.ComponentModel.DataAnnotations;
using DataSyncService.Core.Enums;
using Newtonsoft.Json;

namespace DataSyncService.Core.Entities;

public class SyncConflict : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ConflictId { get; set; } = Guid.NewGuid().ToString();
    
    [StringLength(50)]
    public string? SessionId { get; set; }
    
    [StringLength(50)]
    public string? SyncId { get; set; }
    
    [StringLength(50)]
    public string? ReplicationId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    // Conflict Identification
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string RecordId { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? RecordIdentifiers { get; set; } // JSON object with primary key values
    
    [Required]
    public ConflictStatus Status { get; set; } = ConflictStatus.Detected;
    
    [Required]
    public ConflictType ConflictType { get; set; }
    
    public ConflictSeverity Severity { get; set; } = ConflictSeverity.Medium;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [StringLength(1000)]
    public string? ConflictDetails { get; set; } // Detailed explanation of the conflict
    
    // Data Information
    [Required]
    public string SourceDataJson { get; set; } = string.Empty;
    
    [Required]
    public string TargetDataJson { get; set; } = string.Empty;
    
    public string? ResolvedDataJson { get; set; }
    public string? OriginalDataJson { get; set; } // Data before any changes
    
    [StringLength(1000)]
    public string? ConflictingFields { get; set; } // JSON array of field names that conflict
    
    // Source and Target Information
    [Required]
    [StringLength(50)]
    public string SourceSiteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string TargetSiteId { get; set; } = string.Empty;
    
    public DateTime SourceTimestamp { get; set; }
    public DateTime TargetTimestamp { get; set; }
    
    [StringLength(50)]
    public string? SourceVersion { get; set; }
    
    [StringLength(50)]
    public string? TargetVersion { get; set; }
    
    [StringLength(50)]
    public string? SourceUser { get; set; }
    
    [StringLength(50)]
    public string? TargetUser { get; set; }
    
    // Resolution Information
    public ConflictResolutionStrategy ResolutionStrategy { get; set; } = ConflictResolutionStrategy.Manual;
    
    public ResolutionAlgorithm? ResolutionAlgorithm { get; set; }
    
    [StringLength(1000)]
    public string? ResolutionRules { get; set; } // JSON array of rules applied
    
    [StringLength(1000)]
    public string? ResolutionReason { get; set; }
    
    [StringLength(50)]
    public string? ResolvedBy { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    public TimeSpan? ResolutionDuration
    {
        get => ResolvedAt.HasValue ? ResolvedAt.Value - ConflictDetectedAt : null;
    }
    
    // Auto-Resolution Configuration
    public bool AutoResolutionAttempted { get; set; } = false;
    public bool AutoResolutionSuccessful { get; set; } = false;
    
    [StringLength(1000)]
    public string? AutoResolutionError { get; set; }
    
    public int AutoResolutionAttempts { get; set; } = 0;
    public int MaxAutoResolutionAttempts { get; set; } = 3;
    
    // Priority and Escalation
    public ConflictPriority Priority { get; set; } = ConflictPriority.Medium;
    
    public bool RequiresEscalation { get; set; } = false;
    public bool IsEscalated { get; set; } = false;
    
    [StringLength(50)]
    public string? EscalatedTo { get; set; }
    
    public DateTime? EscalatedAt { get; set; }
    
    [StringLength(1000)]
    public string? EscalationReason { get; set; }
    
    // Timing Information
    public DateTime ConflictDetectedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? FirstNotificationSent { get; set; }
    public DateTime? LastNotificationSent { get; set; }
    public int NotificationCount { get; set; } = 0;
    
    // Business Impact Assessment
    public BusinessImpact BusinessImpact { get; set; } = BusinessImpact.Low;
    
    [StringLength(1000)]
    public string? BusinessImpactDescription { get; set; }
    
    [StringLength(100)]
    public string? AffectedBusinessProcess { get; set; }
    
    [StringLength(1000)]
    public string? AffectedUsers { get; set; } // JSON array of affected users
    
    // Quality and Validation
    public decimal? DataQualityScore { get; set; }
    
    [StringLength(1000)]
    public string? ValidationErrors { get; set; } // JSON array of validation issues
    
    public bool RequiresDataValidation { get; set; } = true;
    public bool ValidationCompleted { get; set; } = false;
    
    // Audit and Compliance
    public bool RequiresAuditTrail { get; set; } = true;
    
    [StringLength(1000)]
    public string? AuditTrail { get; set; } // JSON array of audit events
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    public bool RequiresManagerialApproval { get; set; } = false;
    public bool ManagerialApprovalReceived { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    // Error Handling
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    [StringLength(5000)]
    public string? ErrorDetails { get; set; }
    
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    
    public DateTime? LastRetryAt { get; set; }
    
    // Communication and Notifications
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? EscalationRecipients { get; set; } // JSON array
    
    public bool NotificationsSent { get; set; } = false;
    public bool EscalationNotificationsSent { get; set; } = false;
    
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
    
    public List<string>? ConflictingFieldsList
    {
        get => string.IsNullOrEmpty(ConflictingFields) ? null : JsonConvert.DeserializeObject<List<string>>(ConflictingFields);
        set => ConflictingFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ResolutionRulesList
    {
        get => string.IsNullOrEmpty(ResolutionRules) ? null : JsonConvert.DeserializeObject<List<object>>(ResolutionRules);
        set => ResolutionRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AffectedUsersList
    {
        get => string.IsNullOrEmpty(AffectedUsers) ? null : JsonConvert.DeserializeObject<List<string>>(AffectedUsers);
        set => AffectedUsers = value == null ? null : JsonConvert.SerializeObject(value);
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
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? EscalationRecipientsList
    {
        get => string.IsNullOrEmpty(EscalationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(EscalationRecipients);
        set => EscalationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsResolved => Status == ConflictStatus.Resolved;
    public bool IsActive => Status == ConflictStatus.Detected || Status == ConflictStatus.InResolution;
    public bool RequiresManualIntervention => ResolutionStrategy == ConflictResolutionStrategy.Manual || IsEscalated;
    public bool IsStale => ConflictDetectedAt < DateTime.UtcNow.AddHours(-24) && Status == ConflictStatus.Detected;
    
    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - ConflictDetectedAt;
    }
    
    public bool ShouldEscalate()
    {
        var age = GetAge();
        return !IsEscalated && 
               (age.TotalHours > 4 && Priority == ConflictPriority.High) ||
               (age.TotalHours > 8 && Priority == ConflictPriority.Medium) ||
               (age.TotalDays > 1 && Priority == ConflictPriority.Low);
    }
    
    // Navigation properties
    public virtual SyncSession? SyncSession { get; set; }
    public virtual Sync? Sync { get; set; }
    public virtual Replication? Replication { get; set; }
    public virtual SyncSite SourceSite { get; set; } = null!;
    public virtual SyncSite TargetSite { get; set; } = null!;
    public virtual ICollection<ChangeRecord> RelatedChanges { get; set; } = new List<ChangeRecord>();
}

public enum ConflictType
{
    DataConflict = 1,
    VersionConflict = 2,
    TimestampConflict = 3,
    DeleteConflict = 4,
    StructureConflict = 5,
    ConstraintConflict = 6,
    PermissionConflict = 7,
    BusinessRuleConflict = 8,
    CustomConflict = 9
}

public enum ConflictSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ResolutionAlgorithm
{
    LastWriteWins = 1,
    FirstWriteWins = 2,
    HighestPriority = 3,
    MergeFields = 4,
    UserPreference = 5,
    BusinessRuleBased = 6,
    VersionBased = 7,
    TimestampBased = 8,
    SourcePreference = 9,
    TargetPreference = 10,
    CustomAlgorithm = 11
}

public enum ConflictPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum BusinessImpact
{
    None = 1,
    Low = 2,
    Medium = 3,
    High = 4,
    Critical = 5
}