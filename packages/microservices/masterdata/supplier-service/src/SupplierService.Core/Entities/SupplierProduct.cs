namespace SupplierService.Core.Entities;

public class SupplierProduct : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
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
    public bool IsActive { get; set; } = true;
    public DateTime? DiscontinuedDate { get; set; }
    public string? Specifications { get; set; }
    public string? QualityCertifications { get; set; }
    public string? ComplianceStandards { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
}