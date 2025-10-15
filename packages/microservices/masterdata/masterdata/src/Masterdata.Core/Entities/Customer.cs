using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masterdata.Core.Entities;

public class Customer : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Column(TypeName = "jsonb")]
    public string? ContactInfo { get; set; }

    public string? Status { get; set; } = "active";

    public string? Logo { get; set; }
}
