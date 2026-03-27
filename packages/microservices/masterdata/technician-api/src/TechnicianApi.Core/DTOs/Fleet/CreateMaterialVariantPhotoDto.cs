using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.DTOs.Fleet;

public class CreateMaterialVariantPhotoDto
{
    [Required]
    public string MaterialVariantId { get; set; } = string.Empty;

    [Required]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Caption { get; set; }
}
