using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Orders.Entities;

/// <summary>
/// Customer sales orders for cement products
/// </summary>
public class CustomerOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; } // Reference to BusinessEntity in MasterData
    public Guid FromSiteId { get; set; } // Reference to Site in MasterData
    public Guid? ToSiteId { get; set; } // For delivery orders
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, InProgress, Completed, Cancelled
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsVisibleAtGate { get; set; } = false;
    public string CreatedBy { get; set; } = string.Empty;

    // Navigation properties
    public virtual ICollection<CustomerOrderLine> OrderLines { get; set; } = new List<CustomerOrderLine>();
}

/// <summary>
/// Individual line items within customer orders
/// </summary>
public class CustomerOrderLine : BaseEntity
{
    public Guid CustomerOrderId { get; set; }
    public Guid ProductVariantId { get; set; } // Reference to ProductVariant in MasterData
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityDelivered { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public string LineStatus { get; set; } = "Pending"; // Pending, InProgress, Delivered, Cancelled
    public string? SpecialInstructions { get; set; }

    // Navigation properties
    public virtual CustomerOrder CustomerOrder { get; set; } = null!;
}