namespace ProductService.Core.Entities;

public class ProductVariant : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string VariantType { get; set; } = string.Empty; // e.g., Size, Color, Grade
    public string VariantValue { get; set; } = string.Empty; // e.g., Large, Red, Premium
    public decimal? PriceAdjustment { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}