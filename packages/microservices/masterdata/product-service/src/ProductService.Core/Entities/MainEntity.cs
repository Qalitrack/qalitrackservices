namespace ProductService.Core.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public ProductType Type { get; set; } = ProductType.Standard;
    
    // Catalog Information
    public string Brand { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    
    // Physical Properties
    public decimal Weight { get; set; }
    public string WeightUnit { get; set; } = "kg";
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string DimensionUnit { get; set; } = "cm";
    public decimal Volume { get; set; }
    public string VolumeUnit { get; set; } = "m3";
    
    // Inventory Properties
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool TrackInventory { get; set; } = true;
    
    // Validity and Lifecycle
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    
    // Category Relationship
    public string CategoryId { get; set; } = string.Empty;
    public virtual Category? Category { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Pricing> Pricings { get; set; } = new List<Pricing>();
    public virtual ICollection<Specification> Specifications { get; set; } = new List<Specification>();
}

public enum ProductStatus
{
    Active,
    Inactive,
    Suspended,
    Pending,
    Discontinued
}

public enum ProductType
{
    Standard,
    Service,
    Digital,
    Bundle,
    Subscription
}