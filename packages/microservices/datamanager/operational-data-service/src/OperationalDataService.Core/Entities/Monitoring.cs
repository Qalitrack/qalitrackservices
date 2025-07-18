using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OperationalDataService.Core.Entities;

public class Monitoring : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string? OperationalId { get; set; }
    
    [StringLength(50)]
    public string? ProcessId { get; set; }
    
    [Required]
    public MonitoringType MonitoringType { get; set; }
    
    [Required]
    public MetricType MetricType { get; set; }
    
    public decimal MetricValue { get; set; }
    
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [StringLength(100)]
    public string? Source { get; set; }
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    // Threshold and alerting
    public decimal? MinThreshold { get; set; }
    public decimal? MaxThreshold { get; set; }
    public decimal? WarningThreshold { get; set; }
    public decimal? CriticalThreshold { get; set; }
    
    public AlertLevel AlertLevel { get; set; } = AlertLevel.None;
    
    public bool IsAlert { get; set; } = false;
    public bool IsAcknowledged { get; set; } = false;
    
    [StringLength(50)]
    public string? AcknowledgedBy { get; set; }
    
    public DateTime? AcknowledgedAt { get; set; }
    
    [StringLength(1000)]
    public string? AlertMessage { get; set; }
    
    // Performance tracking
    public TimeSpan? Duration { get; set; }
    public long? MemoryUsage { get; set; }
    public double? CpuUsage { get; set; }
    public long? NetworkBytesIn { get; set; }
    public long? NetworkBytesOut { get; set; }
    
    // Business metrics
    public int? TransactionCount { get; set; }
    public int? ErrorCount { get; set; }
    public decimal? SuccessRate { get; set; }
    public decimal? ThroughputRate { get; set; }
    
    // JSON properties for flexible metric data
    public string? MetricDataJson { get; set; }
    public string? ContextDataJson { get; set; }
    
    // Computed properties
    public Dictionary<string, object>? MetricData
    {
        get => string.IsNullOrEmpty(MetricDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(MetricDataJson);
        set => MetricDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? ContextData
    {
        get => string.IsNullOrEmpty(ContextDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ContextDataJson);
        set => ContextDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    // Navigation Properties
    public virtual Operational? Operation { get; set; }
    public virtual Process? Process { get; set; }
}

public enum MonitoringType
{
    SystemMetric = 1,
    BusinessMetric = 2,
    PerformanceMetric = 3,
    SecurityMetric = 4,
    HealthCheck = 5,
    UserActivity = 6,
    DataQuality = 7,
    Integration = 8,
    Compliance = 9,
    CustomMetric = 10
}

public enum MetricType
{
    Counter = 1,
    Gauge = 2,
    Timer = 3,
    Histogram = 4,
    Rate = 5,
    Percentage = 6,
    Duration = 7,
    Size = 8,
    Count = 9,
    Status = 10
}

public enum AlertLevel
{
    None = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4,
    Fatal = 5
}