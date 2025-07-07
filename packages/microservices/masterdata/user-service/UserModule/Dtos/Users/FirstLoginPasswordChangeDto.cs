using System.ComponentModel.DataAnnotations;

namespace UserModule.Dtos.Users;

public class FirstLoginPasswordChangeDto
{
    [StringLength(100, MinimumLength = 6)]
    [Compare("ConfirmNewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    [Required]
    public string NewPassword { get; set; }
    [Required]
    public string ConfirmNewPassword { get; set; }
}