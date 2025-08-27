using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Core.Modules.Product.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string Unit { get; set; } = string.Empty; // kg, liters, pieces, etc.
    public decimal Weight { get; set; }
    public string? Dimensions { get; set; } // JSON: length, width, height
    public string? Color { get; set; }
    public string? Material { get; set; }
    public bool IsHazardous { get; set; } = false;
    public string? HazardClass { get; set; }
    public string? StorageRequirements { get; set; }
    public int MinStockLevel { get; set; } = 0;
    public int MaxStockLevel { get; set; } = 0;
    public int ReorderLevel { get; set; } = 0;
    public string? QualityStandards { get; set; } // JSON array of quality standards
    public string? Certifications { get; set; } // JSON array of certifications
    public string? ImageUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public Guid OrganizationId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<ProductSpecification> Specifications { get; set; } = new List<ProductSpecification>();
    public virtual ICollection<ProductDocument> Documents { get; set; } = new List<ProductDocument>();
    public virtual ICollection<ProductPricing> Pricing { get; set; } = new List<ProductPricing>();
    
    // Cross-module relationships (handled via DbContext configuration)
    // public virtual ICollection<ProductSupplierCatalog> SupplierCatalogs { get; set; } = new List<ProductSupplierCatalog>();
}

public class ProductSpecification : BaseEntity
{
    public Guid ProductId { get; set; }
    public string SpecificationName { get; set; } = string.Empty;
    public string SpecificationValue { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string SpecificationType { get; set; } = "Technical"; // Technical, Quality, Safety, etc.
    public bool IsCritical { get; set; } = false;
    public string? ToleranceRange { get; set; }
    public string? TestMethod { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}

public class ProductDocument : BaseEntity
{
    public Guid ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public long FileSize { get; set; }
    public string Category { get; set; } = "General"; // Datasheet, Manual, Certificate, Image, etc.
    public string? Description { get; set; }
    public string? Version { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}

public class ProductPricing : BaseEntity
{
    public Guid ProductId { get; set; }
    public string PriceType { get; set; } = "Standard"; // Standard, Bulk, Wholesale, Retail, etc.
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public int MinQuantity { get; set; } = 1;
    public int? MaxQuantity { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Region { get; set; }
    public string? CustomerType { get; set; } // Regular, Premium, Wholesale, etc.
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}

// Enumerations
public enum ProductStatus
{
    Active,
    Inactive,
    Discontinued,
    OutOfStock,
    Pending
}

public enum ProductCategory
{
    Raw,
    Finished,
    SemiFinished,
    Consumable,
    Service,
    Digital
}

public enum SpecificationType
{
    Technical,
    Quality,
    Safety,
    Environmental,
    Legal,
    Performance
}

public enum DocumentCategory
{
    General,
    Datasheet,
    Manual,
    Certificate,
    Image,
    Video,
    Legal,
    Safety
}

public enum PriceType
{
    Standard,
    Bulk,
    Wholesale,
    Retail,
    Promotional,
    Contract
}