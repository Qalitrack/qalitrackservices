using UserService.Core.Entities;

namespace UserService.Core.DTOs;

public class OrganizationUserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public string? EmployeeId { get; set; }
    public string? Position { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public OrganizationUserStatus Status { get; set; }
    public bool IsOwner { get; set; }
    public bool IsAdmin { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime? InvitedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOrganizationUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
    public string? EmployeeId { get; set; }
    public string? Position { get; set; }
    public bool IsOwner { get; set; } = false;
    public bool IsAdmin { get; set; } = false;
    public List<string> RoleIds { get; set; } = new();
}

public class UpdateOrganizationUserDto
{
    public string? DepartmentId { get; set; }
    public string? EmployeeId { get; set; }
    public string? Position { get; set; }
    public OrganizationUserStatus? Status { get; set; }
    public bool? IsAdmin { get; set; }
    public List<string>? RoleIds { get; set; }
}

public class UserInvitationDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string InvitedBy { get; set; } = string.Empty;
    public string? RoleId { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserInvitationDto
{
    public string Email { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string? RoleId { get; set; }
    public string? Message { get; set; }
    public List<string> RoleIds { get; set; } = new();
}

public class AcceptInvitationDto
{
    public string Token { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}