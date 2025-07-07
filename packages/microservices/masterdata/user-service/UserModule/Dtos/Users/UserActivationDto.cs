using System.ComponentModel.DataAnnotations;

public class UserActivationDto
{
    [Required]
    public bool IsActive { get; set; }
    public string? Reason { get; set; } // Optional reason for audit purposes
}