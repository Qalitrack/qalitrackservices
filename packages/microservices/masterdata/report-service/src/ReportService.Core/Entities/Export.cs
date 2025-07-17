using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ReportService.Core.Entities;

public class Export : BaseEntity
{
    [Required]
    [StringLength(50)]
    public string ReportId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string ExportName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Export Configuration
    [Required]
    public ReportFormat Format { get; set; }
    
    public ExportStatus Status { get; set; } = ExportStatus.Pending;
    
    public ExportType ExportType { get; set; } = ExportType.OnDemand;
    
    // File Information
    [StringLength(500)]
    public string? FileName { get; set; }
    
    [StringLength(1000)]
    public string? FilePath { get; set; }
    
    public long? FileSizeBytes { get; set; }
    
    [StringLength(100)]
    public string? FileHash { get; set; } // MD5 or SHA256
    
    [StringLength(100)]
    public string? MimeType { get; set; }
    
    // Generation Information
    public DateTime? StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    
    public TimeSpan? GenerationDuration
    {
        get => StartTime.HasValue && CompletionTime.HasValue ? CompletionTime.Value - StartTime.Value : null;
    }
    
    [StringLength(50)]
    public string? GeneratedBy { get; set; }
    
    [StringLength(100)]
    public string? GenerationMethod { get; set; } // Manual, Scheduled, API
    
    // Content Configuration
    public bool IncludeHeader { get; set; } = true;
    public bool IncludeFooter { get; set; } = true;
    public bool IncludePageNumbers { get; set; } = true;
    public bool IncludeTableOfContents { get; set; } = false;
    public bool IncludeAppendix { get; set; } = false;
    
