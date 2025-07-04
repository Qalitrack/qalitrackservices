using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteSchedule : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
    
    [Required]
    public ScheduleType Type { get; set; }
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public TimeOnly? DepartureTime { get; set; }
    public TimeOnly? ArrivalTime { get; set; }
    
    [StringLength(100)]
    public string? DaysOfWeek { get; set; } // Comma-separated values
    
    [StringLength(100)]
    public string? DaysOfMonth { get; set; } // Comma-separated values
    
    [StringLength(100)]
    public string? MonthsOfYear { get; set; } // Comma-separated values
    
    [Range(0, double.MaxValue)]
    public double? EstimatedDuration { get; set; } // in minutes
    
    [Range(0, int.MaxValue)]
    public int? MaxVehicles { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    [StringLength(50)]
    public string? RecurrencePattern { get; set; } // Daily, Weekly, Monthly, etc.
    
    [Range(1, int.MaxValue)]
    public int? RecurrenceInterval { get; set; }
    
    [StringLength(100)]
    public string? Priority { get; set; }
    
    public bool RequiresReservation { get; set; } = false;
    
    [Range(0, int.MaxValue)]
    public int? MaxReservations { get; set; }
    
    [StringLength(200)]
    public string? ReservationContact { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(500)]
    public string? SpecialInstructions { get; set; }
    
    [StringLength(100)]
    public string? WeatherDependency { get; set; }
    
    [StringLength(100)]
    public string? SeasonalAdjustments { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum ScheduleType
{
    Regular,
    Express,
    Peak,
    OffPeak,
    Weekend,
    Holiday,
    Seasonal,
    Emergency,
    Maintenance,
    Special
}