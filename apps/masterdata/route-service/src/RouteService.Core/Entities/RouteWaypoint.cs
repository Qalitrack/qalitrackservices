using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteWaypoint : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }
    
    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Sequence { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? DistanceFromPrevious { get; set; }
    
    public TimeSpan? TimeFromPrevious { get; set; }
    
    [Required]
    public WaypointType Type { get; set; } = WaypointType.Intermediate;
    
    public bool IsRequired { get; set; } = true;
    
    [StringLength(100)]
    public string? Address { get; set; }
    
    [StringLength(50)]
    public string? City { get; set; }
    
    [StringLength(50)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(50)]
    public string? Country { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum WaypointType
{
    Origin,
    Destination,
    Intermediate,
    RestStop,
    FuelStop,
    Checkpoint,
    Toll,
    WeighStation,
    BorderCrossing,
    ServiceCenter
}