    // Data Selection
    [StringLength(1000)]
    public string? SelectedSections { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? SelectedFields { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? FilterCriteria { get; set; } // JSON object
    
    public DateTime? DataStartDate { get; set; }
    public DateTime? DataEndDate { get; set; }
    
    public int? MaxRecords { get; set; }
    
    // Format-Specific Options
    [StringLength(1000)]
    public string? PdfOptions { get; set; } // JSON object
    
    [StringLength(1000)]
    public string? ExcelOptions { get; set; } // JSON object
    
    [StringLength(1000)]
    public string? CsvOptions { get; set; } // JSON object
    
    [StringLength(1000)]
    public string? ImageOptions { get; set; } // JSON object
    
    // Quality and Optimization
    public int? CompressionLevel { get; set; } // 0-100
    public int? ImageQuality { get; set; } // 0-100
    public int? ImageResolution { get; set; } // DPI
    
    public bool OptimizeForSize { get; set; } = false;
    public bool OptimizeForQuality { get; set; } = true;
    
    // Security and Access
    public bool IsPasswordProtected { get; set; } = false;
    
    [StringLength(500)]
    public string? PasswordHash { get; set; }
    
    public bool IsEncrypted { get; set; } = false;
    
    [StringLength(100)]
    public string? EncryptionMethod { get; set; }
    
    public ExportVisibility Visibility { get; set; } = ExportVisibility.Private;
    
    [StringLength(1000)]
    public string? AllowedUsers { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? AllowedRoles { get; set; } // JSON array
    
    // Download Information
    public int DownloadCount { get; set; } = 0;
    public DateTime? FirstDownloadDate { get; set; }
    public DateTime? LastDownloadDate { get; set; }
    
    [StringLength(50)]
    public string? LastDownloadedBy { get; set; }
    
    [StringLength(200)]
    public string? LastDownloadIP { get; set; }
    
    // Retention and Lifecycle
    public DateTime? ExpirationDate { get; set; }
    public bool AutoDelete { get; set; } = false;
    public int? RetentionDays { get; set; }
    
    public bool IsArchived { get; set; } = false;
    public DateTime? ArchivedDate { get; set; }
    
    [StringLength(50)]
    public string? ArchivedBy { get; set; }
    
    // Distribution
    public bool AutoDistribute { get; set; } = false;
    
    [StringLength(1000)]
    public string? DistributionEmails { get; set; } // JSON array
    
    [StringLength(500)]
    public string? DistributionSubject { get; set; }
    
    [StringLength(1000)]
    public string? DistributionMessage { get; set; }
    
    public DateTime? DistributionDate { get; set; }
    public bool DistributionCompleted { get; set; } = false;
    
    [StringLength(1000)]
    public string? DistributionErrors { get; set; }
    
    // Error Handling
    [StringLength(1000)]
    public string? ErrorMessage { get; set; }
    
    [StringLength(100)]
    public string? ErrorCode { get; set; }
    
    [StringLength(5000)]
    public string? ErrorDetails { get; set; }
    
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
    
    public DateTime? LastRetryDate { get; set; }
    
    // Performance Metrics
    public long? MemoryUsedBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    public int? RecordsProcessed { get; set; }
    public int? PagesGenerated { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessPurpose { get; set; }
    
    [StringLength(100)]
    public string? RequestedBy { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    public ExportPriority Priority { get; set; } = ExportPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessJustification { get; set; }
    
    // Audit and Compliance
    public bool RequiresAudit { get; set; } = false;
    
    [StringLength(1000)]
    public string? AuditTrail { get; set; } // JSON array
    
    public bool IsComplianceExport { get; set; } = false;
    
    [StringLength(100)]
    public string? ComplianceFramework { get; set; }
    
    [StringLength(1000)]
    public string? ComplianceMetadata { get; set; } // JSON object
    
    // External Integration
    [StringLength(500)]
    public string? ExternalSystemId { get; set; }
    
    [StringLength(100)]
    public string? ExternalSystemName { get; set; }
    
    [StringLength(1000)]
    public string? ExternalMetadata { get; set; } // JSON object
    
    public bool SyncToExternal { get; set; } = false;
    public DateTime? LastSyncDate { get; set; }
    public bool SyncCompleted { get; set; } = false;
    
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
    
    public List<string>? SelectedSectionsList
    {
        get => string.IsNullOrEmpty(SelectedSections) ? null : JsonConvert.DeserializeObject<List<string>>(SelectedSections);
        set => SelectedSections = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? SelectedFieldsList
    {
        get => string.IsNullOrEmpty(SelectedFields) ? null : JsonConvert.DeserializeObject<List<string>>(SelectedFields);
        set => SelectedFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? FilterCriteriaObject
    {
        get => string.IsNullOrEmpty(FilterCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(FilterCriteria);
        set => FilterCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? PdfOptionsObject
    {
        get => string.IsNullOrEmpty(PdfOptions) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(PdfOptions);
        set => PdfOptions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ExcelOptionsObject
    {
        get => string.IsNullOrEmpty(ExcelOptions) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ExcelOptions);
        set => ExcelOptions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? CsvOptionsObject
    {
        get => string.IsNullOrEmpty(CsvOptions) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(CsvOptions);
        set => CsvOptions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ImageOptionsObject
    {
        get => string.IsNullOrEmpty(ImageOptions) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ImageOptions);
        set => ImageOptions = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AllowedUsersList
    {
        get => string.IsNullOrEmpty(AllowedUsers) ? null : JsonConvert.DeserializeObject<List<string>>(AllowedUsers);
        set => AllowedUsers = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AllowedRolesList
    {
        get => string.IsNullOrEmpty(AllowedRoles) ? null : JsonConvert.DeserializeObject<List<string>>(AllowedRoles);
        set => AllowedRoles = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DistributionEmailsList
    {
        get => string.IsNullOrEmpty(DistributionEmails) ? null : JsonConvert.DeserializeObject<List<string>>(DistributionEmails);
        set => DistributionEmails = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? AuditTrailList
    {
        get => string.IsNullOrEmpty(AuditTrail) ? null : JsonConvert.DeserializeObject<List<object>>(AuditTrail);
        set => AuditTrail = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ComplianceMetadataObject
    {
        get => string.IsNullOrEmpty(ComplianceMetadata) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ComplianceMetadata);
        set => ComplianceMetadata = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ExternalMetadataObject
    {
        get => string.IsNullOrEmpty(ExternalMetadata) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ExternalMetadata);
        set => ExternalMetadata = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Helper methods
    public bool IsComplete => Status == ExportStatus.Completed;
    public bool IsFailed => Status == ExportStatus.Failed;
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;
    public bool CanRetry => RetryCount < MaxRetries && IsFailed;
    
    public string GetHumanReadableFileSize()
    {
        if (!FileSizeBytes.HasValue) return "Unknown";
        
        var bytes = FileSizeBytes.Value;
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        while (bytes >= 1024 && order < sizes.Length - 1)
        {
            order++;
            bytes = bytes / 1024;
        }
        return $"{bytes:0.##} {sizes[order]}";
    }
    
    public decimal GetCompressionRatio()
    {
        if (!FileSizeBytes.HasValue || FileSizeBytes.Value == 0) return 0;
        
        // This would be calculated based on original size vs compressed size
        // For now, return a placeholder calculation
        return CompressionLevel ?? 0;
    }
    
    // Navigation Properties
    public virtual Report Report { get; set; } = null!;
}

public enum ExportStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Expired = 6,
    Archived = 7
}

public enum ExportType
{
    OnDemand = 1,
    Scheduled = 2,
    Automated = 3,
    Batch = 4,
    RealTime = 5
}

public enum ExportVisibility
{
    Private = 1,
    Organization = 2,
    Public = 3,
    RoleBased = 4,
    Custom = 5
}

public enum ExportPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}