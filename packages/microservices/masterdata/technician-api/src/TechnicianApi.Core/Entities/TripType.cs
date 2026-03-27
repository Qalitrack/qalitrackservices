using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class TripType : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public TripCategory Category { get; set; } = TripCategory.Other;

    public EmptyTripOption EmptyTripOption { get; set; } = EmptyTripOption.NotAllowed;

    public MaterialRequirement MaterialRequirement { get; set; } = MaterialRequirement.None;

    public string? CreatedByUserId { get; set; } // References user service

    // Navigation properties
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}

public enum TripCategory
{
    Loaded,
    Empty,
    Maintenance,
    Other
}

public enum EmptyTripOption
{
    NotAllowed,
    Allowed,
    Required
}

public enum MaterialRequirement
{
    None,
    Optional,
    Mandatory
}
