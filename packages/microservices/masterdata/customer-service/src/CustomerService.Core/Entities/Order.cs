namespace CustomerService.Core.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "KES";
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public OrderType OrderType { get; set; } = OrderType.Purchase;
    
    // Transportation details - references Transporter Service
    public string? TransporterId { get; set; }  // References Transporter Service ID
    public string? RouteId { get; set; }
    public string? OriginLocation { get; set; }
    public string? DestinationLocation { get; set; }
    
    // Quality and specifications
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Reference and tracking
    public string? CustomerOrderReference { get; set; }
    public string? SupplierOrderReference { get; set; }
    public string? WeighingTransactionId { get; set; }
    
    // Audit and notes
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Customer? Supplier { get; set; }
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
}

public class OrderStatusHistory : BaseEntity
{
    public string OrderId { get; set; } = string.Empty;
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    InProduction,
    ReadyForShipment,
    InTransit,
    AtDestination,
    WeighingInProgress,
    Completed,
    Cancelled,
    Disputed
}

public enum OrderType
{
    Purchase,    // Customer buying from supplier
    Sale,        // Customer selling to buyer
    Transfer,    // Internal transfer
    Sample       // Sample order for testing
}