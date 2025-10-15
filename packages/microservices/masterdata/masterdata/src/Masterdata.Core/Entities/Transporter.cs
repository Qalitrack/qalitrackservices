using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Transporter : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    public string? Status { get; set; } = "active";

    public string? Logo { get; set; }

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
