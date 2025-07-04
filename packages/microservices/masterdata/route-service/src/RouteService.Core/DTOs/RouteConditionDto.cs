using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteConditionDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public ConditionType Type { get; set; }
    public ConditionStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime ReportedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ReportedBy { get; set; }
    public int? SeverityLevel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }
    public double? EstimatedDelay { get; set; }
    public string? RecommendedAction { get; set; }
    public bool IsActive { get; set; }
    public string? WeatherCondition { get; set; }
    public double? Temperature { get; set; }
    public int? Visibility { get; set; }
    public string? TrafficInfo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteConditionRequest
{
    public ConditionType Type { get; set; }
    public ConditionStatus Status { get; set; } = ConditionStatus.Normal;
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public int? SeverityLevel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }
    public double? EstimatedDelay { get; set; }
    public string? RecommendedAction { get; set; }
    public string? WeatherCondition { get; set; }
    public double? Temperature { get; set; }
    public int? Visibility { get; set; }
    public string? TrafficInfo { get; set; }
}

public class UpdateRouteConditionRequest
{
    public ConditionType? Type { get; set; }
    public ConditionStatus? Status { get; set; }
    public string? Description { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? SeverityLevel { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Location { get; set; }
    public double? EstimatedDelay { get; set; }
    public string? RecommendedAction { get; set; }
    public bool? IsActive { get; set; }
    public string? WeatherCondition { get; set; }
    public double? Temperature { get; set; }
    public int? Visibility { get; set; }
    public string? TrafficInfo { get; set; }
}