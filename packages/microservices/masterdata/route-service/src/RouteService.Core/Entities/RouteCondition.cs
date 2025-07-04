using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteCondition : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    public ConditionType Type { get; set; }
    
    [Required]
    public ConditionStatus Status { get; set; } = ConditionStatus.Normal;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ResolvedAt { get; set; }
    
    [StringLength(100)]
    public string? ReportedBy { get; set; }
    
    [Range(1, 10)]
    public int? SeverityLevel { get; set; }
    
    [Range(-90, 90)]
    public double? Latitude { get; set; }
    
    [Range(-180, 180)]
    public double? Longitude { get; set; }
    
    [StringLength(200)]
    public string? Location { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? EstimatedDelay { get; set; } // in minutes
    
    [StringLength(500)]
    public string? RecommendedAction { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    public string? WeatherCondition { get; set; }
    
    [Range(-50, 60)]
    public double? Temperature { get; set; }
    
    [Range(0, 100)]
    public int? Visibility { get; set; } // in kilometers
    
    [StringLength(200)]
    public string? TrafficInfo { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum ConditionType
{
    Traffic,
    Weather,
    Construction,
    Accident,
    RoadClosure,
    Flooding,
    Snow,
    Ice,
    Fog,
    Wind,
    Maintenance,
    Event,
    Emergency
}

public enum ConditionStatus
{
    Normal,
    Advisory,
    Warning,
    Critical,
    Closed
}