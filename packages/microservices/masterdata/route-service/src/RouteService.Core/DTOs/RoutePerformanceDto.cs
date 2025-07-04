namespace RouteService.Core.DTOs;

public class RoutePerformanceDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public DateTime MeasurementDate { get; set; }
    public double? AverageSpeed { get; set; }
    public double? AverageTravelTime { get; set; }
    public double? FuelConsumption { get; set; }
    public decimal? TotalTollCosts { get; set; }
    public double? TotalDistance { get; set; }
    public int? NumberOfStops { get; set; }
    public double? WaitTime { get; set; }
    public double? DelayTime { get; set; }
    public double? OnTimePerformance { get; set; }
    public int? SafetyRating { get; set; }
    public int? RoadConditionRating { get; set; }
    public int? TrafficRating { get; set; }
    public int? NumberOfIncidents { get; set; }
    public int? NumberOfTrips { get; set; }
    public string? WeatherConditions { get; set; }
    public string? Notes { get; set; }
    public string? VehicleType { get; set; }
    public string? DriverId { get; set; }
    public string? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRoutePerformanceRequest
{
    public DateTime MeasurementDate { get; set; }
    public double? AverageSpeed { get; set; }
    public double? AverageTravelTime { get; set; }
    public double? FuelConsumption { get; set; }
    public decimal? TotalTollCosts { get; set; }
    public double? TotalDistance { get; set; }
    public int? NumberOfStops { get; set; }
    public double? WaitTime { get; set; }
    public double? DelayTime { get; set; }
    public double? OnTimePerformance { get; set; }
    public int? SafetyRating { get; set; }
    public int? RoadConditionRating { get; set; }
    public int? TrafficRating { get; set; }
    public int? NumberOfIncidents { get; set; }
    public int? NumberOfTrips { get; set; }
    public string? WeatherConditions { get; set; }
    public string? Notes { get; set; }
    public string? VehicleType { get; set; }
    public string? DriverId { get; set; }
    public string? CompanyId { get; set; }
}

public class UpdateRoutePerformanceRequest
{
    public DateTime? MeasurementDate { get; set; }
    public double? AverageSpeed { get; set; }
    public double? AverageTravelTime { get; set; }
    public double? FuelConsumption { get; set; }
    public decimal? TotalTollCosts { get; set; }
    public double? TotalDistance { get; set; }
    public int? NumberOfStops { get; set; }
    public double? WaitTime { get; set; }
    public double? DelayTime { get; set; }
    public double? OnTimePerformance { get; set; }
    public int? SafetyRating { get; set; }
    public int? RoadConditionRating { get; set; }
    public int? TrafficRating { get; set; }
    public int? NumberOfIncidents { get; set; }
    public int? NumberOfTrips { get; set; }
    public string? WeatherConditions { get; set; }
    public string? Notes { get; set; }
    public string? VehicleType { get; set; }
    public string? DriverId { get; set; }
    public string? CompanyId { get; set; }
}