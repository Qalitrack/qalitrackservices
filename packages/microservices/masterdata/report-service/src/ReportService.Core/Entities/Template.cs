using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ReportService.Core.Entities;

public class Template : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string TemplateName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public TemplateType TemplateType { get; set; }
    
    public TemplateStatus Status { get; set; } = TemplateStatus.Draft;
    
    // Template Configuration
    [StringLength(50)]
    public string? BaseTemplateId { get; set; } // For template inheritance
    
    [StringLength(100)]
    public string? Version { get; set; } = "1.0.0";
    
    public bool IsSystemTemplate { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    
    // Layout Configuration
    public ReportFormat DefaultFormat { get; set; } = ReportFormat.PDF;
    public ReportLayout DefaultLayout { get; set; } = ReportLayout.Portrait;
    
    [StringLength(20)]
    public string? PageSize { get; set; } = "A4"; // A4, A3, Letter, etc.
    
    public decimal? PageWidth { get; set; }
    public decimal? PageHeight { get; set; }
    
    public decimal? MarginTop { get; set; } = 20;
    public decimal? MarginBottom { get; set; } = 20;
    public decimal? MarginLeft { get; set; } = 20;
    public decimal? MarginRight { get; set; } = 20;
    
    // Header Configuration
    public bool HasHeader { get; set; } = true;
    
    [StringLength(1000)]
    public string? HeaderContent { get; set; }
    
    public decimal? HeaderHeight { get; set; } = 50;
    
    [StringLength(500)]
    public string? HeaderImagePath { get; set; }
    
    // Footer Configuration
    public bool HasFooter { get; set; } = true;
    
    [StringLength(1000)]
    public string? FooterContent { get; set; }
    
    public decimal? FooterHeight { get; set; } = 30;
    
    public bool ShowPageNumbers { get; set; } = true;
    
    [StringLength(50)]
    public string? PageNumberFormat { get; set; } = "Page {0} of {1}";
    
    // Content Structure
    [StringLength(5000)]
    public string? TemplateStructure { get; set; } // JSON layout definition
    
    [StringLength(5000)]
    public string? DefaultSections { get; set; } // JSON array of section definitions
    
    [StringLength(1000)]
    public string? RequiredDataSources { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? SupportedParameters { get; set; } // JSON object
    
    // Styling
    [StringLength(5000)]
    public string? StyleDefinitions { get; set; } // CSS or styling JSON
    
    [StringLength(100)]
    public string? FontFamily { get; set; } = "Arial";
    
    public int? FontSize { get; set; } = 12;
    
    [StringLength(50)]
    public string? PrimaryColor { get; set; } = "#000000";
    
    [StringLength(50)]
    public string? SecondaryColor { get; set; } = "#666666";
    
    [StringLength(50)]
    public string? AccentColor { get; set; } = "#0066CC";
    
    // Customization Options
    public bool AllowCustomization { get; set; } = true;
    
    [StringLength(1000)]
    public string? CustomizableElements { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? LockedElements { get; set; } // JSON array
    
    public bool AllowSectionReordering { get; set; } = true;
    public bool AllowSectionAddition { get; set; } = false;
    public bool AllowSectionRemoval { get; set; } = false;
    
    // Content Configuration
    public bool IncludeTitlePage { get; set; } = true;
    public bool IncludeTableOfContents { get; set; } = false;
    public bool IncludeExecutiveSummary { get; set; } = false;
    public bool IncludeAppendix { get; set; } = false;
    
    // Data Binding
    [StringLength(1000)]
    public string? DataMappings { get; set; } // JSON object for field mappings
    
    [StringLength(1000)]
    public string? CalculatedFields { get; set; } // JSON array of calculated field definitions
    
    [StringLength(1000)]
    public string? ConditionalFormatting { get; set; } // JSON array of formatting rules
    
    // Chart and Visualization Configuration
    public bool SupportsCharts { get; set; } = false;
    
    [StringLength(1000)]
    public string? DefaultChartTypes { get; set; } // JSON array
    
    [StringLength(1000)]
    public string? ChartConfiguration { get; set; } // JSON object
    
    // Business Context
    [StringLength(100)]
    public string? Industry { get; set; }
    
    [StringLength(100)]
    public string? Category { get; set; }
    
    [StringLength(1000)]
    public string? UseCases { get; set; } // JSON array
    
    [StringLength(100)]
    public string? TemplateOwner { get; set; }
    
    [StringLength(100)]
    public string? ApprovedBy { get; set; }
    
    public DateTime? ApprovalDate { get; set; }
    
    // Usage and Performance
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsedDate { get; set; }
    
    [StringLength(50)]
    public string? LastUsedBy { get; set; }
    
    public TimeSpan? AverageGenerationTime { get; set; }
    public decimal? AverageFileSizeKB { get; set; }
    
    // Validation and Quality
    [StringLength(1000)]
    public string? ValidationRules { get; set; } // JSON array
    
    public bool RequiresDataValidation { get; set; } = true;
    public bool HasQualityChecks { get; set; } = false;
    
    [StringLength(1000)]
    public string? QualityMetrics { get; set; } // JSON object
    
    // Localization
    public bool SupportsLocalization { get; set; } = false;
    
    [StringLength(1000)]
    public string? SupportedLocales { get; set; } // JSON array
    
    [StringLength(20)]
    public string? DefaultLocale { get; set; } = "en-US";
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? LayoutJson { get; set; }
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
    
    public Dictionary<string, object>? Metadata
    {
        get => string.IsNullOrEmpty(MetadataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetadataJson);
        set => MetadataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? RequiredDataSourcesList
    {
        get => string.IsNullOrEmpty(RequiredDataSources) ? null : JsonConvert.DeserializeObject<List<string>>(RequiredDataSources);
        set => RequiredDataSources = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? SupportedParametersObject
    {
        get => string.IsNullOrEmpty(SupportedParameters) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(SupportedParameters);
        set => SupportedParameters = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? CustomizableElementsList
    {
        get => string.IsNullOrEmpty(CustomizableElements) ? null : JsonConvert.DeserializeObject<List<string>>(CustomizableElements);
        set => CustomizableElements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? LockedElementsList
    {
        get => string.IsNullOrEmpty(LockedElements) ? null : JsonConvert.DeserializeObject<List<string>>(LockedElements);
        set => LockedElements = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? DataMappingsObject
    {
        get => string.IsNullOrEmpty(DataMappings) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(DataMappings);
        set => DataMappings = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? CalculatedFieldsList
    {
        get => string.IsNullOrEmpty(CalculatedFields) ? null : JsonConvert.DeserializeObject<List<object>>(CalculatedFields);
        set => CalculatedFields = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ConditionalFormattingList
    {
        get => string.IsNullOrEmpty(ConditionalFormatting) ? null : JsonConvert.DeserializeObject<List<object>>(ConditionalFormatting);
        set => ConditionalFormatting = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DefaultChartTypesList
    {
        get => string.IsNullOrEmpty(DefaultChartTypes) ? null : JsonConvert.DeserializeObject<List<string>>(DefaultChartTypes);
        set => DefaultChartTypes = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ChartConfigurationObject
    {
        get => string.IsNullOrEmpty(ChartConfiguration) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ChartConfiguration);
        set => ChartConfiguration = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? UseCasesList
    {
        get => string.IsNullOrEmpty(UseCases) ? null : JsonConvert.DeserializeObject<List<string>>(UseCases);
        set => UseCases = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<object>? ValidationRulesList
    {
        get => string.IsNullOrEmpty(ValidationRules) ? null : JsonConvert.DeserializeObject<List<object>>(ValidationRules);
        set => ValidationRules = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? QualityMetricsObject
    {
        get => string.IsNullOrEmpty(QualityMetrics) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(QualityMetrics);
        set => QualityMetrics = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? SupportedLocalesList
    {
        get => string.IsNullOrEmpty(SupportedLocales) ? null : JsonConvert.DeserializeObject<List<string>>(SupportedLocales);
        set => SupportedLocales = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Template? BaseTemplate { get; set; }
    public virtual ICollection<Template> DerivedTemplates { get; set; } = new List<Template>();
    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
}

public enum TemplateType
{
    Standard = 1,
    Executive = 2,
    Financial = 3,
    Operational = 4,
    Compliance = 5,
    Analytics = 6,
    Dashboard = 7,
    Letter = 8,
    Invoice = 9,
    Certificate = 10,
    Custom = 11
}

public enum TemplateStatus
{
    Draft = 1,
    UnderReview = 2,
    Approved = 3,
    Active = 4,
    Deprecated = 5,
    Archived = 6
}