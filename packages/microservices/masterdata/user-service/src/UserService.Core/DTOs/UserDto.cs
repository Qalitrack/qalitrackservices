using UserService.Core.Entities;

namespace UserService.Core.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public UserStatus Status { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public UserProfileDto? Profile { get; set; }
    public List<UserRoleDto> Roles { get; set; } = new();
    public List<OrganizationUserDto> Organizations { get; set; } = new();
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public List<string> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
    public string? Language { get; set; }
    public UserStatus? Status { get; set; }
}

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public bool IsProfilePublic { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
}

public class UpdateUserProfileDto
{
    public string? Avatar { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? CompanyName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Bio { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? Twitter { get; set; }
    public bool? IsProfilePublic { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public Dictionary<string, object>? Preferences { get; set; }
}