namespace ProductService.Core.Entities;

public class Pricing : BaseEntity
{
    public string ProductId { get; set; } = string.Empty;
    public virtual Product? Product { get; set; }
    
    // Pricing Type and Strategy
    public PricingType Type { get; set; } = PricingType.Standard;
    public PricingStrategy Strategy { get; set; } = PricingStrategy.Fixed;
    
    // Basic Pricing
    public decimal BasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Customer-Specific Pricing
    public string? CustomerId { get; set; } // For customer-specific pricing
    public string? CustomerGroupId { get; set; } // For customer group pricing
    
    // Tiered Pricing
    public int MinQuantity { get; set; } = 1;
    public int? MaxQuantity { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    
    // Validity Period
    public DateTime ValidFrom { get; set; } = DateTime.UtcNow;
    public DateTime? ValidTo { get; set; }
    
    // Promotional Pricing
    public bool IsPromotional { get; set; } = false;
    public string? PromotionCode { get; set; }
    public string? PromotionDescription { get; set; }
    
    // Geographic Pricing
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    
    // Business Rules
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority takes precedence
    public bool RequiresApproval { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Tax Information
    public bool IsTaxInclusive { get; set; } = false;
    public decimal? TaxRate { get; set; }
    public string? TaxCategory { get; set; }
    
    // Cost Information (for margin calculation)
    public decimal? CostPrice { get; set; }
    public decimal? MarginPercentage { get; set; }
    
    // Helper Properties
    public decimal EffectivePrice => SalePrice > 0 ? SalePrice : BasePrice;
    public bool IsExpired => ValidTo.HasValue && ValidTo.Value < DateTime.UtcNow;
    public bool IsValid => IsActive && !IsExpired && ValidFrom <= DateTime.UtcNow;
}

public enum PricingType
{
    Standard,
    CustomerSpecific,
    CustomerGroup,
    Tiered,
    Promotional,
    Geographic,
    Seasonal,
    Volume,
    Contract
}

public enum PricingStrategy
{
    Fixed,
    Dynamic,
    CostPlus,
    MarketBased,
    CompetitorBased,
    ValueBased
}