using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.Entities;

public class Product : BaseEntity
{
    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Image { get; set; }
}
