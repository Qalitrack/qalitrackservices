using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    
    public bool IsActive { get; set; } = false;
    public bool IsFirstLogin { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool TwoFactorEnabled { get; set; } = true; // Mandatory 2FA for all users

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserShift> UserShifts { get; set; } = new List<UserShift>();
    public virtual ICollection<PersonalAccessToken> PersonalAccessTokens { get; set; } = new List<PersonalAccessToken>();
    public virtual ICollection<UserPermissions> UserPermissions { get; set; } = new List<UserPermissions>();

    [NotMapped]
    public bool IsOnline => PersonalAccessTokens.Any(t => !t.IsRevoked);
    [NotMapped]
    public int ActiveTokenCount => PersonalAccessTokens.Count(t => !t.IsRevoked);
}

// Removed UserStatus enum since we're using boolean flags instead