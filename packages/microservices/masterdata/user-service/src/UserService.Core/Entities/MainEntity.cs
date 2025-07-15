using System.ComponentModel.DataAnnotations;
using UserService.Core.Interfaces;

namespace UserService.Core.Entities;

public class User : BaseEntity
{   [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string MobileNumber { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
    
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();  // New addition
    public virtual ICollection<PersonalAccessToken> PersonalAccessTokens { get; set; } = new List<PersonalAccessToken>();
    public virtual ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended,
    Pending
}