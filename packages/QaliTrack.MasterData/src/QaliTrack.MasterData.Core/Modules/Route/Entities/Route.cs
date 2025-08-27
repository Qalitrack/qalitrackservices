using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Route.Entities;

public class Route : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public decimal Distance { get; set; } // in kilometers
    public int EstimatedDurationMinutes { get; set; }
    public string Status { get; set; } = "Active";
    public string RouteType { get; set; } = "Standard"; // Standard, Express, Economy, etc.
    public string RoadType { get; set; } = "Mixed"; // Highway, Urban, Rural, Mixed
    public string? Coordinates { get; set; } // JSON array of GPS coordinates
    public string? TrafficConditions { get; set; }
    public string? WeatherRestrictions { get; set; }
    public string? VehicleRestrictions { get; set; } // JSON array of vehicle type restrictions
    public decimal? TollFee { get; set; }
    public string? FuelStations { get; set; } // JSON array of fuel station locations
    public string? RestAreas { get; set; } // JSON array of rest area locations
    public bool IsActive { get; set; } = true;
    public Guid OrganizationId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<RouteWaypoint> Waypoints { get; set; } = new List<RouteWaypoint>();
    public virtual ICollection<RouteSchedule> Schedules { get; set; } = new List<RouteSchedule>();
    public virtual ICollection<RouteHistory> History { get; set; } = new List<RouteHistory>();
    
    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<RouteWeighbridgeAssociation> WeighbridgeAssociations { get; set; } = new List<RouteWeighbridgeAssociation>();
}

public class RouteWaypoint : BaseEntity
{
    public Guid RouteId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public int SequenceOrder { get; set; }
    public string WaypointType { get; set; } = "Stop"; // Stop, Checkpoint, Warning, etc.
    public bool IsMandatory { get; set; } = true;
    public int? EstimatedDurationMinutes { get; set; }
    public decimal? DistanceFromPrevious { get; set; }
    public string? Instructions { get; set; }
    public string? Restrictions { get; set; }
    public string? Services { get; set; } // JSON array of available services
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Route Route { get; set; } = null!;
}

public class RouteSchedule : BaseEntity
{
    public Guid RouteId { get; set; }
    public string ScheduleName { get; set; } = string.Empty;
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public string[] DaysOfWeek { get; set; } = Array.Empty<string>(); // JSON array: Mon, Tue, Wed, etc.
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int Frequency { get; set; } = 1; // How many times per day
    public string ScheduleType { get; set; } = "Regular"; // Regular, Special, Emergency
    public decimal? PriceModifier { get; set; } // Multiplier for base price
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Route Route { get; set; } = null!;
}

public class RouteHistory : BaseEntity
{
    public Guid RouteId { get; set; }
    public DateTime TripDate { get; set; }
    public TimeSpan ActualDepartureTime { get; set; }
    public TimeSpan ActualArrivalTime { get; set; }
    public int ActualDurationMinutes { get; set; }
    public string? DriverId { get; set; } // Will be Guid when Driver module is referenced
    public string? VehicleId { get; set; } // Will be Guid when Vehicle module is referenced
    public string TripStatus { get; set; } = "Completed"; // Completed, Delayed, Cancelled, InProgress
    public string? DelayReason { get; set; }
    public int? DelayMinutes { get; set; }
    public decimal ActualDistance { get; set; }
    public decimal FuelConsumed { get; set; }
    public decimal? FuelCost { get; set; }
    public decimal? TollsPaid { get; set; }
    public string? Incidents { get; set; } // JSON array of incidents during trip
    public string? WeatherConditions { get; set; }
    public string? TrafficConditions { get; set; }
    public int? PassengerCount { get; set; }
    public decimal? Revenue { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Route Route { get; set; } = null!;
}

// Enumerations
public enum RouteStatus
{
    Active,
    Inactive,
    Suspended,
    UnderMaintenance,
    Seasonal
}

public enum RouteType
{
    Standard,
    Express,
    Economy,
    Premium,
    Charter,
    Emergency
}

public enum RoadType
{
    Highway,
    Urban,
    Rural,
    Mountain,
    Coastal,
    Mixed
}

public enum WaypointType
{
    Stop,
    Checkpoint,
    FuelStation,
    RestArea,
    Warning,
    Weighbridge,
    Border,
    Service
}

public enum ScheduleType
{
    Regular,
    Peak,
    OffPeak,
    Weekend,
    Holiday,
    Special,
    Emergency
}

public enum TripStatus
{
    Scheduled,
    InProgress,
    Completed,
    Delayed,
    Cancelled,
    Diverted
}