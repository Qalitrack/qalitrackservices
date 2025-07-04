namespace ProductService.Core.Entities;

public class ProductPricing : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public PricingType PricingType { get; set; } = PricingType.Standard;
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CustomerGroup { get; set; }
    public string? Region { get; set; }

    // Navigation properties
    public virtual Product? Product { get; set; }
}

public enum PricingType
{
    Standard,
    Volume,
    Contract,
    Promotional,
    Seasonal
}