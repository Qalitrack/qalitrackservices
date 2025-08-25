namespace SupplierService.Core.Entities;

public class SupplierPricing : BaseEntity
{
    public string SupplierProductId { get; set; } = string.Empty;
    public PricingType Type { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinQuantity { get; set; } = 1;
    public int? MaxQuantity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Navigation Properties
    public virtual SupplierProduct SupplierProduct { get; set; } = null!;
}

public enum PricingType
{
    Standard,
    Bulk,
    Tier,
    Contract,
    Promotional,
    Seasonal
}