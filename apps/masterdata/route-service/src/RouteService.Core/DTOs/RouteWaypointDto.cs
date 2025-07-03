using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteWaypointDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Sequence { get; set; }
    public double? DistanceFromPrevious { get; set; }
    public TimeSpan? TimeFromPrevious { get; set; }
    public WaypointType Type { get; set; }
    public bool IsRequired { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteWaypointRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Sequence { get; set; }
    public double? DistanceFromPrevious { get; set; }
    public TimeSpan? TimeFromPrevious { get; set; }
    public WaypointType Type { get; set; } = WaypointType.Intermediate;
    public bool IsRequired { get; set; } = true;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}

public class UpdateRouteWaypointRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? Sequence { get; set; }
    public double? DistanceFromPrevious { get; set; }
    public TimeSpan? TimeFromPrevious { get; set; }
    public WaypointType? Type { get; set; }
    public bool? IsRequired { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}