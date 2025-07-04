using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double Distance { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public double? MaxVehicleWeight { get; set; }
    public double? MaxVehicleHeight { get; set; }
    public double? MaxVehicleWidth { get; set; }
    public double? MaxVehicleLength { get; set; }
    public RouteType RouteType { get; set; }
    public RouteStatus Status { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<RouteWaypointDto> Waypoints { get; set; } = new();
    public List<RouteRestrictionDto> Restrictions { get; set; } = new();
    public List<RouteConditionDto> Conditions { get; set; } = new();
    public List<RouteTollDto> Tolls { get; set; } = new();
    public List<RoutePerformanceDto> Performance { get; set; } = new();
    public List<RouteHazmatDto> HazmatRestrictions { get; set; } = new();
    public List<RouteScheduleDto> Schedules { get; set; } = new();
}

public class CreateRouteRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double Distance { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public double? MaxVehicleWeight { get; set; }
    public double? MaxVehicleHeight { get; set; }
    public double? MaxVehicleWidth { get; set; }
    public double? MaxVehicleLength { get; set; }
    public RouteType RouteType { get; set; } = RouteType.Standard;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class UpdateRouteRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public double? Distance { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public double? MaxVehicleWeight { get; set; }
    public double? MaxVehicleHeight { get; set; }
    public double? MaxVehicleWidth { get; set; }
    public double? MaxVehicleLength { get; set; }
    public RouteType? RouteType { get; set; }
    public RouteStatus? Status { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class VehicleSpecifications
{
    public double GrossWeight { get; set; }
    public double Height { get; set; }
    public double Width { get; set; }
    public double Length { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public List<string> HazmatClasses { get; set; } = new();
    public int AxleCount { get; set; } = 2;
}