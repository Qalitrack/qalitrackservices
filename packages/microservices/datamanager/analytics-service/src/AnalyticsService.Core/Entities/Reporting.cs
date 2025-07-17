using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AnalyticsService.Core.Entities;

public class Reporting : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string ReportName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? AnalyticsId { get; set; }
    
    [Required]
    public ReportType ReportType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public ReportStatus Status { get; set; } = ReportStatus.Draft;
    
    // Report Configuration
    public ReportFormat Format { get; set; } = ReportFormat.Dashboard;
    public DataVisualizationType VisualizationType { get; set; } = DataVisualizationType.Table;
    
    [StringLength(1000)]
    public string? ReportQuery { get; set; } // SQL or query configuration
    
    [StringLength(1000)]
    public string? DataSources { get; set; } // JSON array of data sources
    
    [StringLength(1000)]
    public string? FilterCriteria { get; set; } // JSON object for filters
    
    [StringLength(1000)]
    public string? SortCriteria { get; set; } // JSON object for sorting
    
    // Scheduling and Automation
    public bool IsScheduled { get; set; } = false;
    public ScheduleFrequency? ScheduleFrequency { get; set; }
    
    public DateTime? NextRunDate { get; set; }
    public DateTime? LastRunDate { get; set; }
    
    [StringLength(100)]
    public string? CronExpression { get; set; }
    
    // Distribution
    public bool AutoDistribute { get; set; } = false;
    
    [StringLength(1000)]
    public string? Recipients { get; set; } // JSON array of email addresses
    
    [StringLength(1000)]
    public string? DistributionChannels { get; set; } // Email, Portal, API, etc.
    
    // Report Content
    public DateTime? ReportPeriodStart { get; set; }
    public DateTime? ReportPeriodEnd { get; set; }
    
    public bool IncludeTrendAnalysis { get; set; } = false;
    public bool IncludeBenchmarks { get; set; } = false;
    public bool IncludeAnomalies { get; set; } = false;
    public bool IncludeKPIs { get; set; } = false;
    
    // Performance and Execution
    public DateTime? LastGenerationDate { get; set; }
    public TimeSpan? LastGenerationDuration { get; set; }
    
    public long? LastRecordCount { get; set; }
    public long? LastFileSizeBytes { get; set; }
    
    [StringLength(1000)]
    public string? LastGenerationError { get; set; }
    
    public int GenerationRetryCount { get; set; } = 0;
    
    // Output Configuration
    [StringLength(500)]
    public string? OutputPath { get; set; }
    
    [StringLength(100)]
    public string? FileNamingPattern { get; set; }
    
    public bool CompressOutput { get; set; } = false;
    public bool EncryptOutput { get; set; } = false;
    
    // Business Intelligence Features
    public bool EnableDrillDown { get; set; } = false;
    public bool EnableExport { get; set; } = true;
    public bool EnableInteractivity { get; set; } = false;
    
    [StringLength(1000)]
    public string? DrillDownConfiguration { get; set; }
    
    // Access Control
    public ReportVisibility Visibility { get; set; } = ReportVisibility.Private;
    
    [StringLength(1000)]
    public string? AllowedRoles { get; set; } // JSON array of roles
    
    [StringLength(1000)]
    public string? AllowedUsers { get; set; } // JSON array of user IDs
    
    // Versioning and Approval
    public int Version { get; set; } = 1;
    public bool RequiresApproval { get; set; } = false;
    
    [StringLength(50)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovedDate { get; set; }
    
    [StringLength(1000)]
    public string? ApprovalNotes { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessPurpose { get; set; }
    
    [StringLength(1000)]
    public string? Tags { get; set; } // Searchable tags
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? LayoutJson { get; set; }
    public string? StyleJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? Configuration
    {
        get => string.IsNullOrEmpty(ConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ConfigurationJson);
        set => ConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Layout
    {
        get => string.IsNullOrEmpty(LayoutJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(LayoutJson);
        set => LayoutJson = value == null ? null : JsonConvert.SerializeObject(value);
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
    
    public List<string>? RecipientsList
    {
        get => string.IsNullOrEmpty(Recipients) ? null : JsonConvert.DeserializeObject<List<string>>(Recipients);
        set => Recipients = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? TagsList
    {
        get => string.IsNullOrEmpty(Tags) ? null : JsonConvert.DeserializeObject<List<string>>(Tags);
        set => Tags = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Analytics? Analytics { get; set; }
    public virtual ICollection<ReportDefinition> ReportDefinitions { get; set; } = new List<ReportDefinition>();
}

public enum ReportType
{
    OperationalReport = 1,
    FinancialReport = 2,
    ComplianceReport = 3,
    PerformanceReport = 4,
    TrendReport = 5,
    KPIDashboard = 6,
    ExecutiveSummary = 7,
    DetailedAnalysis = 8,
    BenchmarkReport = 9,
    AnomalyReport = 10,
    CustomReport = 11
}

public enum ReportStatus
{
    Draft = 1,
    Active = 2,
    Scheduled = 3,
    Generating = 4,
    Generated = 5,
    Failed = 6,
    Archived = 7,
    Deprecated = 8
}

public enum ReportFormat
{
    Dashboard = 1,
    PDF = 2,
    Excel = 3,
    CSV = 4,
    JSON = 5,
    XML = 6,
    PowerBI = 7,
    Tableau = 8,
    WebReport = 9
}

public enum DataVisualizationType
{
    Table = 1,
    Chart = 2,
    Graph = 3,
    Gauge = 4,
    Map = 5,
    Timeline = 6,
    Heatmap = 7,
    Treemap = 8,
    Sankey = 9,
    Custom = 10
}

public enum ScheduleFrequency
{
    Hourly = 1,
    Daily = 2,
    Weekly = 3,
    Monthly = 4,
    Quarterly = 5,
    Yearly = 6,
    OnDemand = 7,
    Triggered = 8,
    Custom = 9
}

public enum ReportVisibility
{
    Private = 1,
    Organization = 2,
    Public = 3,
    RoleBased = 4
}

public enum ReportPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}