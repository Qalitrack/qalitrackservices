using RouteService.Core.Entities;

namespace RouteService.Core.DTOs;

public class RouteHazmatDto
{
    public string Id { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string HazmatClass { get; set; } = string.Empty;
    public string? HazmatDescription { get; set; }
    public HazmatRestrictionType RestrictionType { get; set; }
    public bool IsProhibited { get; set; }
    public bool RequiresPermit { get; set; }
    public string? PermitType { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? PermitValidFrom { get; set; }
    public DateTime? PermitValidTo { get; set; }
    public double? MaxQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? AlternativeRoute { get; set; }
    public TimeOnly? RestrictedTimeStart { get; set; }
    public TimeOnly? RestrictedTimeEnd { get; set; }
    public string? RestrictedDays { get; set; }
    public string? EmergencyContactInfo { get; set; }
    public string? SafetyRequirements { get; set; }
    public bool RequiresEscort { get; set; }
    public string? EscortRequirements { get; set; }
    public double? RestrictionStartLatitude { get; set; }
    public double? RestrictionStartLongitude { get; set; }
    public double? RestrictionEndLatitude { get; set; }
    public double? RestrictionEndLongitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRouteHazmatRequest
{
    public string HazmatClass { get; set; } = string.Empty;
    public string? HazmatDescription { get; set; }
    public HazmatRestrictionType RestrictionType { get; set; }
    public bool IsProhibited { get; set; } = false;
    public bool RequiresPermit { get; set; } = false;
    public string? PermitType { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? PermitValidFrom { get; set; }
    public DateTime? PermitValidTo { get; set; }
    public double? MaxQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? AlternativeRoute { get; set; }
    public TimeOnly? RestrictedTimeStart { get; set; }
    public TimeOnly? RestrictedTimeEnd { get; set; }
    public string? RestrictedDays { get; set; }
    public string? EmergencyContactInfo { get; set; }
    public string? SafetyRequirements { get; set; }
    public bool RequiresEscort { get; set; } = false;
    public string? EscortRequirements { get; set; }
    public double? RestrictionStartLatitude { get; set; }
    public double? RestrictionStartLongitude { get; set; }
    public double? RestrictionEndLatitude { get; set; }
    public double? RestrictionEndLongitude { get; set; }
}

public class UpdateRouteHazmatRequest
{
    public string? HazmatClass { get; set; }
    public string? HazmatDescription { get; set; }
    public HazmatRestrictionType? RestrictionType { get; set; }
    public bool? IsProhibited { get; set; }
    public bool? RequiresPermit { get; set; }
    public string? PermitType { get; set; }
    public string? IssuingAuthority { get; set; }
    public DateTime? PermitValidFrom { get; set; }
    public DateTime? PermitValidTo { get; set; }
    public double? MaxQuantity { get; set; }
    public string? QuantityUnit { get; set; }
    public string? SpecialRequirements { get; set; }
    public string? AlternativeRoute { get; set; }
    public TimeOnly? RestrictedTimeStart { get; set; }
    public TimeOnly? RestrictedTimeEnd { get; set; }
    public string? RestrictedDays { get; set; }
    public string? EmergencyContactInfo { get; set; }
    public string? SafetyRequirements { get; set; }
    public bool? RequiresEscort { get; set; }
    public string? EscortRequirements { get; set; }
    public double? RestrictionStartLatitude { get; set; }
    public double? RestrictionStartLongitude { get; set; }
    public double? RestrictionEndLatitude { get; set; }
    public double? RestrictionEndLongitude { get; set; }
}