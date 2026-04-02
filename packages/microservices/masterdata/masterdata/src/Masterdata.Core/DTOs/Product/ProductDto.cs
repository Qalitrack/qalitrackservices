using System;
using Masterdata.Core.Entities;

namespace Masterdata.Core.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public string Status { get; set; } = "Active";
    public string? Image { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
