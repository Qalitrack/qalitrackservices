using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteHazmat : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10)]
    public string HazmatClass { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string? HazmatDescription { get; set; }
    
    [Required]
    public HazmatRestrictionType RestrictionType { get; set; }
    
    public bool IsProhibited { get; set; } = false;
    
    public bool RequiresPermit { get; set; } = false;
    
    [StringLength(100)]
    public string? PermitType { get; set; }
    
    [StringLength(100)]
    public string? IssuingAuthority { get; set; }
    
    public DateTime? PermitValidFrom { get; set; }
    public DateTime? PermitValidTo { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxQuantity { get; set; }
    
    [StringLength(20)]
    public string? QuantityUnit { get; set; }
    
    [StringLength(500)]
    public string? SpecialRequirements { get; set; }
    
    [StringLength(200)]
    public string? AlternativeRoute { get; set; }
    
    public TimeOnly? RestrictedTimeStart { get; set; }
    public TimeOnly? RestrictedTimeEnd { get; set; }
    
    [StringLength(200)]
    public string? RestrictedDays { get; set; }
    
    [StringLength(200)]
    public string? EmergencyContactInfo { get; set; }
    
    [StringLength(500)]
    public string? SafetyRequirements { get; set; }
    
    public bool RequiresEscort { get; set; } = false;
    
    [StringLength(200)]
    public string? EscortRequirements { get; set; }
    
    [Range(-90, 90)]
    public double? RestrictionStartLatitude { get; set; }
    
    [Range(-180, 180)]
    public double? RestrictionStartLongitude { get; set; }
    
    [Range(-90, 90)]
    public double? RestrictionEndLatitude { get; set; }
    
    [Range(-180, 180)]
    public double? RestrictionEndLongitude { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum HazmatRestrictionType
{
    ClassRestriction,
    QuantityRestriction,
    TimeRestriction,
    RouteRestriction,
    PermitRequired,
    EscortRequired,
    PackagingRestriction,
    VehicleRestriction,
    WeatherRestriction,
    TunnelRestriction,
    BridgeRestriction
}