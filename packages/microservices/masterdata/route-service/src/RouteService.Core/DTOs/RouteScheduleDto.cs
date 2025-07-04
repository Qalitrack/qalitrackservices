using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ScheduleType Type { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? DepartureTime { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? DaysOfMonth { get; set; }
    public string? MonthsOfYear { get; set; }
    public double? EstimatedDuration { get; set; }
    public int? MaxVehicles { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public string? Priority { get; set; }
    public bool RequiresReservation { get; set; }
    public int? MaxReservations { get; set; }
    public string? ReservationContact { get; set; }
    public bool IsActive { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? WeatherDependency { get; set; }
    public string? SeasonalAdjustments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteScheduleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ScheduleType Type { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? DepartureTime { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? DaysOfMonth { get; set; }
    public string? MonthsOfYear { get; set; }
    public double? EstimatedDuration { get; set; }
    public int? MaxVehicles { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public string? Priority { get; set; }
    public bool RequiresReservation { get; set; } = false;
    public int? MaxReservations { get; set; }
    public string? ReservationContact { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? WeatherDependency { get; set; }
    public string? SeasonalAdjustments { get; set; }
}

public class UpdateRouteScheduleRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public ScheduleType? Type { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? DepartureTime { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public string? DaysOfMonth { get; set; }
    public string? MonthsOfYear { get; set; }
    public double? EstimatedDuration { get; set; }
    public int? MaxVehicles { get; set; }
    public bool? IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public int? RecurrenceInterval { get; set; }
    public string? Priority { get; set; }
    public bool? RequiresReservation { get; set; }
    public int? MaxReservations { get; set; }
    public string? ReservationContact { get; set; }
    public bool? IsActive { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? WeatherDependency { get; set; }
    public string? SeasonalAdjustments { get; set; }
}