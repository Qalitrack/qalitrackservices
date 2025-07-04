using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class BenchmarkData
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string BenchmarkType { get; set; } = string.Empty; // Industry, Regional, Historical, Target
    
    [MaxLength(100)]
    public string BenchmarkSource { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Industry { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Region { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string OrganizationSize { get; set; } = string.Empty; // Small, Medium, Large, Enterprise
    
    public double BenchmarkValue { get; set; }
    
    public double? P25Value { get; set; } // 25th percentile
    
    public double? P50Value { get; set; } // Median
    
    public double? P75Value { get; set; } // 75th percentile
    
    public double? P90Value { get; set; } // 90th percentile
    
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime BenchmarkPeriodStart { get; set; }
    
    public DateTime BenchmarkPeriodEnd { get; set; }
    
    public int SampleSize { get; set; }
    
    public Dictionary<string, object> Criteria { get; set; } = new();
    
    public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
    
    public string DataQuality { get; set; } = string.Empty; // High, Medium, Low
    
    public double ConfidenceLevel { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    [MaxLength(50)]
    public string CreatedBy { get; set; } = string.Empty;
}