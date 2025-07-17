using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AnalyticsService.Core.Entities;

public class Dashboard : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string DashboardName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? AnalyticsId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public DashboardType DashboardType { get; set; } = DashboardType.Operational;
    public DashboardStatus Status { get; set; } = DashboardStatus.Draft;
    
    // Layout and Design
    public LayoutType LayoutType { get; set; } = LayoutType.Grid;
    
    [StringLength(20)]
    public string? Theme { get; set; } = "default";
    
    public int Columns { get; set; } = 12;
    public int Rows { get; set; } = 6;
    
    [StringLength(20)]
    public string? BackgroundColor { get; set; }
    
    [StringLength(500)]
    public string? CustomCSS { get; set; }
    
    // Refresh and Real-time
    public bool IsRealTime { get; set; } = false;
    public int? RefreshIntervalSeconds { get; set; }
    
    public DateTime? LastRefreshDate { get; set; }
    public DateTime? NextRefreshDate { get; set; }
    
    public bool AutoRefresh { get; set; } = false;
    
    // Access Control
    public DashboardVisibility Visibility { get; set; } = DashboardVisibility.Private;
    
    [StringLength(1000)]
    public string? AllowedRoles { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? AllowedUsers { get; set; } // JSON array
    
    public bool IsPublic { get; set; } = false;
    public bool RequiresLogin { get; set; } = true;
    
    // Interactivity
    public bool EnableFiltering { get; set; } = true;
    public bool EnableExport { get; set; } = true;
    public bool EnableDrillDown { get; set; } = false;
    public bool EnableFullScreen { get; set; } = true;
    
    [StringLength(1000)]
    public string? GlobalFilters { get; set; } // JSON configuration
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    public DashboardPriority Priority { get; set; } = DashboardPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessPurpose { get; set; }
    
    [StringLength(1000)]
    public string? Tags { get; set; } // Searchable tags
    
    // Usage Analytics
    public int ViewCount { get; set; } = 0;
    public DateTime? LastViewedDate { get; set; }
    
    [StringLength(50)]
    public string? LastViewedBy { get; set; }
    
    public int UniqueViewersCount { get; set; } = 0;
    public decimal? AverageViewDurationMinutes { get; set; }
    
    // Performance
    public TimeSpan? AverageLoadTime { get; set; }
    public DateTime? LastLoadTime { get; set; }
    
    [StringLength(1000)]
    public string? LastLoadError { get; set; }
    
    // Sharing and Embedding
    public bool AllowEmbedding { get; set; } = false;
    
    [StringLength(500)]
    public string? EmbedUrl { get; set; }
    
    [StringLength(100)]
    public string? ShareToken { get; set; }
    
    public DateTime? ShareTokenExpiry { get; set; }
    
    // Notifications and Alerts
    public bool EnableAlerts { get; set; } = false;
    
    [StringLength(1000)]
    public string? AlertConfiguration { get; set; }
    
    [StringLength(500)]
    public string? AlertRecipients { get; set; }
    
    // Mobile and Responsive
    public bool IsMobileOptimized { get; set; } = false;
    public bool IsResponsive { get; set; } = true;
    
    [StringLength(1000)]
    public string? MobileLayout { get; set; }
    
    // JSON properties for flexible configuration
    public string? LayoutConfigurationJson { get; set; }
    public string? WidgetConfigurationJson { get; set; }
    public string? StyleConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? LayoutConfiguration
    {
        get => string.IsNullOrEmpty(LayoutConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(LayoutConfigurationJson);
        set => LayoutConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? WidgetConfiguration
    {
        get => string.IsNullOrEmpty(WidgetConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(WidgetConfigurationJson);
        set => WidgetConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? StyleConfiguration
    {
        get => string.IsNullOrEmpty(StyleConfigurationJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(StyleConfigurationJson);
        set => StyleConfigurationJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
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
    
    // Navigation Properties
    public virtual Analytics? Analytics { get; set; }
    public virtual ICollection<DashboardWidget> Widgets { get; set; } = new List<DashboardWidget>();
}

public enum DashboardType
{
    Operational = 1,
    Executive = 2,
    Analytical = 3,
    KPI = 4,
    Financial = 5,
    Compliance = 6,
    Performance = 7,
    RealTime = 8,
    Strategic = 9,
    Tactical = 10,
    Custom = 11
}

public enum DashboardStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3,
    Published = 4,
    Archived = 5,
    UnderReview = 6,
    Deprecated = 7
}

public enum LayoutType
{
    Grid = 1,
    Freeform = 2,
    Tabs = 3,
    Accordion = 4,
    Carousel = 5,
    Masonry = 6,
    Flow = 7,
    Custom = 8
}

public enum DashboardVisibility
{
    Private = 1,
    Organization = 2,
    Public = 3,
    RoleBased = 4,
    Custom = 5
}

public enum DashboardPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}