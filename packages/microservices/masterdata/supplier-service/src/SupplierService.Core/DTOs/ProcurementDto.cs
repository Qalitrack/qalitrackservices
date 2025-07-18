using SupplierService.Core.Entities;

namespace SupplierService.Core.DTOs;

public class ProcurementDto
{
    public string Id { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string ProcurementNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public ProcurementStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public string? Currency { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? ApprovedBy { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? Notes { get; set; }
    public ProcurementPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation Properties
    public SupplierDto? Supplier { get; set; }
    public ICollection<ProcurementItemDto> Items { get; set; } = new List<ProcurementItemDto>();
}

public class ProcurementItemDto
{
    public string Id { get; set; } = string.Empty;
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
}

public class CreateProcurementRequest
{
    public string SupplierId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public string? Currency { get; set; } = "USD";
    public string RequestedBy { get; set; } = string.Empty;
    public string? DeliveryAddress { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? Notes { get; set; }
    public ProcurementPriority Priority { get; set; } = ProcurementPriority.Medium;
    public ICollection<CreateProcurementItemRequest> Items { get; set; } = new List<CreateProcurementItemRequest>();
}

public class CreateProcurementItemRequest
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? UnitPrice { get; set; }
    public string? Specifications { get; set; }
    public string? Notes { get; set; }
}

public class UpdateProcurementRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProcurementType ProcurementType { get; set; }
    public ProcurementStatus Status { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public decimal? EstimatedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public string? Currency { get; set; }
    public string? ApprovedBy { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? SpecialInstructions { get; set; }
    public string? Notes { get; set; }
    public ProcurementPriority Priority { get; set; }
}

// DTOs for integration compatibility with tests
public class CreateSupplierProductRequest
{
    public string ProductId { get; set; } = string.Empty;
    public decimal SupplierPrice { get; set; }
    public string Currency { get; set; } = "KES";
    public int MinimumOrderQuantity { get; set; }
    public int LeadTimeDays { get; set; }
    public bool IsPreferred { get; set; } = false;
}

public class CreateSupplierPerformanceRequest
{
    public DateTime EvaluationDate { get; set; }
    public int QualityScore { get; set; }
    public int DeliveryScore { get; set; }
    public int ServiceScore { get; set; }
    public decimal OverallScore { get; set; }
    public string? Comments { get; set; }
    public string? EvaluatedBy { get; set; }
}