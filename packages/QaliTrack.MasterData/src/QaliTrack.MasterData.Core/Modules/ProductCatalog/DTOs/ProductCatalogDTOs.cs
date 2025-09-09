namespace QaliTrack.MasterData.Core.Modules.ProductCatalog.DTOs;

// ProductCategory DTOs
public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ProductCategoryDto? ParentCategory { get; set; }
    public ICollection<ProductCategoryDto> SubCategories { get; set; } = new List<ProductCategoryDto>();
}

public class CreateProductCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}

public class UpdateProductCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

// PackagingType DTOs
public class PackagingTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool RequiresContainer { get; set; }
    public string? HandlingEquipment { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePackagingTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool RequiresContainer { get; set; } = false;
    public string? HandlingEquipment { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePackagingTypeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public bool RequiresContainer { get; set; }
    public string? HandlingEquipment { get; set; }
    public bool IsActive { get; set; }
}

// ProductBase DTOs
public class ProductBaseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Grade { get; set; }
    public string? ChemicalFormula { get; set; }
    public decimal? StandardDensity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ProductCategoryDto? Category { get; set; }
    public ICollection<ProductSpecificationDto> Specifications { get; set; } = new List<ProductSpecificationDto>();
}

public class CreateProductBaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Grade { get; set; }
    public string? ChemicalFormula { get; set; }
    public decimal? StandardDensity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductBaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string? Grade { get; set; }
    public string? ChemicalFormula { get; set; }
    public decimal? StandardDensity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// ProductVariant DTOs
public class ProductVariantDto
{
    public Guid Id { get; set; }
    public Guid ProductBaseId { get; set; }
    public Guid PackagingTypeId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public decimal? UnitWeight { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ProductBaseDto? ProductBase { get; set; }
    public PackagingTypeDto? PackagingType { get; set; }
}

public class CreateProductVariantDto
{
    public Guid ProductBaseId { get; set; }
    public Guid PackagingTypeId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public decimal? UnitWeight { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = "KSH";
    public bool IsActive { get; set; } = true;
}

public class UpdateProductVariantDto
{
    public Guid ProductBaseId { get; set; }
    public Guid PackagingTypeId { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string VariantCode { get; set; } = string.Empty;
    public decimal? UnitWeight { get; set; }
    public decimal PricePerUnit { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ProductSpecification DTOs
public class ProductSpecificationDto
{
    public Guid Id { get; set; }
    public Guid ProductBaseId { get; set; }
    public string SpecCategory { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public string? SpecUnit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TypicalValue { get; set; }
    public string? TestMethod { get; set; }
    public bool IsMandatory { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductSpecificationDto
{
    public Guid ProductBaseId { get; set; }
    public string SpecCategory { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public string? SpecUnit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TypicalValue { get; set; }
    public string? TestMethod { get; set; }
    public bool IsMandatory { get; set; } = true;
}

public class UpdateProductSpecificationDto
{
    public Guid ProductBaseId { get; set; }
    public string SpecCategory { get; set; } = string.Empty;
    public string SpecName { get; set; } = string.Empty;
    public string? SpecUnit { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TypicalValue { get; set; }
    public string? TestMethod { get; set; }
    public bool IsMandatory { get; set; }
}

// SiteCapability DTOs
public class SiteCapabilityDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string CapabilityType { get; set; } = string.Empty;
    public Guid ProductCategoryId { get; set; }
    public decimal? MaxCapacityTons { get; set; }
    public string? OperationalHours { get; set; }
    public bool RequiresSpecialEquipment { get; set; }
    public string? EquipmentRequired { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ProductCategoryDto? ProductCategory { get; set; }
}

public class CreateSiteCapabilityDto
{
    public Guid SiteId { get; set; }
    public string CapabilityType { get; set; } = string.Empty;
    public Guid ProductCategoryId { get; set; }
    public decimal? MaxCapacityTons { get; set; }
    public string? OperationalHours { get; set; }
    public bool RequiresSpecialEquipment { get; set; } = false;
    public string? EquipmentRequired { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSiteCapabilityDto
{
    public Guid SiteId { get; set; }
    public string CapabilityType { get; set; } = string.Empty;
    public Guid ProductCategoryId { get; set; }
    public decimal? MaxCapacityTons { get; set; }
    public string? OperationalHours { get; set; }
    public bool RequiresSpecialEquipment { get; set; }
    public string? EquipmentRequired { get; set; }
    public bool IsActive { get; set; }
}

// ProductUsagePermission DTOs
public class ProductUsagePermissionDto
{
    public Guid Id { get; set; }
    public Guid ProductVariantId { get; set; }
    public string UsageType { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public bool IsPermitted { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ProductVariantDto? ProductVariant { get; set; }
    public object? Site { get; set; }
}

public class CreateProductUsagePermissionDto
{
    public Guid ProductVariantId { get; set; }
    public string UsageType { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public bool IsPermitted { get; set; } = true;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

public class UpdateProductUsagePermissionDto
{
    public string UsageType { get; set; } = string.Empty;
    public bool IsPermitted { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}