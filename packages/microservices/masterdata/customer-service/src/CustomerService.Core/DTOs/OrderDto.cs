namespace CustomerService.Core.DTOs;

public class OrderDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    
    // Transportation details - references Transporter Service
    public string? TransporterId { get; set; }
    public string? TransporterName { get; set; }  // Populated from Transporter Service
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
    
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateOrderDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string OrderType { get; set; } = "Purchase";
    
    // Transportation details - references Transporter Service
    public string? TransporterId { get; set; }
    public string? TransporterName { get; set; }  // Populated from Transporter Service
    public string? RouteId { get; set; }
    public string? OriginLocation { get; set; }
    public string? DestinationLocation { get; set; }
    
    // Quality and specifications
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // References
    public string? CustomerOrderReference { get; set; }
    public string? SupplierOrderReference { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderDto
{
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string? Status { get; set; }
    
    // Transportation updates
    public string? TransporterId { get; set; }
    public string? TransporterName { get; set; }
    public bool? IsCustomerTransporting { get; set; }
    public string? RouteId { get; set; }
    
    // Quality and specifications
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
    
    // Tracking
    public string? WeighingTransactionId { get; set; }
    public string? Notes { get; set; }
}

public class OrderStatusUpdateDto
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? ChangedBy { get; set; }
}

public class OrderSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
}