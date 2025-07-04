namespace OrganizationService.Core.Entities;

public class OrganizationUser : BaseEntity
{
    public string OrganizationId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty; // Reference to User Service
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public OrganizationRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? InvitationToken { get; set; }
    public List<string> Permissions { get; set; } = new List<string>();
    public string? DepartmentId { get; set; }
    public string? JobTitle { get; set; }
    public string? PhoneNumber { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public OrganizationDepartment? Department { get; set; }
}

public enum OrganizationRole
{
    Owner = 0,
    Admin = 1,
    Manager = 2,
    User = 3,
    Viewer = 4
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Invited = 2,
    Suspended = 3,
    Left = 4
}