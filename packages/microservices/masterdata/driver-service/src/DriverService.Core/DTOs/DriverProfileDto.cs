namespace DriverService.Core.DTOs;

public class DriverProfileDto
{
    public string Id { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string PreviousEmployers { get; set; } = string.Empty;
    public string SpecialSkills { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
    public bool HasCleanRecord { get; set; }
    public string AccidentHistory { get; set; } = string.Empty;
    public decimal SafetyRating { get; set; }
    public decimal PerformanceRating { get; set; }
    public int TotalMilesDriven { get; set; }
    public int TotalTripsCompleted { get; set; }
    public DateTime? LastTrip { get; set; }
    public string PreferredRoutes { get; set; } = string.Empty;
    public string PreferredVehicleTypes { get; set; } = string.Empty;
    public bool IsAvailableForOvertire { get; set; }
    public bool IsAvailableForWeekends { get; set; }
    public bool IsAvailableForNightShifts { get; set; }
    public string WorkSchedulePreference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateDriverProfileDto
{
    public int YearsOfExperience { get; set; } = 0;
    public string PreviousEmployers { get; set; } = string.Empty;
    public string SpecialSkills { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
    public bool HasCleanRecord { get; set; } = true;
    public string AccidentHistory { get; set; } = string.Empty;
    public decimal SafetyRating { get; set; } = 5.0m;
    public decimal PerformanceRating { get; set; } = 5.0m;
    public int TotalMilesDriven { get; set; } = 0;
    public int TotalTripsCompleted { get; set; } = 0;
    public DateTime? LastTrip { get; set; }
    public string PreferredRoutes { get; set; } = string.Empty;
    public string PreferredVehicleTypes { get; set; } = string.Empty;
    public bool IsAvailableForOvertire { get; set; } = true;
    public bool IsAvailableForWeekends { get; set; } = true;
    public bool IsAvailableForNightShifts { get; set; } = true;
    public string WorkSchedulePreference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
}

public class UpdateDriverProfileDto
{
    public int YearsOfExperience { get; set; }
    public string PreviousEmployers { get; set; } = string.Empty;
    public string SpecialSkills { get; set; } = string.Empty;
    public string Languages { get; set; } = string.Empty;
    public bool HasCleanRecord { get; set; }
    public string AccidentHistory { get; set; } = string.Empty;
    public decimal SafetyRating { get; set; }
    public decimal PerformanceRating { get; set; }
    public int TotalMilesDriven { get; set; }
    public int TotalTripsCompleted { get; set; }
    public DateTime? LastTrip { get; set; }
    public string PreferredRoutes { get; set; } = string.Empty;
    public string PreferredVehicleTypes { get; set; } = string.Empty;
    public bool IsAvailableForOvertire { get; set; }
    public bool IsAvailableForWeekends { get; set; }
    public bool IsAvailableForNightShifts { get; set; }
    public string WorkSchedulePreference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}