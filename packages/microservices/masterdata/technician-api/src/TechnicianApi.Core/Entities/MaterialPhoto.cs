using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class MaterialPhoto : BaseEntity
{
    [Required]
    public string MaterialId { get; set; } = string.Empty;

    [Required]
    public string PhotoUrl { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Caption { get; set; }

    // Navigation properties
    public virtual Material Material { get; set; } = null!;
}
