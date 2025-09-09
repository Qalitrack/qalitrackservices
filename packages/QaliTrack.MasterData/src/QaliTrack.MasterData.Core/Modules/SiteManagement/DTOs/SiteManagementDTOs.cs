using System.ComponentModel.DataAnnotations;

namespace QaliTrack.MasterData.Core.Modules.SiteManagement.DTOs;

// Zone DTOs
public class ZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateZoneDto
{
    [Required(ErrorMessage = "Zone name is required")]
    [StringLength(100, ErrorMessage = "Zone name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Zone code is required")]
    [StringLength(20, ErrorMessage = "Zone code cannot exceed 20 characters")]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateZoneDto
{
    [StringLength(100, ErrorMessage = "Zone name cannot exceed 100 characters")]
    public string? Name { get; set; }
    
    [StringLength(20, ErrorMessage = "Zone code cannot exceed 20 characters")]
    public string? Code { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
}

// LocationType DTOs
public class LocationTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateLocationTypeDto
{
    [Required(ErrorMessage = "Location type name is required")]
    [StringLength(100, ErrorMessage = "Location type name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Location type code is required")]
    [StringLength(20, ErrorMessage = "Location type code cannot exceed 20 characters")]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class UpdateLocationTypeDto
{
    [StringLength(100, ErrorMessage = "Location type name cannot exceed 100 characters")]
    public string? Name { get; set; }
    
    [StringLength(20, ErrorMessage = "Location type code cannot exceed 20 characters")]
    public string? Code { get; set; }
    
    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; }
}

// Site DTOs
public class SiteDto
{
    public Guid Id { get; set; }
    public Guid LocationTypeId { get; set; }
    public Guid? ZoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public LocationTypeDto? LocationType { get; set; }
    public ZoneDto? Zone { get; set; }
}

public class CreateSiteDto
{
    [Required(ErrorMessage = "Location type is required")]
    public Guid LocationTypeId { get; set; }
    
    public Guid? ZoneId { get; set; }
    
    [Required(ErrorMessage = "Site name is required")]
    [StringLength(200, ErrorMessage = "Site name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }
    
    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string? State { get; set; }
    
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; set; }
    
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string? PostalCode { get; set; }
    
    [StringLength(200, ErrorMessage = "Contact person cannot exceed 200 characters")]
    public string? ContactPerson { get; set; }
    
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters")]
    public string? ContactPhone { get; set; }
    
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string? ContactEmail { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

public class UpdateSiteDto
{
    [Required(ErrorMessage = "Location type is required")]
    public Guid LocationTypeId { get; set; }
    
    public Guid? ZoneId { get; set; }
    
    [Required(ErrorMessage = "Site name is required")]
    [StringLength(200, ErrorMessage = "Site name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string? City { get; set; }
    
    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string? State { get; set; }
    
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string? Country { get; set; }
    
    [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
    public string? PostalCode { get; set; }
    
    [StringLength(200, ErrorMessage = "Contact person cannot exceed 200 characters")]
    public string? ContactPerson { get; set; }
    
    [StringLength(20, ErrorMessage = "Contact phone cannot exceed 20 characters")]
    public string? ContactPhone { get; set; }
    
    [StringLength(100, ErrorMessage = "Contact email cannot exceed 100 characters")]
    [EmailAddress(ErrorMessage = "Please provide a valid email address")]
    public string? ContactEmail { get; set; }
    
    public bool IsActive { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}