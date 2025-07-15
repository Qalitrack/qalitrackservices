using System.ComponentModel.DataAnnotations;
using UserService.Core.Entities;

namespace UserService.Core.DTOs.User;

public class UpdateUserDto
{   
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required")]
    [StringLength(20, ErrorMessage = "Mobile number must not exceed 20 characters")]
    public string MobileNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    // Password update is optional when updating user
    // Only required if Password is provided
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    // Boolean status flags
    [Required(ErrorMessage = "Is Active is required")]
    public bool IsActive { get; set; }
    [Required(ErrorMessage = "Is First Login is required")]
    public bool IsFirstLogin { get; set; }
    [Required(ErrorMessage = "Is Deleted is required")]
    public bool IsDeleted { get; set; }

    // Timestamps
    public DateTime? UpdatedAt { get; set; }
}