using System.ComponentModel.DataAnnotations;

namespace Masterdata.Core.DTOs.Product;

public class UpdateProductDto
{
    [Required]
    public string Code { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }
    public string? Image { get; set; }
}
