using OrganizationService.Core.Entities;

namespace OrganizationService.Core.DTOs;

public class OrganizationUserDto
{
    public string Id { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public OrganizationRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
}

public class CreateOrganizationUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public OrganizationRole Role { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public string? DepartmentId { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
}

public class UpdateOrganizationUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public OrganizationRole Role { get; set; }
    public UserStatus Status { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public string? DepartmentId { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }
}