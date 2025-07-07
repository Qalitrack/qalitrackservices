using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Users;

public class UserCreateDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    [Required]
    public bool IsActive { get; set; } = true;
    
    public bool LoginStatus { get; set; } = false; 
    [Required(ErrorMessage = "Role name is required")]
    public string Role { get; set; } // Role name to assign
}