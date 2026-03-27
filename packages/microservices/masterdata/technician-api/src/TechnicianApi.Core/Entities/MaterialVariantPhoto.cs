using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class MaterialVariantPhoto : BaseEntity
{
    [Required]
    public string MaterialVariantId { get; set; } = string.Empty;

    [Required]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Caption { get; set; }

    // Navigation properties
    public virtual MaterialVariant MaterialVariant { get; set; } = null!;
}
