namespace QaliTrack.MasterData.Core.Modules.Organization.DTOs;

/// <summary>
/// Simplified DTO for creating an organization - only essential fields
/// </summary>
public class CreateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    
    // Optional fields
    public string? PostalCode { get; set; }
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating basic organization information
/// </summary>
public class UpdateOrganizationDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Simplified DTO for organization list view
/// </summary>
public class OrganizationSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public DateTime EstablishedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed DTO for single organization view - without child collections
/// </summary>
public class OrganizationDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string? LogoUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Status flags
    public bool HasSettings { get; set; }
    public bool HasSubscription { get; set; }
    public bool HasActiveUsers { get; set; }
    public bool HasLocations { get; set; }
}

// Organization User DTOs
public class OrganizationUserDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
    public DateTime? LeftDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Organization Location DTOs
public class OrganizationLocationDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsHeadquarters { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}