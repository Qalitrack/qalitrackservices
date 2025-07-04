using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class TrendAnalysis
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? WeighbridgeId { get; set; }
    
    [Required]
    public DateTime AnalysisPeriodStart { get; set; }
    
    [Required]
    public DateTime AnalysisPeriodEnd { get; set; }
    
    [MaxLength(20)]
    public string TrendDirection { get; set; } = string.Empty; // Upward, Downward, Stable, Volatile
    
    public double TrendStrength { get; set; } // 0-1, where 1 is strongest trend
    
    public double Slope { get; set; } // Linear regression slope
    
    public double RSquared { get; set; } // Coefficient of determination
    
    public double? SeasonalityFactor { get; set; }
    
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    
    public Dictionary<string, double> StatisticalMeasures { get; set; } = new();
    
    public List<TrendBreakpoint> Breakpoints { get; set; } = new(); // Significant changes in trend
    
    public Dictionary<string, object> Insights { get; set; } = new();
    
    public double ConfidenceLevel { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public double? PredictedValue { get; set; }
    public double? ConfidenceInterval { get; set; }
}

public class TrendBreakpoint
{
    public DateTime Timestamp { get; set; }
    public double ValueBefore { get; set; }
    public double ValueAfter { get; set; }
    public double ChangePercentage { get; set; }
    public string ChangeType { get; set; } = string.Empty; // Sudden, Gradual
    public string? PossibleCause { get; set; }
}