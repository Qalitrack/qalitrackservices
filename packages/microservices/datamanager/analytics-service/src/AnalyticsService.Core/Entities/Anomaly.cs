using System.ComponentModel.DataAnnotations;

namespace AnalyticsService.Core.Entities;

public class Anomaly
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
    
    [MaxLength(50)]
    public string? LocationId { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public double ActualValue { get; set; }
    
    public double ExpectedValue { get; set; }
    
    public double Deviation { get; set; }
    
    public double DeviationPercentage { get; set; }
    
    public double ZScore { get; set; }
    
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    
    [MaxLength(50)]
    public string DetectionMethod { get; set; } = string.Empty; // ZScore, IQR, ML, Custom
    
    [MaxLength(50)]
    public string AnomalyType { get; set; } = string.Empty; // Spike, Drop, Drift, Pattern
    
    public double ConfidenceScore { get; set; }
    
    public Dictionary<string, object> Context { get; set; } = new();
    
    public Dictionary<string, object> DetectionParameters { get; set; } = new();
    
    [MaxLength(20)]
    public string Status { get; set; } = "Open"; // Open, Investigating, Resolved, FalsePositive
    
    public string? Resolution { get; set; }
    
    public string? RootCause { get; set; }
    
    [MaxLength(50)]
    public string? AssignedTo { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(50)]
    public string? ResolvedBy { get; set; }
    
    public List<string> RelatedAnomalies { get; set; } = new(); // IDs of related anomalies
    
    public Dictionary<string, object> ImpactAnalysis { get; set; } = new();
    
    public bool IsNotified { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}