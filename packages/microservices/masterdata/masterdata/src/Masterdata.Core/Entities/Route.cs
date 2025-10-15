using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class Route : BaseEntity
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string StartPoint { get; set; } = null!;

    [Required]
    public string EndPoint { get; set; } = null!;

    public string? Status { get; set; } = "active";
}
