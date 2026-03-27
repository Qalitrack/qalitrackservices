using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class MaterialVariant : BaseEntity
{
    [Required]
    public string MaterialId { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual Material Material { get; set; } = null!;
    public virtual ICollection<MaterialVariantPhoto> Photos { get; set; } = new List<MaterialVariantPhoto>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<TripMaterial> TripMaterials { get; set; } = new List<TripMaterial>();
}
