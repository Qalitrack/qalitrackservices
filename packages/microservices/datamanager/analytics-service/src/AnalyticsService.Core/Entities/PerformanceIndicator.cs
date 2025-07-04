using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class PerformanceIndicator
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Operational, Financial, Quality, Compliance
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    public double CurrentValue { get; set; }
    
    public double? TargetValue { get; set; }
    
    public double? ThresholdGreen { get; set; }
    
    public double? ThresholdYellow { get; set; }
    
    public double? ThresholdRed { get; set; }
    
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string CalculationMethod { get; set; } = string.Empty; // Sum, Average, Ratio, Custom
    
    public string CalculationFormula { get; set; } = string.Empty;
    
    public List<string> DependentMetrics { get; set; } = new();
    
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty; // Green, Yellow, Red, Unknown
    
    public double? PreviousPeriodValue { get; set; }
    
    public double? PercentageChange { get; set; }
    
    [MaxLength(20)]
    public string Trend { get; set; } = string.Empty; // Improving, Declining, Stable
    
    public Dictionary<string, object> Benchmarks { get; set; } = new();
    
    public DateTime LastCalculated { get; set; }
    
    public DateTime LastUpdated { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int DisplayOrder { get; set; }
    
    public DateTime CreatedAt { get; set; }
}