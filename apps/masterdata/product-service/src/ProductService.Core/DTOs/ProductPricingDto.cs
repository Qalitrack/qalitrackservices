namespace ProductService.Core.DTOs;

public class ProductPricingDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PricingType { get; set; } = string.Empty;
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; }
    public string? CustomerGroup { get; set; }
    public string? Region { get; set; }
}

public class UpdateProductPricingRequest
{
    public List<ProductPricingDto> PricingRules { get; set; } = new List<ProductPricingDto>();
}