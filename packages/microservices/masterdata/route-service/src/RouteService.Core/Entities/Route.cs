using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class Route : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Origin { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Destination { get; set; } = string.Empty;
    
    [Range(0.1, double.MaxValue)]
    public double Distance { get; set; }
    
    public TimeSpan EstimatedDuration { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxVehicleWeight { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxVehicleHeight { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxVehicleWidth { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxVehicleLength { get; set; }
    
    [Required]
    public RouteType RouteType { get; set; } = RouteType.Standard;
    
    [Required]
    public RouteStatus Status { get; set; } = RouteStatus.Active;
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<RouteWaypoint> Waypoints { get; set; } = new List<RouteWaypoint>();
    public virtual ICollection<RouteRestriction> Restrictions { get; set; } = new List<RouteRestriction>();
    public virtual ICollection<RouteCondition> Conditions { get; set; } = new List<RouteCondition>();
    public virtual ICollection<RouteToll> Tolls { get; set; } = new List<RouteToll>();
    public virtual ICollection<RoutePerformance> Performance { get; set; } = new List<RoutePerformance>();
    public virtual ICollection<RouteHazmat> HazmatRestrictions { get; set; } = new List<RouteHazmat>();
    public virtual ICollection<RouteSchedule> Schedules { get; set; } = new List<RouteSchedule>();
}

public enum RouteType
{
    Standard,
    Express,
    Highway,
    Local,
    Scenic,
    Commercial,
    Hazmat
}

public enum RouteStatus
{
    Active,
    Inactive,
    UnderMaintenance,
    Closed,
    Restricted
}