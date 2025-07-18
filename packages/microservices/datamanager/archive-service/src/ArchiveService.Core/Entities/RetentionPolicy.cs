using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArchiveService.Core.Entities;

public class RetentionPolicy : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(100)]
    public string EntityType { get; set; } = string.Empty; // TRANSACTIONS, WEIGHT_MEASUREMENTS, COMPLIANCE_RECORDS
    
    [StringLength(100)]
    public string? Category { get; set; } // Optional category filter
    
    public RetentionPolicyType PolicyType { get; set; } = RetentionPolicyType.Standard;
    public RetentionPolicyStatus Status { get; set; } = RetentionPolicyStatus.Active;
    
    // Retention Configuration
    public int RetentionDays { get; set; } // How long to keep in operational database
    public int ArchiveAfterDays { get; set; } // When to archive data
    public int DeleteAfterDays { get; set; } // When to delete archived data (0 = never delete)
    
    public DateTime? RetentionStartDate { get; set; }
    public DateTime? RetentionEndDate { get; set; }
    
    // Tiered Storage Configuration
    public StorageTier StorageTier { get; set; } = StorageTier.Hot;
    public CompressionType CompressionType { get; set; } = CompressionType.GZip;
    
    public int? WarmTierAfterDays { get; set; }
    public int? ColdTierAfterDays { get; set; }
    public int? DeepArchiveAfterDays { get; set; }
    
    // Execution Configuration
    public bool IsActive { get; set; } = true;
    public bool IsAutomatic { get; set; } = true; // Whether to execute automatically
    
    public ExecutionSchedule ExecutionSchedule { get; set; } = ExecutionSchedule.Daily;
    
    [StringLength(100)]
    public string? CronExpression { get; set; }
    
    public DateTime? LastExecuted { get; set; }
    public DateTime? NextExecution { get; set; }
    
    public int ExecutionTimeoutMinutes { get; set; } = 120;
    public int MaxRetryAttempts { get; set; } = 3;
    
    // Filtering and Selection
    [StringLength(2000)]
    public string? FilterCriteria { get; set; } // JSON filter criteria
    
    [StringLength(1000)]
    public string? InclusionRules { get; set; } // JSON array of inclusion rules
    
    [StringLength(1000)]
    public string? ExclusionRules { get; set; } // JSON array of exclusion rules
    
    [StringLength(1000)]
    public string? OrganizationIds { get; set; } // JSON array of org IDs (empty = all)
    
    // Data Processing
    public bool AnonymizeData { get; set; } = false;
    
    [StringLength(1000)]
    public string? AnonymizationRules { get; set; } // JSON array of anonymization rules
    
    public bool PurgePersonalData { get; set; } = false;
    public bool MaintainStatisticalData { get; set; } = true;
    
    // Performance Configuration
    public int BatchSize { get; set; } = 1000;
    public int MaxConcurrentBatches { get; set; } = 3;
    
    public bool EnableParallelProcessing { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 4;
    
    // Quality and Validation
    public bool EnableDataValidation { get; set; } = true;
    public bool RequireIntegrityChecks { get; set; } = true;
    
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array of validation rules
    
    public decimal? AcceptableErrorRate { get; set; } = 1.0m; // Percentage
    
    // Compliance Configuration
    public bool RequiresComplianceReview { get; set; } = false;
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; } // GDPR, CCPA, SOX, etc.
    
    [StringLength(1000)]
    public string? RegulatoryRequirements { get; set; } // JSON array of regulatory requirements
    
    public bool IsSubjectToLegalHold { get; set; } = false;
    
    [StringLength(1000)]
    public string? LegalHoldExemptions { get; set; } // JSON array of exemption criteria
    
    // Approval Workflow
    public bool RequiresApproval { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovalDate { get; set; }
    
    [StringLength(1000)]
    public string? ApprovalComments { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? DataSteward { get; set; }
    
    [StringLength(100)]
    public string? ComplianceOfficer { get; set; }
    
    public PolicyPriority Priority { get; set; } = PolicyPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    // Risk Assessment
    public DataSensitivity DataSensitivity { get; set; } = DataSensitivity.Internal;
    public BusinessRisk BusinessRisk { get; set; } = BusinessRisk.Low;
    public ComplianceRisk ComplianceRisk { get; set; } = ComplianceRisk.Low;
    
    [StringLength(1000)]
    public string? RiskMitigationStrategies { get; set; } // JSON array
    
    // Monitoring and Alerting
    public bool EnableMonitoring { get; set; } = true;
    public bool EnableAlerting { get; set; } = true;
    
    [StringLength(1000)]
    public string? AlertRecipients { get; set; } // JSON array of email addresses
    
    public bool AlertOnFailure { get; set; } = true;
    public bool AlertOnSuccess { get; set; } = false;
    public bool AlertOnNonCompliance { get; set; } = true;
    
    // Execution Statistics
    public int TotalExecutions { get; set; } = 0;
    public int SuccessfulExecutions { get; set; } = 0;
    public int FailedExecutions { get; set; } = 0;
    
    public long TotalRecordsProcessed { get; set; } = 0;
    public long TotalRecordsArchived { get; set; } = 0;
    public long TotalRecordsDeleted { get; set; } = 0;
    
    public TimeSpan? AverageExecutionDuration { get; set; }
    public TimeSpan? LastExecutionDuration { get; set; }
    
    // Error Handling
    [StringLength(1000)]
    public string? LastErrorMessage { get; set; }
    
    public DateTime? LastErrorDate { get; set; }
    public int ConsecutiveFailures { get; set; } = 0;
    
    public bool PauseOnError { get; set; } = true;
    
    // Cost Management
    public decimal? EstimatedMonthlyCost { get; set; }
    public decimal? ActualMonthlyCost { get; set; }
    public decimal? StorageCostPerGB { get; set; }
    
    // Lifecycle Management
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public bool IsTemplate { get; set; } = false;
    
    [StringLength(50)]
    public string? BasedOnPolicyId { get; set; } // Template or parent policy
    
    // Notification Configuration
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array
    
    public bool NotifyBeforeExecution { get; set; } = false;
    public bool NotifyAfterExecution { get; set; } = true;
    public bool NotifyOnPolicyChanges { get; set; } = true;
    
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
    
    public Dictionary<string, object>? FilterCriteriaObject
    {
        get => string.IsNullOrEmpty(FilterCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(FilterCriteria);
        set => FilterCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? InclusionRulesList
    {
        get => string.IsNullOrEmpty(InclusionRules) ? null : JsonConvert.DeserializeObject<List<object>>(InclusionRules);
        set => InclusionRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ExclusionRulesList
    {
        get => string.IsNullOrEmpty(ExclusionRules) ? null : JsonConvert.DeserializeObject<List<object>>(ExclusionRules);
        set => ExclusionRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? OrganizationIdsList
    {
        get => string.IsNullOrEmpty(OrganizationIds) ? null : JsonConvert.DeserializeObject<List<string>>(OrganizationIds);
        set => OrganizationIds = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AnonymizationRulesList
    {
        get => string.IsNullOrEmpty(AnonymizationRules) ? null : JsonConvert.DeserializeObject<List<object>>(AnonymizationRules);
        set => AnonymizationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RegulatoryRequirementsList
    {
        get => string.IsNullOrEmpty(RegulatoryRequirements) ? null : JsonConvert.DeserializeObject<List<string>>(RegulatoryRequirements);
        set => RegulatoryRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? LegalHoldExemptionsList
    {
        get => string.IsNullOrEmpty(LegalHoldExemptions) ? null : JsonConvert.DeserializeObject<List<object>>(LegalHoldExemptions);
        set => LegalHoldExemptions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? RiskMitigationStrategiesList
    {
        get => string.IsNullOrEmpty(RiskMitigationStrategies) ? null : JsonConvert.DeserializeObject<List<object>>(RiskMitigationStrategies);
        set => RiskMitigationStrategies = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AlertRecipientsList
    {
        get => string.IsNullOrEmpty(AlertRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(AlertRecipients);
        set => AlertRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public decimal GetSuccessRate()
    {
        if (TotalExecutions == 0) return 0;
        return (decimal)SuccessfulExecutions / TotalExecutions * 100;
    }
    
    public bool IsHealthy()
    {
        return Status == RetentionPolicyStatus.Active && 
               ConsecutiveFailures == 0 && 
               GetSuccessRate() >= 95m;
    }
    
    public bool RequiresAttention()
    {
        return ConsecutiveFailures >= 3 || 
               Status == RetentionPolicyStatus.Failed || 
               (ExpirationDate.HasValue && ExpirationDate < DateTime.UtcNow.AddDays(30)) ||
               (RequiresApproval && string.IsNullOrEmpty(ApprovedBy));
    }
    
    public bool IsExpired()
    {
        return ExpirationDate.HasValue && ExpirationDate < DateTime.UtcNow;
    }
    
    public bool IsOverdue()
    {
        return NextExecution.HasValue && NextExecution < DateTime.UtcNow && IsActive;
    }
    
    public TimeSpan? GetTimeSinceLastExecution()
    {
        return LastExecuted.HasValue ? DateTime.UtcNow - LastExecuted.Value : null;
    }
    
    // Navigation properties
    public virtual ICollection<Archive> Archives { get; set; } = new List<Archive>();
    public virtual ICollection<ArchiveMetadata> ArchiveMetadatas { get; set; } = new List<ArchiveMetadata>();
    public virtual ICollection<DataMigration> DataMigrations { get; set; } = new List<DataMigration>();
    public virtual ICollection<Compliance> ComplianceRecords { get; set; } = new List<Compliance>();
}

public enum RetentionPolicyType
{
    Standard = 1,
    Legal = 2,
    Regulatory = 3,
    Business = 4,
    Technical = 5,
    Custom = 6
}

public enum RetentionPolicyStatus
{
    Draft = 1,
    PendingApproval = 2,
    Active = 3,
    Paused = 4,
    Failed = 5,
    Expired = 6,
    Archived = 7
}

public enum StorageTier
{
    Hot = 1,
    Warm = 2,
    Cold = 3,
    Archive = 4,
    DeepArchive = 5
}

public enum ExecutionSchedule
{
    Manual = 1,
    Hourly = 2,
    Daily = 3,
    Weekly = 4,
    Monthly = 5,
    Quarterly = 6,
    Yearly = 7,
    Custom = 8
}

public enum PolicyPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum DataSensitivity
{
    Public = 1,
    Internal = 2,
    Confidential = 3,
    Restricted = 4
}

public enum BusinessRisk
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ComplianceRisk
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}