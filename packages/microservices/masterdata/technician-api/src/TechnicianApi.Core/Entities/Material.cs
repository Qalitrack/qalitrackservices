using System.ComponentModel.DataAnnotations;

namespace TechnicianApi.Core.Entities;

public class Material : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<MaterialVariant> Variants { get; set; } = new List<MaterialVariant>();
    public virtual ICollection<MaterialPhoto> Photos { get; set; } = new List<MaterialPhoto>();
    public virtual ICollection<MaterialCost> Costs { get; set; } = new List<MaterialCost>();
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
    public virtual ICollection<TripMaterial> TripMaterials { get; set; } = new List<TripMaterial>();
}
