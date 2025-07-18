namespace RouteService.Core.DTOs;

public class RouteMapDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double Distance { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public List<RouteWaypointDto> Waypoints { get; set; } = new List<RouteWaypointDto>();
    public List<MapCoordinateDto> Coordinates { get; set; } = new List<MapCoordinateDto>();
    public MapBoundsDto Bounds { get; set; } = new MapBoundsDto();
    public string? MapProvider { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class MapCoordinateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Sequence { get; set; }
}

public class MapBoundsDto
{
    public double NortheastLat { get; set; }
    public double NortheastLng { get; set; }
    public double SouthwestLat { get; set; }
    public double SouthwestLng { get; set; }
}

public class RouteTrafficDto
{
    public string RouteId { get; set; } = string.Empty;
    public string TrafficStatus { get; set; } = string.Empty; // "low", "moderate", "heavy", "severe"
    public TimeSpan CurrentDuration { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public double DelayMinutes { get; set; }
    public List<TrafficIncidentDto> Incidents { get; set; } = new List<TrafficIncidentDto>();
    public DateTime LastUpdated { get; set; }
}

public class TrafficIncidentDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "accident", "construction", "roadblock", "weather"
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // "minor", "moderate", "major", "critical"
    public MapCoordinateDto Location { get; set; } = new MapCoordinateDto();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? EstimatedDelay { get; set; }
}

public class RouteOptimizationRequestDto
{
    public List<string> VehicleTypes { get; set; } = new List<string>();
    public DateTime DepartureTime { get; set; }
    public bool AvoidTolls { get; set; } = false;
    public bool AvoidHighways { get; set; } = false;
    public bool ConsiderTraffic { get; set; } = true;
    public string? PreferredRouteType { get; set; }
    public List<string> RestrictedAreas { get; set; } = new List<string>();
}

public class RouteOptimizationDto
{
    public string RouteId { get; set; } = string.Empty;
    public RouteMapDto OptimizedRoute { get; set; } = new RouteMapDto();
    public RouteMapDto? AlternativeRoute { get; set; }
    public TimeSpan TimeSaved { get; set; }
    public double DistanceSaved { get; set; }
    public decimal CostSaved { get; set; }
    public string OptimizationReason { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new List<string>();
    public DateTime OptimizedAt { get; set; }
}