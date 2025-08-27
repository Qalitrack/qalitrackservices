namespace QaliTrack.MasterData.Core.Modules.Product.DTOs;

public record CreateProductDto(
    string Name,
    string Code,
    string Category,
    string Unit,
    decimal UnitPrice,
    Guid OrganizationId,
    string? Description = null,
    string? SubCategory = null,
    string? Brand = null,
    string? Model = null,
    decimal Weight = 0,
    string Currency = "USD",
    string? Dimensions = null,
    string? Color = null,
    string? Material = null,
    bool IsHazardous = false,
    string? HazardClass = null,
    string? StorageRequirements = null,
    int MinStockLevel = 0,
    int MaxStockLevel = 0,
    int ReorderLevel = 0,
    string? QualityStandards = null,
    string? Certifications = null,
    string? Notes = null
);

public record UpdateProductDto(
    string Name,
    string Category,
    string Unit,
    decimal UnitPrice,
    string Status,
    string? Description = null,
    string? SubCategory = null,
    string? Brand = null,
    string? Model = null,
    decimal Weight = 0,
    string Currency = "USD",
    string? Dimensions = null,
    string? Color = null,
    string? Material = null,
    bool IsHazardous = false,
    string? HazardClass = null,
    string? StorageRequirements = null,
    int MinStockLevel = 0,
    int MaxStockLevel = 0,
    int ReorderLevel = 0,
    string? QualityStandards = null,
    string? Certifications = null,
    string? Notes = null
);

public record ProductSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public int MinStockLevel { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProductDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string SubCategory { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public decimal Weight { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string? Dimensions { get; init; }
    public string? Color { get; init; }
    public string? Material { get; init; }
    public bool IsHazardous { get; init; }
    public string? HazardClass { get; init; }
    public string? StorageRequirements { get; init; }
    public int MinStockLevel { get; init; }
    public int MaxStockLevel { get; init; }
    public int ReorderLevel { get; init; }
    public string? QualityStandards { get; init; }
    public string? Certifications { get; init; }
    public Guid OrganizationId { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool HasSpecifications { get; init; }
    public bool HasDocuments { get; init; }
    public bool IsLowStock { get; init; }
}

// Product Specification DTOs
public record CreateProductSpecificationDto(
    string SpecificationName,
    string SpecificationValue,
    string Unit = "",
    string SpecificationType = "Technical",
    bool IsCritical = false,
    string? ToleranceRange = null,
    string? TestMethod = null,
    string? Notes = null
);

public record UpdateProductSpecificationDto(
    string SpecificationName,
    string SpecificationValue,
    string Unit = "",
    string SpecificationType = "Technical",
    bool IsCritical = false,
    string? ToleranceRange = null,
    string? TestMethod = null,
    string? Notes = null
);

public record PatchProductSpecificationDto(
    string? SpecificationName = null,
    string? SpecificationValue = null,
    string? Unit = null,
    string? SpecificationType = null,
    bool? IsCritical = null,
    string? ToleranceRange = null,
    string? TestMethod = null,
    string? Notes = null
);

public record ProductSpecificationDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string SpecificationName { get; init; } = string.Empty;
    public string SpecificationValue { get; init; } = string.Empty;
    public string Unit { get; init; } = string.Empty;
    public string SpecificationType { get; init; } = string.Empty;
    public bool IsCritical { get; init; }
    public string? ToleranceRange { get; init; }
    public string? TestMethod { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Product Document DTOs
public record CreateProductDocumentDto(
    string FileName,
    string OriginalFileName,
    string ContentType,
    string FilePath,
    long FileSize,
    string UploadedBy,
    string Category = "General",
    string? Description = null,
    string? Version = null,
    DateTime? ExpiryDate = null,
    string? FileUrl = null
);

public record UpdateProductDocumentDto(
    string FileName,
    string OriginalFileName,
    string ContentType,
    string FilePath,
    long FileSize,
    string UploadedBy,
    string Category = "General",
    string? Description = null,
    string? Version = null,
    DateTime? ExpiryDate = null,
    string? FileUrl = null,
    bool IsActive = true
);

public record PatchProductDocumentDto(
    string? FileName = null,
    string? OriginalFileName = null,
    string? ContentType = null,
    string? FilePath = null,
    long? FileSize = null,
    string? UploadedBy = null,
    string? Category = null,
    string? Description = null,
    string? Version = null,
    DateTime? ExpiryDate = null,
    string? FileUrl = null,
    bool? IsActive = null
);

public record ProductDocumentDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string? FileUrl { get; init; }
    public long FileSize { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Version { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public bool IsActive { get; init; }
    public string UploadedBy { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public bool IsExpired { get; init; }
}