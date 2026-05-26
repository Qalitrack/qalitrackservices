using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Sacco;

public class CreateSaccoDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters")]
    public string Name { get; set; } = null!;

    public string? RegistrationNumber { get; set; }

    public string? OtherDetails { get; set; }
}
