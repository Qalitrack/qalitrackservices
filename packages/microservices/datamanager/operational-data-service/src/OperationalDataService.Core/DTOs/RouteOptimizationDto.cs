using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class RouteOptimizationRequest
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string? VehicleType { get; set; }
    public decimal? MaxWeight { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public List<string> AvoidRestrictions { get; set; } = new();
    public OptimizationCriteria Criteria { get; set; } = new();
    public List<string> Waypoints { get; set; } = new();
    public string? OrganizationId { get; set; }
}

public class OptimizationCriteria
{
    public bool OptimizeForTime { get; set; } = true;
    public bool OptimizeForDistance { get; set; } = false;
    public bool OptimizeForCost { get; set; } = false;
    public bool OptimizeForFuel { get; set; } = false;
    public bool AvoidTolls { get; set; } = false;
    public bool AvoidHighways { get; set; } = false;
    public bool AvoidTraffic { get; set; } = true;
    public List<string> PreferredRoutes { get; set; } = new();
    public List<string> AvoidedRoutes { get; set; } = new();
    public Dictionary<string, decimal> WeightFactors { get; set; } = new();
}

public class OptimizedRoute
{
    public string RouteId { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal EstimatedFuelCost { get; set; }
    public decimal TollCost { get; set; }
    public RouteTrafficCondition TrafficConditions { get; set; }
    public List<RouteWaypoint> Waypoints { get; set; } = new();
    public List<RouteRestriction> Restrictions { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public RouteQuality Quality { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ValidUntil { get; set; }
}

public class RouteRecommendation
{
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public RecommendationReason Reason { get; set; }
    public List<string> Advantages { get; set; } = new();
    public List<string> Disadvantages { get; set; } = new();
    public OptimizedRoute Route { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class RoutePerformanceMetricsDto
{
    public string RouteId { get; set; } = string.Empty;
    public TimeRange Period { get; set; } = new();
    public decimal AverageSpeed { get; set; }
    public decimal AverageDelay { get; set; }
    public decimal ReliabilityScore { get; set; }
    public int TotalTrips { get; set; }
    public int SuccessfulTrips { get; set; }
    public int DelayedTrips { get; set; }
    public decimal CustomerSatisfactionScore { get; set; }
    public List<RouteIssue> Issues { get; set; } = new();
    public List<PerformanceTrend> Trends { get; set; } = new();
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
}

public class RouteQuality
{
    public decimal OverallScore { get; set; }
    public decimal RoadQuality { get; set; }
    public decimal SafetyScore { get; set; }
    public decimal TrafficScore { get; set; }
    public decimal WeatherScore { get; set; }
    public decimal InfrastructureScore { get; set; }
    public List<string> QualityFactors { get; set; } = new();
}


public class TrafficData
{
    public string RouteId { get; set; } = string.Empty;
    public RouteTrafficCondition Condition { get; set; }
    public decimal AverageSpeed { get; set; }
    public decimal Congestion { get; set; }
    public List<TrafficIncident> Incidents { get; set; } = new();
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
}

public class TrafficIncident
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public IncidentSeverity Severity { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsActive { get; set; } = true;
}


public enum RecommendationReason
{
    Fastest,
    Shortest,
    Cheapest,
    MostReliable,
    BestTraffic,
    BestRoadConditions,
    MostSafe,
    Custom
}



public enum IncidentSeverity
{
    Minor,
    Moderate,
    Major,
    Severe
}