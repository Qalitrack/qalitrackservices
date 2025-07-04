namespace DriverService.Core.Entities;

public class DriverProfile : BaseEntity
{
    public int YearsOfExperience { get; set; }
    public string PreviousEmployers { get; set; } = string.Empty; // JSON array
    public string SpecialSkills { get; set; } = string.Empty; // JSON array
    public string Languages { get; set; } = string.Empty; // JSON array
    public bool HasCleanRecord { get; set; } = true;
    public string AccidentHistory { get; set; } = string.Empty; // JSON array
    public decimal SafetyRating { get; set; } = 5.0m; // Out of 5
    public decimal PerformanceRating { get; set; } = 5.0m; // Out of 5
    public int TotalMilesDriven { get; set; } = 0;
    public int TotalTripsCompleted { get; set; } = 0;
    public DateTime? LastTrip { get; set; }
    public string PreferredRoutes { get; set; } = string.Empty; // JSON array
    public string PreferredVehicleTypes { get; set; } = string.Empty; // JSON array
    public bool IsAvailableForOvertire { get; set; } = true;
    public bool IsAvailableForWeekends { get; set; } = true;
    public bool IsAvailableForNightShifts { get; set; } = true;
    public string WorkSchedulePreference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Foreign keys
    public string DriverId { get; set; } = string.Empty;

    // Navigation properties
    public virtual Driver Driver { get; set; } = null!;
}