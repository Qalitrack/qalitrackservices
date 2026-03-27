using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.DTOs.Settings;

public class CreateLicenseClassDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
