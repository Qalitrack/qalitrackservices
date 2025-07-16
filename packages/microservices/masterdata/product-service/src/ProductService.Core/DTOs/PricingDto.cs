using ProductService.Core.Entities;

namespace ProductService.Core.DTOs;

public class PricingReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    
    // Pricing Type and Strategy
    public string Type { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    
    // Basic Pricing
    public decimal BasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    
    // Customer-Specific Pricing
    public string? CustomerId { get; set; }
    public string? CustomerGroupId { get; set; }
    
    // Tiered Pricing
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    
    // Validity Period
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    
    // Promotional Pricing
    public bool IsPromotional { get; set; }
    public string? PromotionCode { get; set; }
    public string? PromotionDescription { get; set; }
    
    // Geographic Pricing
    public string? Region { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    
    // Business Rules
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public bool RequiresApproval { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Tax Information
    public bool IsTaxInclusive { get; set; }
    public decimal? TaxRate { get; set; }
    public string? TaxCategory { get; set; }
    
    // Cost Information
    public decimal? CostPrice { get; set; }
    public decimal? MarginPercentage { get; set; }
    
    // Helper Properties
    public decimal EffectivePrice { get; set; }
    public bool IsExpired { get; set; }
    public bool IsValid { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePricingDto
{
    public string ProductId { get; set; } = string.Empty;
    
    // Pricing Type and Strategy
    public PricingType Type { get; set; } = PricingType.Standard;
    public PricingStrategy Strategy { get; set; } = PricingStrategy.Fixed;
    
    // Basic Pricing
    public decimal BasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Customer-Specific Pricing
    public string? CustomerId { get; set; }
    public string? CustomerGroupId { get; set; }
    
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
    public int Priority { get; set; } = 0;
    public bool RequiresApproval { get; set; } = false;
    
    // Tax Information
    public bool IsTaxInclusive { get; set; } = false;
    public decimal? TaxRate { get; set; }
    public string? TaxCategory { get; set; }
    
    // Cost Information
    public decimal? CostPrice { get; set; }
    public decimal? MarginPercentage { get; set; }
}

public class UpdatePricingDto
{
    // Pricing Type and Strategy
    public PricingType Type { get; set; } = PricingType.Standard;
    public PricingStrategy Strategy { get; set; } = PricingStrategy.Fixed;
    
    // Basic Pricing
    public decimal BasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = "USD";
    
    // Customer-Specific Pricing
    public string? CustomerId { get; set; }
    public string? CustomerGroupId { get; set; }
    
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
    public int Priority { get; set; } = 0;
    public bool RequiresApproval { get; set; } = false;
    
    // Tax Information
    public bool IsTaxInclusive { get; set; } = false;
    public decimal? TaxRate { get; set; }
    public string? TaxCategory { get; set; }
    
    // Cost Information
    public decimal? CostPrice { get; set; }
    public decimal? MarginPercentage { get; set; }
}public cla
ss PricingReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public decimal EffectivePrice { get; set; }
}