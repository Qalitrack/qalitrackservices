namespace ProductService.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal? Weight { get; set; }
    public decimal? Density { get; set; }
    public bool IsHazardous { get; set; }
    public string? HazmatClass { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ProductCategory? Category { get; set; }
    public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
    public virtual ICollection<ProductPricing> Pricing { get; set; } = new List<ProductPricing>();
    public virtual ProductHazmat? HazmatInfo { get; set; }
    public virtual ICollection<ProductCompliance> ComplianceRequirements { get; set; } = new List<ProductCompliance>();
    public virtual ProductInventory? Inventory { get; set; }
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}

public enum ProductStatus
{
    Active,
    Inactive,
    Discontinued,
    Pending
}