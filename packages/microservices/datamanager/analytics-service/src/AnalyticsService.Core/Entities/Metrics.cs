using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AnalyticsService.Core.Entities;

public class Metrics : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? AnalyticsId { get; set; }
    
    [Required]
    public MetricType MetricType { get; set; }
    
    [Required]
    public MetricCategory Category { get; set; }
    
    [StringLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Metric Definition
    [Required]
    [StringLength(1000)]
    public string CalculationFormula { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string DisplayFormat { get; set; } = "0.00"; // Number format
    
    // KPI Configuration
    public bool IsKPI { get; set; } = false;
    public KPIType? KPIType { get; set; }
    
    public decimal? TargetValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    
    public decimal? GoodThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    
    [StringLength(20)]
    public string? ThresholdDirection { get; set; } // Higher is better, Lower is better
    
    // Current Values
    public decimal? CurrentValue { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? PercentageChange { get; set; }
    
    public DateTime? LastCalculationDate { get; set; }
    public DateTime? NextCalculationDate { get; set; }
    
    // Status and Performance
    public MetricStatus Status { get; set; } = MetricStatus.Good;
    public CalculationStatus CalculationStatus { get; set; } = CalculationStatus.Pending;
    
    [StringLength(1000)]
    public string? LastCalculationError { get; set; }
    
    public int CalculationRetryCount { get; set; } = 0;
    public TimeSpan? LastCalculationDuration { get; set; }
    
    // Trend Analysis
    public TrendDirection TrendDirection { get; set; } = TrendDirection.Stable;
    public decimal? TrendStrength { get; set; } // 0-100
    
    public decimal? MovingAverage7Days { get; set; }
    public decimal? MovingAverage30Days { get; set; }
    public decimal? MovingAverage90Days { get; set; }
    
    // Aggregation Configuration
    public AggregationType AggregationType { get; set; } = AggregationType.Sum;
    public TimeGranularity Granularity { get; set; } = TimeGranularity.Day;
    
    public bool IsRealTime { get; set; } = false;
    public int? RefreshIntervalMinutes { get; set; }
    
    // Business Context
    [StringLength(100)]
    public string? BusinessOwner { get; set; }
    
    [StringLength(100)]
    public string? TechnicalOwner { get; set; }
    
    public MetricPriority Priority { get; set; } = MetricPriority.Medium;
    
    [StringLength(1000)]
    public string? BusinessPurpose { get; set; }
    
    // Data Sources
    [StringLength(1000)]
    public string? DataSources { get; set; }
    
    [StringLength(1000)]
    public string? Dependencies { get; set; } // Other metrics this depends on
    
    // Alerting
    public bool AlertingEnabled { get; set; } = false;
    
    [StringLength(500)]
    public string? AlertRecipients { get; set; }
    
    [StringLength(1000)]
    public string? AlertMessage { get; set; }
    
    public DateTime? LastAlertSent { get; set; }
    
    // JSON properties for flexible configuration
    public string? ConfigurationJson { get; set; }
    public string? MetadataJson { get; set; }
    public string? HistoricalDataJson { get; set; }
    
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
    
    public List<HistoricalMetricValue>? HistoricalData
    {
        get => string.IsNullOrEmpty(HistoricalDataJson) ? null : JsonConvert.DeserializeObject<List<HistoricalMetricValue>>(HistoricalDataJson);
        set => HistoricalDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DataSourcesList
    {
        get => string.IsNullOrEmpty(DataSources) ? null : JsonConvert.DeserializeObject<List<string>>(DataSources);
        set => DataSources = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public List<string>? DependenciesList
    {
        get => string.IsNullOrEmpty(Dependencies) ? null : JsonConvert.DeserializeObject<List<string>>(Dependencies);
        set => Dependencies = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Analytics? Analytics { get; set; }
    public virtual ICollection<AnalyticsMetric> AnalyticsMetrics { get; set; } = new List<AnalyticsMetric>();
}

public class HistoricalMetricValue
{
    public DateTime Timestamp { get; set; }
    public decimal Value { get; set; }
    public string? Note { get; set; }
}

public enum MetricType
{
    BusinessMetric = 1,
    OperationalMetric = 2,
    FinancialMetric = 3,
    QualityMetric = 4,
    PerformanceMetric = 5,
    ComplianceMetric = 6,
    CustomerMetric = 7,
    SupplyChainMetric = 8,
    RiskMetric = 9,
    EnvironmentalMetric = 10,
    SafetyMetric = 11,
    CustomMetric = 12
}

public enum MetricCategory
{
    Volume = 1,
    Efficiency = 2,
    Quality = 3,
    Cost = 4,
    Revenue = 5,
    Utilization = 6,
    Compliance = 7,
    Customer = 8,
    Employee = 9,
    Environment = 10,
    Safety = 11,
    Innovation = 12
}

public enum KPIType
{
    LeadingIndicator = 1,
    LaggingIndicator = 2,
    BalancedScorecard = 3,
    OperationalKPI = 4,
    StrategicKPI = 5,
    TacticalKPI = 6
}

public enum MetricStatus
{
    Good = 1,
    Warning = 2,
    Critical = 3,
    Unknown = 4,
    NoData = 5
}

public enum CalculationStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Scheduled = 6
}

public enum TrendDirection
{
    StronglyUp = 1,
    Up = 2,
    SlightlyUp = 3,
    Stable = 4,
    SlightlyDown = 5,
    Down = 6,
    StronglyDown = 7
}

public enum MetricPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}