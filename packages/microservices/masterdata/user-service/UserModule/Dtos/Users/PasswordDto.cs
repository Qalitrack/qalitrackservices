using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Users;

public class PasswordDto
{   [Required]
    public static string CurrentPassword { get; set; }
    [Required]
    public static string NewPassword { get; set; }
    [Required]
    public string ConfirmNewPassword { get; set; }
}