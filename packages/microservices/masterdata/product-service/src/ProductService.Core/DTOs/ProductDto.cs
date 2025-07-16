using ProductService.Core.Entities;

namespace ProductService.Core.DTOs;

public class ProductReadDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    
    // Catalog Information
    public string Brand { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    
    // Physical Properties
    public decimal Weight { get; set; }
    public string WeightUnit { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public string DimensionUnit { get; set; } = string.Empty;
    public decimal Volume { get; set; }
    public string VolumeUnit { get; set; } = string.Empty;
    
    // Inventory Properties
    public int StockQuantity { get; set; }
    public int MinimumStock { get; set; }
    public int MaximumStock { get; set; }
    public int ReorderLevel { get; set; }
    public bool TrackInventory { get; set; }
    
    // Validity and Lifecycle
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    
    // Category
    public string CategoryId { get; set; } = string.Empty;
    public CategoryReadDto? Category { get; set; }
    
    // Related Data
    public ICollection<PricingReadDto> Pricings { get; set; } = new List<PricingReadDto>();
    public ICollection<SpecificationReadDto> Specifications { get; set; } = new List<SpecificationReadDto>();
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductDto
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
    
    // Category
    public string CategoryId { get; set; } = string.Empty;
}

public class UpdateProductDto
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
    
    // Category
    public string CategoryId { get; set; } = string.Empty;
}
// DT
Os for integration compatibility
public class ProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RegisterProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = string.Empty;
}

public class ProductPricingDto
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string PricingType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}