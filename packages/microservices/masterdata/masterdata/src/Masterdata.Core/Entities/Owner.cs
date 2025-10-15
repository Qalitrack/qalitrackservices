using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Owner : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    public string? Type { get; set; } // e.g., Individual, Company

    [InverseProperty(nameof(Vehicle.Owner))]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
