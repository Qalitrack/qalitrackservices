using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Orders.Entities;

/// <summary>
/// Purchase orders to suppliers for raw materials
/// </summary>
public class PurchaseOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; } // Reference to BusinessEntity in MasterData
    public Guid ToSiteId { get; set; } // Delivery site
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, InProgress, Completed, Cancelled
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<PurchaseOrderLine> OrderLines { get; set; } = new List<PurchaseOrderLine>();
}

/// <summary>
/// Individual line items within purchase orders
/// </summary>
public class PurchaseOrderLine : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductVariantId { get; set; } // Reference to ProductVariant in MasterData
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public string LineStatus { get; set; } = "Pending"; // Pending, InProgress, Received, Cancelled
    public string? SpecialInstructions { get; set; }

    // Navigation properties
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}