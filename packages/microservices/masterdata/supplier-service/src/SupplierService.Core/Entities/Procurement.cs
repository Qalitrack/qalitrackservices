namespace SupplierService.Core.Entities;

public class Procurement : BaseEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string ProcurementNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public ProcurementStatus Status { get; set; } = ProcurementStatus.Draft;
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? Notes { get; set; }
    public ProcurementPriority Priority { get; set; } = ProcurementPriority.Medium;

    // Navigation Properties
    public virtual Supplier Supplier { get; set; } = null!;
    public virtual ICollection<ProcurementItem> Items { get; set; } = new List<ProcurementItem>();
}

public class ProcurementItem : BaseEntity
{
    public string ProcurementId { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public decimal? TotalPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Procurement Procurement { get; set; } = null!;
}

public enum ProcurementType
{
    Goods,
    Services,
    Works,
    Consultancy,
    Maintenance,
    Emergency
}

public enum ProcurementStatus
{
    Draft,
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Ordered,
    PartiallyDelivered,
    Delivered,
    Completed,
    Cancelled
}

public enum ProcurementPriority
{
    Low,
    Medium,
    High,
    Critical,
    Emergency
}