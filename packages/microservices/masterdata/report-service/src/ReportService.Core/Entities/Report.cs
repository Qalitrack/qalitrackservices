using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ReportService.Core.Entities;

public class Report : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ReportName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? TemplateId { get; set; }
    
    [Required]
    public ReportType ReportType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    // Report Configuration
    public ReportFormat Format { get; set; } = ReportFormat.PDF;
    public ReportLayout Layout { get; set; } = ReportLayout.Portrait;
    
    [StringLength(1000)]
    public string? DataSources { get; set; } // JSON array of data source configurations
    
    [StringLength(1000)]
    public string? Parameters { get; set; } // JSON object for report parameters
    
    [StringLength(1000)]
    public string? Filters { get; set; } // JSON object for data filters
    
    // Data Range
    public DateTime? DataStartDate { get; set; }
    public DateTime? DataEndDate { get; set; }
    public bool UseDynamicDateRange { get; set; } = false;
    
    [StringLength(50)]
    public string? DynamicDateRangeType { get; set; } // LastMonth, LastWeek, LastYear, etc.
    
    // Generation Settings
    public DateTime? LastGeneratedDate { get; set; }
    public DateTime? NextGenerationDate { get; set; }
    
    [StringLength(50)]
    public string? GeneratedBy { get; set; }
    
    public TimeSpan? LastGenerationDuration { get; set; }
    public long? LastGenerationSizeBytes { get; set; }
    
    [StringLength(1000)]
    public string? LastGenerationError { get; set; }
    
    public int GenerationRetryCount { get; set; } = 0;
    
    // Scheduling
    public bool IsScheduled { get; set; } = false;
    
    [StringLength(50)]
    public string? ScheduleId { get; set; }
    
    // Distribution
    public bool AutoDistribute { get; set; } = false;
    
    [StringLength(1000)]
    public string? Recipients { get; set; } // JSON array of email addresses
    
    [StringLength(500)]
    public string? EmailSubject { get; set; }
    
    [StringLength(1000)]
    public string? EmailBody { get; set; }
    
    // Storage
    [StringLength(500)]
    public string? OutputPath { get; set; }
    
    [StringLength(100)]
    public string? FileNamingPattern { get; set; }
    
    public bool RetainFiles { get; set; } = true;
    public int? RetentionDays { get; set; }
    
    // Access Control
    public ReportVisibility Visibility { get; set; } = ReportVisibility.Private;
    
    [StringLength(1000)]
    public string? AllowedRoles { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? AllowedUsers { get; set; } // JSON array
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessPurpose { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(1000)]
    public string? Tags { get; set; } // Searchable tags
    
    // Analytics Integration
    [StringLength(50)]
    public string? AnalyticsServiceId { get; set; }
    
    [StringLength(50)]
    public string? ComplianceServiceId { get; set; }
    
    public bool IncludeAnalytics { get; set; } = false;
    public bool IncludeCompliance { get; set; } = false;
    
    // Performance and Usage
    public int ViewCount { get; set; } = 0;
    public int DownloadCount { get; set; } = 0;
    public int ShareCount { get; set; } = 0;
    
    public DateTime? LastViewedDate { get; set; }
    public DateTime? LastDownloadedDate { get; set; }
    
    [StringLength(50)]
    public string? LastViewedBy { get; set; }
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? StyleJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties - temporarily commented out to avoid EF Core mapping issues
    /*
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Style
    {
        get => string.IsNullOrEmpty(StyleJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(StyleJson);
        set => StyleJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DataSourcesList
    {
        get => string.IsNullOrEmpty(DataSources) ? null : JsonConvert.DeserializeObject<List<string>>(DataSources);
        set => DataSources = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ParametersObject
    {
        get => string.IsNullOrEmpty(Parameters) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(Parameters);
        set => Parameters = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? FiltersObject
    {
        get => string.IsNullOrEmpty(Filters) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(Filters);
        set => Filters = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RecipientsList
    {
        get => string.IsNullOrEmpty(Recipients) ? null : JsonConvert.DeserializeObject<List<string>>(Recipients);
        set => Recipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AllowedRolesList
    {
        get => string.IsNullOrEmpty(AllowedRoles) ? null : JsonConvert.DeserializeObject<List<string>>(AllowedRoles);
        set => AllowedRoles = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? AllowedUsersList
    {
        get => string.IsNullOrEmpty(AllowedUsers) ? null : JsonConvert.DeserializeObject<List<string>>(AllowedUsers);
        set => AllowedUsers = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? TagsList
    {
        get => string.IsNullOrEmpty(Tags) ? null : JsonConvert.DeserializeObject<List<string>>(Tags);
        set => Tags = value == null ? null : JsonConvert.SerializeObject(value);
    }
    */
    
    // Navigation Properties
    public virtual Template? Template { get; set; }
    public virtual Schedule? Schedule { get; set; }
    public virtual ICollection<Export> Exports { get; set; } = new List<Export>();
}

public enum ReportType
{
    OperationalReport = 1,
    FinancialReport = 2,
    ComplianceReport = 3,
    AnalyticsReport = 4,
    PerformanceReport = 5,
    TransactionReport = 6,
    WeightReport = 7,
    DriverReport = 8,
    VehicleReport = 9,
    RouteReport = 10,
    SupplierReport = 11,
    CustomerReport = 12,
    ExecutiveSummary = 13,
    RegulatorySubmission = 14,
    CustomReport = 15
}

public enum ReportStatus
{
    Draft = 1,
    Active = 2,
    Generating = 3,
    Generated = 4,
    Failed = 5,
    Scheduled = 6,
    Distributed = 7,
    Archived = 8,
    Deprecated = 9
}

public enum ReportFormat
{
    PDF = 1,
    Excel = 2,
    CSV = 3,
    Word = 4,
    PowerPoint = 5,
    HTML = 6,
    JSON = 7,
    XML = 8,
    Image = 9,
    Dashboard = 10
}

public enum ReportLayout
{
    Portrait = 1,
    Landscape = 2,
    Custom = 3
}

public enum ReportVisibility
{
    Private = 1,
    Organization = 2,
    Public = 3,
    RoleBased = 4,
    Custom = 5
}

public enum ReportPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}