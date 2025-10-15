using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class Weighbridge : BaseEntity
{
    [Required]
    public string Location { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; } = "active";
}
