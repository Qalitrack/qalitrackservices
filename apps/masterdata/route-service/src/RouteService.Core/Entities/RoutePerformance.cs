using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RoutePerformance : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    public DateTime MeasurementDate { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? AverageSpeed { get; set; } // km/h
    
    [Range(0, double.MaxValue)]
    public double? AverageTravelTime { get; set; } // in minutes
    
    [Range(0, double.MaxValue)]
    public double? FuelConsumption { get; set; } // liters per 100km
    
    [Range(0, double.MaxValue)]
    public decimal? TotalTollCosts { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? TotalDistance { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? NumberOfStops { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? WaitTime { get; set; } // in minutes
    
    [Range(0, double.MaxValue)]
    public double? DelayTime { get; set; } // in minutes
    
    [Range(0, 100)]
    public double? OnTimePerformance { get; set; } // percentage
    
    [Range(1, 10)]
    public int? SafetyRating { get; set; }
    
    [Range(1, 10)]
    public int? RoadConditionRating { get; set; }
    
    [Range(1, 10)]
    public int? TrafficRating { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? NumberOfIncidents { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? NumberOfTrips { get; set; }
    
    [StringLength(200)]
    public string? WeatherConditions { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [StringLength(100)]
    public string? VehicleType { get; set; }
    
    [StringLength(100)]
    public string? DriverId { get; set; }
    
    [StringLength(100)]
    public string? CompanyId { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}