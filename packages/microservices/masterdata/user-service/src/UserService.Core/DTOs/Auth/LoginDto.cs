using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Auth;

public class LoginDto
{
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}