using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class CapacityManagement : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string WeighbridgeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public int CurrentLoad { get; set; } = 0;
    
    [Required]
    public int MaxCapacity { get; set; }
    
    [Required]
    public int HourlyCapacity { get; set; }
    
    [Required]
    public decimal UtilizationRate { get; set; } = 0;
    
    [Required]
    public int VehiclesInQueue { get; set; } = 0;
    
    [Required]
    public TimeSpan EstimatedWaitTime { get; set; } = TimeSpan.Zero;
    
    [Required]
    public TimeSpan AverageProcessingTime { get; set; } = TimeSpan.Zero;
    
    public DateTime? NextAvailableSlot { get; set; }
    
    public List<CapacityForecast> Forecasts { get; set; } = new();
    
    public List<LoadBalancingMetric> LoadBalancingMetrics { get; set; } = new();
    
    public CapacityAlert? Alert { get; set; }
    
    public Dictionary<string, decimal> HourlyUtilization { get; set; } = new();
    
    public List<string> BottleneckFactors { get; set; } = new();
    
    public CapacityRecommendation? Recommendation { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CapacityForecast
{
    [Required]
    public DateTime ForecastDate { get; set; }
    
    [Required]
    public int PredictedLoad { get; set; }
    
    [Required]
    public decimal PredictedUtilization { get; set; }
    
    [Required]
    public TimeSpan PredictedWaitTime { get; set; }
    
    [Required]
    public ForecastConfidence Confidence { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class LoadBalancingMetric
{
    [Required]
    [MaxLength(100)]
    public required string MetricName { get; set; }
    
    [Required]
    public decimal Value { get; set; }
    
    [Required]
    public DateTime MeasuredAt { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}

public class CapacityAlert
{
    [Required]
    public AlertSeverity Severity { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Message { get; set; }
    
    [Required]
    public DateTime TriggeredAt { get; set; }
    
    public DateTime? ResolvedAt { get; set; }
    
    [MaxLength(100)]
    public string? ResolvedBy { get; set; }
    
    [MaxLength(1000)]
    public string? Resolution { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public List<string> NotificationsSent { get; set; } = new();
}

public class CapacityRecommendation
{
    [Required]
    [MaxLength(100)]
    public required string Type { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Description { get; set; }
    
    [Required]
    public RecommendationPriority Priority { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ImplementedAt { get; set; }
    
    [MaxLength(100)]
    public string? ImplementedBy { get; set; }
    
    public decimal? EstimatedImpact { get; set; }
    
    [MaxLength(1000)]
    public string? Implementation { get; set; }
    
    public bool IsImplemented { get; set; } = false;
}

public enum ForecastConfidence
{
    Low,
    Medium,
    High,
    VeryHigh
}

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum RecommendationPriority
{
    Low,
    Normal,
    High,
    Urgent
}