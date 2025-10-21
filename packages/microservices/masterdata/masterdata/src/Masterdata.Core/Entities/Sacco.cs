using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Sacco : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    [Column(TypeName = "jsonb")]
    public string? OtherDetails { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    public virtual ICollection<Affiliation> Affiliations { get; set; } = new List<Affiliation>();
}
