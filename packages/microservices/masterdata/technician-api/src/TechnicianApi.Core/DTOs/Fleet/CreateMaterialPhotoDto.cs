using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.DTOs.Fleet;

public class CreateMaterialPhotoDto
{
    [Required]
    public string MaterialId { get; set; } = string.Empty;

    [Required]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Caption { get; set; }
}
