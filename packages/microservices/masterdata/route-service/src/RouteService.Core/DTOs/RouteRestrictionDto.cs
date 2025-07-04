using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteRestrictionDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RestrictionType Type { get; set; }
    public RestrictionSeverity Severity { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public double? MaxWeight { get; set; }
    public double? MaxHeight { get; set; }
    public double? MaxWidth { get; set; }
    public double? MaxLength { get; set; }
    public string? VehicleTypes { get; set; }
    public string? HazmatClasses { get; set; }
    public bool IsActive { get; set; }
    public string? EnforcementAgency { get; set; }
    public string? PermitRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteRestrictionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RestrictionType Type { get; set; }
    public RestrictionSeverity Severity { get; set; } = RestrictionSeverity.Medium;
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public double? MaxWeight { get; set; }
    public double? MaxHeight { get; set; }
    public double? MaxWidth { get; set; }
    public double? MaxLength { get; set; }
    public string? VehicleTypes { get; set; }
    public string? HazmatClasses { get; set; }
    public string? EnforcementAgency { get; set; }
    public string? PermitRequired { get; set; }
}

public class UpdateRouteRestrictionRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public RestrictionType? Type { get; set; }
    public RestrictionSeverity? Severity { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? DaysOfWeek { get; set; }
    public double? MaxWeight { get; set; }
    public double? MaxHeight { get; set; }
    public double? MaxWidth { get; set; }
    public double? MaxLength { get; set; }
    public string? VehicleTypes { get; set; }
    public string? HazmatClasses { get; set; }
    public bool? IsActive { get; set; }
    public string? EnforcementAgency { get; set; }
    public string? PermitRequired { get; set; }
}