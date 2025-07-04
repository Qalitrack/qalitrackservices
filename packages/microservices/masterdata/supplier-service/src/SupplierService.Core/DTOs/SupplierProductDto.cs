namespace SupplierService.Core.DTOs;

public class SupplierProductDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SupplierProductCode { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; }
    public string? UnitOfMeasure { get; set; }
    public int? MinimumOrderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public bool IsActive { get; set; }
    public DateTime? DiscontinuedDate { get; set; }
    public string? Specifications { get; set; }
    public string? QualityCertifications { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupplierProductRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SupplierProductCode { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? UnitOfMeasure { get; set; }
    public int? MinimumOrderQuantity { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? Specifications { get; set; }
    public string? QualityCertifications { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? Notes { get; set; }
}