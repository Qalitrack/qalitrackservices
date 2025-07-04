using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class MetricsAggregation
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string AggregationType { get; set; } = string.Empty; // Sum, Average, Count, Min, Max
    
    [Required]
    [MaxLength(20)]
    public string TimeGranularity { get; set; } = string.Empty; // Hour, Day, Week, Month
    
    [Required]
    public DateTime PeriodStart { get; set; }
    
    [Required]
    public DateTime PeriodEnd { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? WeighbridgeId { get; set; }
    
    [MaxLength(50)]
    public string? LocationId { get; set; }
    
    public double Value { get; set; }
    
    public int SampleCount { get; set; }
    
    public double? MinValue { get; set; }
    
    public double? MaxValue { get; set; }
    
    public double? StandardDeviation { get; set; }
    
    public Dictionary<string, object> Dimensions { get; set; } = new();
    
    public Dictionary<string, double> Percentiles { get; set; } = new(); // P50, P90, P95, P99
    
    public bool IsComplete { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
}