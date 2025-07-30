using System.ComponentModel.DataAnnotations;
using UserService.Core.Entities;

namespace UserService.Core.DTOs.User;

public class UpdateUserDto
{   
    [StringLength(50, ErrorMessage = "First name must not exceed 50 characters")]
    public string? FirstName { get; set; }

    [StringLength(50, ErrorMessage = "Last name must not exceed 50 characters")]
    public string? LastName { get; set; }

    [StringLength(20, ErrorMessage = "Mobile number must not exceed 20 characters")]
    public string? MobileNumber { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string? Email { get; set; }
    
    public bool? IsFirstLogin { get; set; }
}