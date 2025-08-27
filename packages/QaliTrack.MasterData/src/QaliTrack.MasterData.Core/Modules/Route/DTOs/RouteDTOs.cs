namespace QaliTrack.MasterData.Core.Modules.Route.DTOs;

public record CreateRouteDto(
    string Name,
    string Code,
    string StartLocation,
    string EndLocation,
    decimal Distance,
    int EstimatedDurationMinutes,
    Guid OrganizationId,
    string? Description = null,
    string Status = "Active",
    string RouteType = "Standard",
    string RoadType = "Mixed",
    string? Coordinates = null,
    string? TrafficConditions = null,
    string? WeatherRestrictions = null,
    string? VehicleRestrictions = null,
    decimal? TollFee = null,
    string? FuelStations = null,
    string? RestAreas = null,
    string? Notes = null
);

public record UpdateRouteDto(
    string Name,
    string StartLocation,
    string EndLocation,
    decimal Distance,
    int EstimatedDurationMinutes,
    string Status,
    string RouteType,
    string RoadType,
    string? Description = null,
    string? Coordinates = null,
    string? TrafficConditions = null,
    string? WeatherRestrictions = null,
    string? VehicleRestrictions = null,
    decimal? TollFee = null,
    string? FuelStations = null,
    string? RestAreas = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchRouteDto(
    string? Name = null,
    string? StartLocation = null,
    string? EndLocation = null,
    decimal? Distance = null,
    int? EstimatedDurationMinutes = null,
    string? Status = null,
    string? RouteType = null,
    string? RoadType = null,
    string? Description = null,
    string? Coordinates = null,
    string? TrafficConditions = null,
    string? WeatherRestrictions = null,
    string? VehicleRestrictions = null,
    decimal? TollFee = null,
    string? FuelStations = null,
    string? RestAreas = null,
    bool? IsActive = null,
    string? Notes = null
);

public record RouteSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string StartLocation { get; init; } = string.Empty;
    public string EndLocation { get; init; } = string.Empty;
    public decimal Distance { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public string Status { get; init; } = string.Empty;
    public string RouteType { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int WaypointCount { get; init; }
    public int ScheduleCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record RouteDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string StartLocation { get; init; } = string.Empty;
    public string EndLocation { get; init; } = string.Empty;
    public decimal Distance { get; init; }
    public int EstimatedDurationMinutes { get; init; }
    public string Status { get; init; } = string.Empty;
    public string RouteType { get; init; } = string.Empty;
    public string RoadType { get; init; } = string.Empty;
    public string? Coordinates { get; init; }
    public string? TrafficConditions { get; init; }
    public string? WeatherRestrictions { get; init; }
    public string? VehicleRestrictions { get; init; }
    public decimal? TollFee { get; init; }
    public string? FuelStations { get; init; }
    public string? RestAreas { get; init; }
    public bool IsActive { get; init; }
    public Guid OrganizationId { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasWaypoints { get; init; }
    public bool HasSchedules { get; init; }
    public bool HasHistory { get; init; }
}

// Route Waypoint DTOs
public record CreateRouteWaypointDto(
    string Name,
    decimal Latitude,
    decimal Longitude,
    int SequenceOrder,
    string WaypointType = "Stop",
    bool IsMandatory = true,
    int? EstimatedDurationMinutes = null,
    decimal? DistanceFromPrevious = null,
    string? Instructions = null,
    string? Restrictions = null,
    string? Services = null,
    string? Notes = null
);

public record UpdateRouteWaypointDto(
    string Name,
    decimal Latitude,
    decimal Longitude,
    int SequenceOrder,
    string WaypointType,
    bool IsMandatory,
    int? EstimatedDurationMinutes = null,
    decimal? DistanceFromPrevious = null,
    string? Instructions = null,
    string? Restrictions = null,
    string? Services = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchRouteWaypointDto(
    string? Name = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    int? SequenceOrder = null,
    string? WaypointType = null,
    bool? IsMandatory = null,
    int? EstimatedDurationMinutes = null,
    decimal? DistanceFromPrevious = null,
    string? Instructions = null,
    string? Restrictions = null,
    string? Services = null,
    bool? IsActive = null,
    string? Notes = null
);

public record RouteWaypointDto
{
    public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public int SequenceOrder { get; init; }
    public string WaypointType { get; init; } = string.Empty;
    public bool IsMandatory { get; init; }
    public int? EstimatedDurationMinutes { get; init; }
    public decimal? DistanceFromPrevious { get; init; }
    public string? Instructions { get; init; }
    public string? Restrictions { get; init; }
    public string? Services { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Route Schedule DTOs
public record CreateRouteScheduleDto(
    string ScheduleName,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    string[] DaysOfWeek,
    DateTime EffectiveDate,
    int Frequency = 1,
    string ScheduleType = "Regular",
    DateTime? ExpiryDate = null,
    decimal? PriceModifier = null,
    string? Notes = null
);

public record UpdateRouteScheduleDto(
    string ScheduleName,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    string[] DaysOfWeek,
    DateTime EffectiveDate,
    int Frequency,
    string ScheduleType,
    DateTime? ExpiryDate = null,
    decimal? PriceModifier = null,
    bool IsActive = true,
    string? Notes = null
);

public record PatchRouteScheduleDto(
    string? ScheduleName = null,
    TimeSpan? DepartureTime = null,
    TimeSpan? ArrivalTime = null,
    string[]? DaysOfWeek = null,
    DateTime? EffectiveDate = null,
    int? Frequency = null,
    string? ScheduleType = null,
    DateTime? ExpiryDate = null,
    decimal? PriceModifier = null,
    bool? IsActive = null,
    string? Notes = null
);

public record RouteScheduleDto
{
    public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public string ScheduleName { get; init; } = string.Empty;
    public TimeSpan DepartureTime { get; init; }
    public TimeSpan ArrivalTime { get; init; }
    public string[] DaysOfWeek { get; init; } = Array.Empty<string>();
    public DateTime EffectiveDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public int Frequency { get; init; }
    public string ScheduleType { get; init; } = string.Empty;
    public decimal? PriceModifier { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsExpired { get; init; }
}

// Route History DTOs
public record CreateRouteHistoryDto(
    DateTime TripDate,
    TimeSpan ActualDepartureTime,
    TimeSpan ActualArrivalTime,
    int ActualDurationMinutes,
    decimal ActualDistance,
    decimal FuelConsumed,
    string TripStatus = "Completed",
    string? DriverId = null,
    string? VehicleId = null,
    string? DelayReason = null,
    int? DelayMinutes = null,
    decimal? FuelCost = null,
    decimal? TollsPaid = null,
    string? Incidents = null,
    string? WeatherConditions = null,
    string? TrafficConditions = null,
    int? PassengerCount = null,
    decimal? Revenue = null,
    string? Notes = null
);

public record UpdateRouteHistoryDto(
    DateTime TripDate,
    TimeSpan ActualDepartureTime,
    TimeSpan ActualArrivalTime,
    int ActualDurationMinutes,
    decimal ActualDistance,
    decimal FuelConsumed,
    string TripStatus,
    string? DriverId = null,
    string? VehicleId = null,
    string? DelayReason = null,
    int? DelayMinutes = null,
    decimal? FuelCost = null,
    decimal? TollsPaid = null,
    string? Incidents = null,
    string? WeatherConditions = null,
    string? TrafficConditions = null,
    int? PassengerCount = null,
    decimal? Revenue = null,
    string? Notes = null
);

public record PatchRouteHistoryDto(
    DateTime? TripDate = null,
    TimeSpan? ActualDepartureTime = null,
    TimeSpan? ActualArrivalTime = null,
    int? ActualDurationMinutes = null,
    decimal? ActualDistance = null,
    decimal? FuelConsumed = null,
    string? TripStatus = null,
    string? DriverId = null,
    string? VehicleId = null,
    string? DelayReason = null,
    int? DelayMinutes = null,
    decimal? FuelCost = null,
    decimal? TollsPaid = null,
    string? Incidents = null,
    string? WeatherConditions = null,
    string? TrafficConditions = null,
    int? PassengerCount = null,
    decimal? Revenue = null,
    string? Notes = null
);

public record RouteHistoryDto
{
    public Guid Id { get; init; }
    public Guid RouteId { get; init; }
    public DateTime TripDate { get; init; }
    public TimeSpan ActualDepartureTime { get; init; }
    public TimeSpan ActualArrivalTime { get; init; }
    public int ActualDurationMinutes { get; init; }
    public string? DriverId { get; init; }
    public string? VehicleId { get; init; }
    public string TripStatus { get; init; } = string.Empty;
    public string? DelayReason { get; init; }
    public int? DelayMinutes { get; init; }
    public decimal ActualDistance { get; init; }
    public decimal FuelConsumed { get; init; }
    public decimal? FuelCost { get; init; }
    public decimal? TollsPaid { get; init; }
    public string? Incidents { get; init; }
    public string? WeatherConditions { get; init; }
    public string? TrafficConditions { get; init; }
    public int? PassengerCount { get; init; }
    public decimal? Revenue { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool WasDelayed { get; init; }
    public decimal? EfficiencyRating { get; init; }
}