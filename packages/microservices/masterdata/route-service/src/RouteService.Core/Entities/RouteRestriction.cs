using System.ComponentModel.DataAnnotations;

namespace RouteService.Core.Entities;

public class RouteRestriction : BaseEntity
{
    [Required]
    public string RouteId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public RestrictionType Type { get; set; }
    
    [Required]
    public RestrictionSeverity Severity { get; set; } = RestrictionSeverity.Medium;
    
    public DateTime? EffectiveDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    
    [StringLength(50)]
    public string? DaysOfWeek { get; set; } // Comma-separated values
    
    [Range(0, double.MaxValue)]
    public double? MaxWeight { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxHeight { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxWidth { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? MaxLength { get; set; }
    
    [StringLength(100)]
    public string? VehicleTypes { get; set; } // Comma-separated values
    
    [StringLength(100)]
    public string? HazmatClasses { get; set; } // Comma-separated values
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(100)]
    public string? EnforcementAgency { get; set; }
    
    [StringLength(50)]
    public string? PermitRequired { get; set; }
    
    // Navigation property
    public virtual Route Route { get; set; } = null!;
}

public enum RestrictionType
{
    Weight,
    Height,
    Width,
    Length,
    VehicleType,
    Hazmat,
    Time,
    Seasonal,
    Weather,
    Construction,
    Environmental,
    Special,
    Permit
}

public enum RestrictionSeverity
{
    Low,
    Medium,
    High,
    Critical
}