using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Organisation : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; } = "active";

    public virtual ICollection<Affiliation> Affiliations { get; set; } = new List<Affiliation>();
}
