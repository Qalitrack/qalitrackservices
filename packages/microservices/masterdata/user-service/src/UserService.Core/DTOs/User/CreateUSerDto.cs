using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.User;

public class CreateUserDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    // Optional — an admin can set this directly instead of relying on the
    // default temporary password. Falls back to UserService.DefaultTemporaryPassword
    // when left blank. Still validated against the org password policy when set.
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string? Password { get; set; }

    // Defaults to true (mandatory 2FA) when not specified.
    public bool? TwoFactorEnabled { get; set; }
}
