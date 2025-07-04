using OrganizationService.Core.Entities;

namespace OrganizationService.Core.DTOs;

public class OrganizationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Industry { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public string? SubscriptionPlan { get; set; }
    public OrganizationStatus Status { get; set; }
    public string? ParentOrganizationId { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public int MaxUsers { get; set; }
    public DateTime? SubscriptionExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrganizationDto> ChildOrganizations { get; set; } = new List<OrganizationDto>();
}

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Industry { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public string? SubscriptionPlan { get; set; }
    public string? ParentOrganizationId { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public int MaxUsers { get; set; } = 10;
}

public class UpdateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? RegistrationNumber { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Industry { get; set; }
    public string? SubscriptionPlan { get; set; }
    public string? Logo { get; set; }
    public string? Website { get; set; }
    public string? Description { get; set; }
    public int MaxUsers { get; set; }
    public OrganizationStatus Status { get; set; }
}