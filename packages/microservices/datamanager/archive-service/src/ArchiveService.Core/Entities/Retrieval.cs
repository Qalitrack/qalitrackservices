using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ArchiveService.Core.Entities;

public class Retrieval : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string RetrievalId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [StringLength(50)]
    public string ArchiveId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string RetrievalName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public RetrievalType RetrievalType { get; set; }
    
    public RetrievalStatus Status { get; set; } = RetrievalStatus.Pending;
    
    // Request Information
    [Required]
    [StringLength(50)]
    public string RequestedBy { get; set; } = string.Empty;
    
    public DateTime RequestedDate { get; set; } = DateTime.UtcNow;
    
    [StringLength(1000)]
    public string? RequestReason { get; set; }
    
    [StringLength(100)]
    public string? RequestCategory { get; set; } // Business, Legal, Compliance, Technical
    
    public RetrievalPriority Priority { get; set; } = RetrievalPriority.Medium;
    
    // Search Criteria
    [StringLength(2000)]
    public string? SearchCriteria { get; set; } // JSON object for search parameters
    
    [StringLength(1000)]
    public string? DateRangeFilter { get; set; } // JSON object for date range
    
    [StringLength(1000)]
    public string? EntityFilter { get; set; } // JSON object for entity-specific filters
    
    [StringLength(1000)]
    public string? FieldSelections { get; set; } // JSON array of specific fields to retrieve
    
    [StringLength(1000)]
    public string? ExclusionCriteria { get; set; } // JSON object for data to exclude
    
    public int? MaxRecords { get; set; }
    public long? MaxSizeBytes { get; set; }
    
    // Output Configuration
    public RetrievalOutputFormat OutputFormat { get; set; } = RetrievalOutputFormat.Original;
    
    [StringLength(500)]
    public string? OutputPath { get; set; }
    
    [StringLength(100)]
    public string? OutputFileName { get; set; }
    
    public bool CompressOutput { get; set; } = true;
    public CompressionType CompressionType { get; set; } = CompressionType.GZip;
    
    public bool EncryptOutput { get; set; } = false;
    
    [StringLength(100)]
    public string? EncryptionMethod { get; set; }
    
    // Delivery Configuration
    public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.Download;
    
    [StringLength(1000)]
    public string? DeliveryDestination { get; set; } // Email, FTP, S3, etc.
    
    [StringLength(1000)]
    public string? DeliveryCredentials { get; set; } // Encrypted delivery credentials
    
    public bool NotifyOnCompletion { get; set; } = true;
    
    [StringLength(1000)]
    public string? NotificationRecipients { get; set; } // JSON array of email addresses
    
    // Approval Workflow
    public bool RequiresApproval { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovalRequestedFrom { get; set; }
    
    public DateTime? ApprovalRequestedDate { get; set; }
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedDate { get; set; }
    
    [StringLength(1000)]
    public string? ApprovalComments { get; set; }
    
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.NotRequired;
    
    // Processing Information
    public DateTime? ProcessingStarted { get; set; }
    public DateTime? ProcessingCompleted { get; set; }
    
    public TimeSpan? ProcessingDuration
    {
        get => ProcessingStarted.HasValue && ProcessingCompleted.HasValue ? ProcessingCompleted.Value - ProcessingStarted.Value : null;
    }
    
    [StringLength(50)]
    public string? ProcessedBy { get; set; } // System or user who processed the request
    
    // Results Information
    public long RecordsFound { get; set; } = 0;
    public long RecordsRetrieved { get; set; } = 0;
    public long RecordsFiltered { get; set; } = 0;
    public long RecordsExcluded { get; set; } = 0;
    
    public long OutputSizeBytes { get; set; } = 0;
    public long CompressedSizeBytes { get; set; } = 0;
    
    [StringLength(100)]
    public string? OutputChecksum { get; set; }
    
    [StringLength(500)]
    public string? ResultSummary { get; set; }
    
    // Performance Metrics
    public decimal? SearchTimeSeconds { get; set; }
    public decimal? RetrievalTimeSeconds { get; set; }
    public decimal? CompressionTimeSeconds { get; set; }
    public decimal? DeliveryTimeSeconds { get; set; }
    
    public long? MemoryUsedBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    public long? DiskUsedBytes { get; set; }
    
    // Quality and Validation
    public bool DataValidationPerformed { get; set; } = false;
    public bool IntegrityCheckPassed { get; set; } = false;
    
    [StringLength(1000)]
    public string? ValidationResults { get; set; } // JSON object with validation results
    
    public decimal? DataQualityScore { get; set; }
    
    // Error Handling
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    [StringLength(5000)]
    public string? ErrorDetails { get; set; }
    
    public int RetryAttempts { get; set; } = 0;
    public int MaxRetryAttempts { get; set; } = 3;
    
    public DateTime? LastRetryDate { get; set; }
    
    // Security and Compliance
    public bool RequiresAuditTrail { get; set; } = true;
    
    [StringLength(1000)]
    public string? AuditTrail { get; set; } // JSON array of audit events
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    public bool DataMasked { get; set; } = false;
    
    [StringLength(1000)]
    public string? MaskingRules { get; set; } // JSON array of masking rules applied
    
    public bool PersonalDataIncluded { get; set; } = false;
    public bool LegalHoldDataIncluded { get; set; } = false;
    
    // Access Control
    [StringLength(1000)]
    public string? AccessRestrictions { get; set; } // JSON object for access restrictions
    
    [StringLength(1000)]
    public string? AuthorizedUsers { get; set; } // JSON array of authorized user IDs
    
    [StringLength(1000)]
    public string? AuthorizedRoles { get; set; } // JSON array of authorized roles
    
    public DateTime? AccessExpiryDate { get; set; }
    public bool AccessRevoked { get; set; } = false;
    
    // Cost Management
    public decimal? RetrievalCost { get; set; }
    public decimal? StorageCost { get; set; }
    public decimal? DeliveryCost { get; set; }
    public decimal? TotalCost { get; set; }
    
    [StringLength(100)]
    public string? CostCenter { get; set; }
    
    [StringLength(100)]
    public string? BillingAccount { get; set; }
    
    // Retention and Cleanup
    public DateTime? ExpiryDate { get; set; }
    public bool AutoDelete { get; set; } = true;
    public int RetentionDays { get; set; } = 30;
    
    public bool OutputDeleted { get; set; } = false;
    public DateTime? OutputDeletedDate { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessJustification { get; set; }
    
    [StringLength(100)]
    public string? ProjectId { get; set; }
    
    [StringLength(100)]
    public string? TicketId { get; set; }
    
    [StringLength(1000)]
    public string? RelatedRetrievals { get; set; } // JSON array of related retrieval IDs
    
    // Monitoring and Analytics
    public bool EnableDetailedLogging { get; set; } = true;
    
    [StringLength(1000)]
    public string? UserActivity { get; set; } // JSON array of user activity logs
    
    public int DownloadCount { get; set; } = 0;
    public DateTime? LastDownloadDate { get; set; }
    
    [StringLength(50)]
    public string? LastDownloadedBy { get; set; }
    
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
    
    public Dictionary<string, object>? SearchCriteriaObject
    {
        get => string.IsNullOrEmpty(SearchCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(SearchCriteria);
        set => SearchCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? DateRangeFilterObject
    {
        get => string.IsNullOrEmpty(DateRangeFilter) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(DateRangeFilter);
        set => DateRangeFilter = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? EntityFilterObject
    {
        get => string.IsNullOrEmpty(EntityFilter) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(EntityFilter);
        set => EntityFilter = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? FieldSelectionsList
    {
        get => string.IsNullOrEmpty(FieldSelections) ? null : JsonConvert.DeserializeObject<List<string>>(FieldSelections);
        set => FieldSelections = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ExclusionCriteriaObject
    {
        get => string.IsNullOrEmpty(ExclusionCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ExclusionCriteria);
        set => ExclusionCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? NotificationRecipientsList
    {
        get => string.IsNullOrEmpty(NotificationRecipients) ? null : JsonConvert.DeserializeObject<List<string>>(NotificationRecipients);
        set => NotificationRecipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ValidationResultsObject
    {
        get => string.IsNullOrEmpty(ValidationResults) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ValidationResults);
        set => ValidationResults = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AuditTrailList
    {
        get => string.IsNullOrEmpty(AuditTrail) ? null : JsonConvert.DeserializeObject<List<object>>(AuditTrail);
        set => AuditTrail = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? MaskingRulesList
    {
        get => string.IsNullOrEmpty(MaskingRules) ? null : JsonConvert.DeserializeObject<List<object>>(MaskingRules);
        set => MaskingRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? AccessRestrictionsObject
    {
        get => string.IsNullOrEmpty(AccessRestrictions) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(AccessRestrictions);
        set => AccessRestrictions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AuthorizedUsersList
    {
        get => string.IsNullOrEmpty(AuthorizedUsers) ? null : JsonConvert.DeserializeObject<List<string>>(AuthorizedUsers);
        set => AuthorizedUsers = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AuthorizedRolesList
    {
        get => string.IsNullOrEmpty(AuthorizedRoles) ? null : JsonConvert.DeserializeObject<List<string>>(AuthorizedRoles);
        set => AuthorizedRoles = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RelatedRetrievalsList
    {
        get => string.IsNullOrEmpty(RelatedRetrievals) ? null : JsonConvert.DeserializeObject<List<string>>(RelatedRetrievals);
        set => RelatedRetrievals = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? UserActivityList
    {
        get => string.IsNullOrEmpty(UserActivity) ? null : JsonConvert.DeserializeObject<List<object>>(UserActivity);
        set => UserActivity = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsCompleted => Status == RetrievalStatus.Completed;
    public bool IsFailed => Status == RetrievalStatus.Failed;
    public bool IsInProgress => Status == RetrievalStatus.InProgress;
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate < DateTime.UtcNow;
    public bool CanRetry => IsFailed && RetryAttempts < MaxRetryAttempts;
    
    public decimal GetCompressionRatio()
    {
        if (OutputSizeBytes == 0) return 0;
        return (decimal)CompressedSizeBytes / OutputSizeBytes * 100;
    }
    
    public decimal GetRetrievalEfficiency()
    {
        if (RecordsFound == 0) return 0;
        return (decimal)RecordsRetrieved / RecordsFound * 100;
    }
    
    public TimeSpan GetAge()
    {
        return DateTime.UtcNow - RequestedDate;
    }
    
    public bool IsStale()
    {
        return GetAge().TotalDays > 30 && Status == RetrievalStatus.Pending;
    }
    
    public string GetHumanReadableSize()
    {
        var bytes = OutputSizeBytes;
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
    
    // Navigation Properties
    public virtual Archive Archive { get; set; } = null!;
}

public enum RetrievalType
{
    FullDataset = 1,
    FilteredData = 2,
    SpecificRecords = 3,
    DateRange = 4,
    SampleData = 5,
    MetadataOnly = 6,
    SchemaOnly = 7,
    Custom = 8
}

public enum RetrievalStatus
{
    Pending = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    InProgress = 5,
    Completed = 6,
    Failed = 7,
    Cancelled = 8,
    Expired = 9
}

public enum RetrievalPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4,
    Critical = 5
}

public enum RetrievalOutputFormat
{
    Original = 1,
    CSV = 2,
    JSON = 3,
    XML = 4,
    Parquet = 5,
    Database = 6,
    Excel = 7,
    Custom = 8
}

public enum DeliveryMethod
{
    Download = 1,
    Email = 2,
    FTP = 3,
    SFTP = 4,
    S3 = 5,
    Azure = 6,
    Database = 7,
    API = 8
}

public enum ApprovalStatus
{
    NotRequired = 1,
    Pending = 2,
    Approved = 3,
    Rejected = 4,
    Escalated = 5
}