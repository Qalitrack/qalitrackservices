using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class RouteConfiguration : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string RouteId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Origin { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Destination { get; set; }
    
    [Required]
    public decimal Distance { get; set; }
    
    [Required]
    public TimeSpan EstimatedDuration { get; set; }
    
    [Required]
    public RouteStatus Status { get; set; } = RouteStatus.Active;
    
    [Required]
    public RoutePriority Priority { get; set; } = RoutePriority.Normal;
    
    public decimal MaxWeightLimit { get; set; } = 0;
    
    public List<string> AllowedVehicleTypes { get; set; } = new();
    
    public List<string> RestrictedVehicleTypes { get; set; } = new();
    
    public List<RouteWaypoint> Waypoints { get; set; } = new();
    
    public List<RouteRestriction> Restrictions { get; set; } = new();
    
    public RouteTrafficCondition TrafficCondition { get; set; } = RouteTrafficCondition.Normal;
    
    public decimal CostPerKm { get; set; } = 0;
    
    public decimal TollCost { get; set; } = 0;
    
    public TimeSpan[] OperatingHours { get; set; } = Array.Empty<TimeSpan>();
    
    public List<string> WeatherRestrictions { get; set; } = new();
    
    [MaxLength(1000)]
    public string? SafetyNotes { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    public RoutePerformanceMetrics PerformanceMetrics { get; set; } = new();
}

public class RouteWaypoint
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [Required]
    public decimal Latitude { get; set; }
    
    [Required]
    public decimal Longitude { get; set; }
    
    public int Sequence { get; set; }
    
    public bool IsOptional { get; set; } = false;
    
    public TimeSpan? StopDuration { get; set; }
}

public class RouteRestriction
{
    [Required]
    [MaxLength(100)]
    public required string Type { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Description { get; set; }
    
    public DateTime? StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public TimeSpan? StartTime { get; set; }
    
    public TimeSpan? EndTime { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class RoutePerformanceMetrics
{
    public decimal AverageSpeed { get; set; } = 0;
    
    public decimal AverageDelay { get; set; } = 0;
    
    public decimal ReliabilityScore { get; set; } = 0;
    
    public int UsageCount { get; set; } = 0;
    
    public decimal CustomerSatisfactionScore { get; set; } = 0;
    
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
}

public enum RouteStatus
{
    Active,
    Inactive,
    Suspended,
    UnderConstruction,
    Closed
}

public enum RoutePriority
{
    Low,
    Normal,
    High,
    Critical
}

public enum RouteTrafficCondition
{
    Light,
    Normal,
    Heavy,
    Congested,
    Blocked
}