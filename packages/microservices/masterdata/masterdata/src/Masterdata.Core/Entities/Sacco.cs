using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class Sacco : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    public string? RegistrationNumber { get; set; }

    public string? OtherDetails { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    public virtual ICollection<Affiliation> Affiliations { get; set; } = new List<Affiliation>();
}
