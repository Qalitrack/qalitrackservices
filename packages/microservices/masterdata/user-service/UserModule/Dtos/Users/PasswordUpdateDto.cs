using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Users;

public class PasswordUpdateDto
{
    [Required]
    public string CurrentPassword { get; set; }
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string NewPassword { get; set; }
    [Required]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmNewPassword { get; set; }
    
}