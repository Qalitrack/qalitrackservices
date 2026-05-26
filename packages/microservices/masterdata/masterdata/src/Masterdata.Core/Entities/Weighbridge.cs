using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class Weighbridge : BaseEntity
{
    [Required]
    public string Location { get; set; } = null!;

    public string? Description { get; set; }

    public string? Status { get; set; } = "active";

    /// <summary>Ordered list of scale names attached to this weighbridge.</summary>
    public List<string> Scales { get; set; } = [];
}
