using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class AnalyticsMetric
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MetricType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public double Value { get; set; }
    
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string OrganizationId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? WeighbridgeId { get; set; }
    
    [MaxLength(50)]
    public string? LocationId { get; set; }
    
    public double? PreviousValue { get; set; }
    
    public double? PercentageChange { get; set; }
    
    [MaxLength(20)]
    public string TrendDirection { get; set; } = string.Empty; // Up, Down, Stable
    
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    public Dictionary<string, object> Breakdown { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}