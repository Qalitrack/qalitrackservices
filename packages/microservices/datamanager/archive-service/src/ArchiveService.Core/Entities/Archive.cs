using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArchiveService.Core.Entities;

public class Archive : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ArchiveName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ArchiveType ArchiveType { get; set; }
    
    public ArchiveStatus Status { get; set; } = ArchiveStatus.Active;
    
    // Data Source Configuration
    [Required]
    [StringLength(100)]
    public string SourceEntityType { get; set; } = string.Empty; // Transaction, WeightMeasurement, etc.
    
    [StringLength(100)]
    public string? SourceServiceName { get; set; }
    
    [StringLength(1000)]
    public string? SourceConnectionString { get; set; }
    
    [StringLength(100)]
    public string? SourceDatabase { get; set; }
    
    [StringLength(100)]
    public string? SourceTable { get; set; }
    
    [StringLength(1000)]
    public string? SourceQuery { get; set; }
    
    [StringLength(1000)]
    public string? SourceFilters { get; set; } // JSON object for filtering criteria
    
    // Archive Storage Configuration
    [Required]
    public StorageType StorageType { get; set; } = StorageType.Database;
    
    [StringLength(500)]
    public string? StoragePath { get; set; }
    
    [StringLength(1000)]
    public string? StorageConnectionString { get; set; }
    
    [StringLength(100)]
    public string? StorageContainer { get; set; }
    
    [StringLength(1000)]
    public string? StorageConfiguration { get; set; } // JSON object for storage settings
    
    // Archival Policies
    [StringLength(50)]
    public string? RetentionPolicyId { get; set; }
    
    public ArchivalTrigger ArchivalTrigger { get; set; } = ArchivalTrigger.Manual;
    
    public int? ArchiveAfterDays { get; set; }
    public DateTime? ArchiveAfterDate { get; set; }
    
    [StringLength(100)]
    public string? ArchivalCronExpression { get; set; }
    
    public bool AutoArchive { get; set; } = false;
    public DateTime? NextArchivalDate { get; set; }
    public DateTime? LastArchivalDate { get; set; }
    
    // Data Selection and Processing
    [StringLength(1000)]
    public string? ArchivalCriteria { get; set; } // JSON object for archival criteria
    
    [StringLength(1000)]
    public string? FieldSelection { get; set; } // JSON array of fields to archive
    
    [StringLength(1000)]
    public string? ExclusionRules { get; set; } // JSON array of exclusion rules
    
    public bool ArchiveRelatedData { get; set; } = true;
    public bool MaintainReferentialIntegrity { get; set; } = true;
    
    // Compression and Optimization
    public bool EnableCompression { get; set; } = true;
    public CompressionType CompressionType { get; set; } = CompressionType.GZip;
    public int CompressionLevel { get; set; } = 6; // 1-9
    
    public bool EnableEncryption { get; set; } = false;
    
    [StringLength(100)]
    public string? EncryptionMethod { get; set; }
    
    [StringLength(500)]
    public string? EncryptionKey { get; set; }
    
    // Performance Configuration
    public int BatchSize { get; set; } = 1000;
    public int MaxConcurrentBatches { get; set; } = 3;
    public int TimeoutMinutes { get; set; } = 60;
    
    public bool EnableParallelProcessing { get; set; } = true;
    public int MaxDegreeOfParallelism { get; set; } = 4;
    
    // Data Validation
    public bool EnableDataValidation { get; set; } = true;
    public bool EnableIntegrityChecks { get; set; } = true;
    public bool EnableChecksumValidation { get; set; } = true;
    
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array of validation rules
    
    // Quality Control
    public decimal? AcceptableErrorRate { get; set; } = 1.0m; // Percentage
    public int? MaxErrorsBeforeStop { get; set; } = 100;
    
    public bool RequirePreArchivalBackup { get; set; } = true;
    public bool RequirePostArchivalVerification { get; set; } = true;
    
    // Indexing Configuration
    public bool CreateSearchIndex { get; set; } = true;
    
    [StringLength(1000)]
    public string? IndexedFields { get; set; } // JSON array of fields to index
    
    [StringLength(100)]
    public string? IndexType { get; set; } = "ELASTICSEARCH";
    
    [StringLength(1000)]
    public string? IndexConfiguration { get; set; } // JSON object for index settings
    
    // Retrieval Configuration
    public bool AllowRetrievalRequests { get; set; } = true;
    public bool RequireApprovalForRetrieval { get; set; } = false;
    
    [StringLength(1000)]
    public string? RetrievalRestrictions { get; set; } // JSON array of restrictions
    
    public int? RetrievalTimeoutMinutes { get; set; } = 30;
    
    // Statistics and Monitoring
    public long TotalRecordsArchived { get; set; } = 0;
    public long TotalBytesArchived { get; set; } = 0;
    public long CompressedSizeBytes { get; set; } = 0;
    
    public int TotalArchivalRuns { get; set; } = 0;
    public int SuccessfulArchivalRuns { get; set; } = 0;
    public int FailedArchivalRuns { get; set; } = 0;
    
    public TimeSpan? AverageArchivalDuration { get; set; }
    public TimeSpan? LastArchivalDuration { get; set; }
    
    public long TotalRetrievalRequests { get; set; } = 0;
    public long SuccessfulRetrievals { get; set; } = 0;
    public long FailedRetrievals { get; set; } = 0;
    
    // Error Handling
    [StringLength(1000)]
    public string? LastErrorMessage { get; set; }
    
    public DateTime? LastErrorDate { get; set; }
    public int ConsecutiveFailures { get; set; } = 0;
    
    public bool PauseOnError { get; set; } = false;
    public bool EnableErrorRecovery { get; set; } = true;
    
    // Compliance and Legal
    public bool RequiresComplianceReview { get; set; } = false;
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    [StringLength(1000)]
    public string? LegalHoldPolicies { get; set; } // JSON array of legal hold policies
    
    public bool IsUnderLegalHold { get; set; } = false;
    public DateTime? LegalHoldStartDate { get; set; }
    public DateTime? LegalHoldEndDate { get; set; }
    
    [StringLength(1000)]
    public string? RegulatoryRequirements { get; set; } // JSON array of regulatory requirements
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    [StringLength(100)]
    public string? DataSteward { get; set; }
    
    public ArchivePriority Priority { get; set; } = ArchivePriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    [StringLength(100)]
    public string? DataClassification { get; set; } // Public, Internal, Confidential, Restricted
    
    // Cost Management
    public decimal? StorageCostPerGB { get; set; }
    public decimal? RetrievalCostPerGB { get; set; }
    public decimal? EstimatedMonthlyCost { get; set; }
    public decimal? ActualMonthlyCost { get; set; }
    
    // Lifecycle Management
    public bool IsActive { get; set; } = true;
    public bool IsArchived { get; set; } = false;
    
    public DateTime? ArchivedDate { get; set; }
    
    [StringLength(50)]
    public string? ArchivedBy { get; set; }
    
    public DateTime? ExpirationDate { get; set; }
    public bool AutoDeleteOnExpiration { get; set; } = false;
    
    // Notification Configuration
    public bool EnableNotifications { get; set; } = true;
    
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array of email addresses
    
    public bool NotifyOnSuccess { get; set; } = false;
    public bool NotifyOnFailure { get; set; } = true;
    public bool NotifyOnCompletion { get; set; } = true;
    
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
    
    public Dictionary<string, object>? SourceFiltersObject
    {
        get => string.IsNullOrEmpty(SourceFilters) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(SourceFilters);
        set => SourceFilters = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? StorageConfigurationObject
    {
        get => string.IsNullOrEmpty(StorageConfiguration) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(StorageConfiguration);
        set => StorageConfiguration = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ArchivalCriteriaObject
    {
        get => string.IsNullOrEmpty(ArchivalCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ArchivalCriteria);
        set => ArchivalCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? FieldSelectionList
    {
        get => string.IsNullOrEmpty(FieldSelection) ? null : JsonConvert.DeserializeObject<List<string>>(FieldSelection);
        set => FieldSelection = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ExclusionRulesList
    {
        get => string.IsNullOrEmpty(ExclusionRules) ? null : JsonConvert.DeserializeObject<List<object>>(ExclusionRules);
        set => ExclusionRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? IndexedFieldsList
    {
        get => string.IsNullOrEmpty(IndexedFields) ? null : JsonConvert.DeserializeObject<List<string>>(IndexedFields);
        set => IndexedFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? IndexConfigurationObject
    {
        get => string.IsNullOrEmpty(IndexConfiguration) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(IndexConfiguration);
        set => IndexConfiguration = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? RetrievalRestrictionsList
    {
        get => string.IsNullOrEmpty(RetrievalRestrictions) ? null : JsonConvert.DeserializeObject<List<object>>(RetrievalRestrictions);
        set => RetrievalRestrictions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? LegalHoldPoliciesList
    {
        get => string.IsNullOrEmpty(LegalHoldPolicies) ? null : JsonConvert.DeserializeObject<List<object>>(LegalHoldPolicies);
        set => LegalHoldPolicies = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RegulatoryRequirementsList
    {
        get => string.IsNullOrEmpty(RegulatoryRequirements) ? null : JsonConvert.DeserializeObject<List<string>>(RegulatoryRequirements);
        set => RegulatoryRequirements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public decimal GetCompressionRatio()
    {
        if (TotalBytesArchived == 0) return 0;
        return (decimal)CompressedSizeBytes / TotalBytesArchived * 100;
    }
    
    public decimal GetSuccessRate()
    {
        if (TotalArchivalRuns == 0) return 0;
        return (decimal)SuccessfulArchivalRuns / TotalArchivalRuns * 100;
    }
    
    public decimal GetRetrievalSuccessRate()
    {
        if (TotalRetrievalRequests == 0) return 0;
        return (decimal)SuccessfulRetrievals / TotalRetrievalRequests * 100;
    }
    
    public bool IsHealthy()
    {
        return Status == ArchiveStatus.Active && 
               ConsecutiveFailures == 0 && 
               GetSuccessRate() >= 95m;
    }
    
    public bool RequiresAttention()
    {
        return ConsecutiveFailures >= 3 || 
               Status == ArchiveStatus.Failed || 
               (IsUnderLegalHold && LegalHoldEndDate < DateTime.UtcNow) ||
               (ExpirationDate.HasValue && ExpirationDate < DateTime.UtcNow.AddDays(30));
    }
    
    public decimal GetStorageEfficiency()
    {
        if (TotalBytesArchived == 0) return 0;
        return 100 - GetCompressionRatio(); // Higher is better
    }
    
    // Navigation Properties
    public virtual RetentionPolicy? RetentionPolicy { get; set; }
    public virtual ICollection<ArchiveMetadata> ArchiveMetadata { get; set; } = new List<ArchiveMetadata>();
    public virtual ICollection<Retrieval> Retrievals { get; set; } = new List<Retrieval>();
    public virtual ICollection<Compliance> ComplianceRecords { get; set; } = new List<Compliance>();
    public virtual ICollection<ArchiveIndex> Indexes { get; set; } = new List<ArchiveIndex>();
}

public enum ArchiveType
{
    Transactional = 1,
    Operational = 2,
    Analytical = 3,
    Compliance = 4,
    Backup = 5,
    Migration = 6,
    Purge = 7,
    Custom = 8
}

public enum ArchiveStatus
{
    Active = 1,
    Scheduled = 2,
    Running = 3,
    Completed = 4,
    Failed = 5,
    Paused = 6,
    Cancelled = 7,
    Archived = 8,
    Expired = 9
}

public enum StorageType
{
    Database = 1,
    FileSystem = 2,
    CloudStorage = 3,
    ObjectStorage = 4,
    DataLake = 5,
    Tape = 6,
    Hybrid = 7
}

public enum ArchivalTrigger
{
    Manual = 1,
    Scheduled = 2,
    AgeBasedDays = 3,
    AgeBasedDate = 4,
    SizeBasedMB = 5,
    SizeBasedGB = 6,
    CountBased = 7,
    EventDriven = 8,
    PolicyBased = 9
}

public enum CompressionType
{
    None = 1,
    GZip = 2,
    Deflate = 3,
    LZ4 = 4,
    Snappy = 5,
    Brotli = 6
}

public enum ArchivePriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}