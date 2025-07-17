using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AnalyticsService.Core.Entities;

public class Analytics : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string AnalyticsName { get; set; } = string.Empty;
    
    [Required]
    public AnalyticsType AnalyticsType { get; set; }
    
    [Required]
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    public AnalyticsStatus Status { get; set; } = AnalyticsStatus.Active;
    
    // Data Aggregation Configuration
    [Required]
    public AggregationType AggregationType { get; set; }
    
    [Required]
    public TimeGranularity TimeGranularity { get; set; }
    
    public DateTime? DataStartDate { get; set; }
    public DateTime? DataEndDate { get; set; }
    
    [StringLength(1000)]
    public string? DataSources { get; set; } // JSON array of data source configurations
    
    [StringLength(1000)]
    public string? FilterCriteria { get; set; } // JSON object for filtering data
    
    // Processing Configuration
    public bool IsRealTime { get; set; } = false;
    public bool IsScheduled { get; set; } = false;
    
    public int? RefreshIntervalMinutes { get; set; }
    public DateTime? LastRefreshDate { get; set; }
    public DateTime? NextRefreshDate { get; set; }
    
    [StringLength(100)]
    public string? RefreshTrigger { get; set; } // Manual, Scheduled, DataChange, API
    
    // Results and Metrics
    public long? TotalRecordsProcessed { get; set; }
    public DateTime? LastCalculationDate { get; set; }
    
    [StringLength(1000)]
    public string? CalculationErrors { get; set; }
    
    public bool HasAnomalies { get; set; } = false;
    public int? AnomalyCount { get; set; }
    
    // Performance Metrics
    public TimeSpan? LastProcessingDuration { get; set; }
    public TimeSpan? AverageProcessingDuration { get; set; }
    
    public long? MemoryUsedBytes { get; set; }
    public decimal? CpuUsagePercentage { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessCategory { get; set; }
    
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    public AnalyticsPriority Priority { get; set; } = AnalyticsPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessPurpose { get; set; }
    
    // Retention and Archival
    public int? DataRetentionDays { get; set; }
    public bool AutoArchive { get; set; } = false;
    public DateTime? LastArchiveDate { get; set; }
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    public string? ResultsJson { get; set; }
    
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
    
    public Dictionary<string, object>? Results
    {
        get => string.IsNullOrEmpty(ResultsJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ResultsJson);
        set => ResultsJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DataSourcesList
    {
        get => string.IsNullOrEmpty(DataSources) ? null : JsonConvert.DeserializeObject<List<string>>(DataSources);
        set => DataSources = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? FilterCriteriaObject
    {
        get => string.IsNullOrEmpty(FilterCriteria) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(FilterCriteria);
        set => FilterCriteria = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual ICollection<AnalyticsMetric> Metrics { get; set; } = new List<AnalyticsMetric>();
    public virtual ICollection<TrendAnalysis> TrendAnalyses { get; set; } = new List<TrendAnalysis>();
    public virtual ICollection<Anomaly> Anomalies { get; set; } = new List<Anomaly>();
    public virtual ICollection<BenchmarkData> Benchmarks { get; set; } = new List<BenchmarkData>();
}

public enum AnalyticsType
{
    BusinessIntelligence = 1,
    OperationalAnalytics = 2,
    PerformanceMetrics = 3,
    TrendAnalysis = 4,
    PredictiveAnalytics = 5,
    ComplianceAnalytics = 6,
    FinancialAnalytics = 7,
    CustomerAnalytics = 8,
    SupplyChainAnalytics = 9,
    RiskAnalytics = 10,
    QualityAnalytics = 11,
    CustomAnalytics = 12
}

public enum AnalyticsStatus
{
    Draft = 1,
    Active = 2,
    Paused = 3,
    Processing = 4,
    Completed = 5,
    Failed = 6,
    Archived = 7,
    Deprecated = 8
}

public enum AggregationType
{
    Sum = 1,
    Average = 2,
    Count = 3,
    Minimum = 4,
    Maximum = 5,
    StandardDeviation = 6,
    Variance = 7,
    Median = 8,
    Percentile = 9,
    DistinctCount = 10,
    FirstValue = 11,
    LastValue = 12,
    CustomFormula = 13
}

public enum TimeGranularity
{
    Minute = 1,
    Hour = 2,
    Day = 3,
    Week = 4,
    Month = 5,
    Quarter = 6,
    Year = 7,
    RealTime = 8,
    Custom = 9
}

public enum AnalyticsPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4,
    Emergency = 5
}