using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class ProductCatalogDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReorderLevel { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime LastSyncDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ComplianceRequirements { get; set; }
    public string? QualitySpecifications { get; set; }
    public string? SupplierIds { get; set; }
    public string? MasterDataVersion { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public decimal MinOrderQuantity { get; set; }
    public decimal MaxOrderQuantity { get; set; }
    public List<string> AllowedVehicleTypes { get; set; } = new();
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductValidationResult
{
    public bool IsValid { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string VehicleId { get; set; } = string.Empty;
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public ProductCatalogDto? Product { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

public class ProductPricingDto
{
    public string ProductId { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime PricingDate { get; set; }
    public List<PricingTier> PricingTiers { get; set; } = new();
    public List<PricingAdjustment> Adjustments { get; set; } = new();
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? DiscountValidUntil { get; set; }
    public string? Currency { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class PricingTier
{
    public string Name { get; set; } = string.Empty;
    public decimal MinQuantity { get; set; }
    public decimal MaxQuantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal? DiscountPercentage { get; set; }
}

public class PricingAdjustment
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public DateTime AppliedAt { get; set; }
    public string? AppliedBy { get; set; }
}

public class SyncProductRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public bool ForceSync { get; set; } = false;
    public List<string> SyncOptions { get; set; } = new();
}

public class UpdateProductAvailabilityRequest
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Operation { get; set; } = "SET"; // SET, ADD, SUBTRACT
    public string? Reason { get; set; }
    public string? UpdatedBy { get; set; }
}