namespace CustomerService.Core.DTOs;

public class OrderReadDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string? SupplierId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public string? TransporterId { get; set; }
    public string? RouteId { get; set; }
    public string? OriginLocation { get; set; }
    public string? DestinationLocation { get; set; }
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
    public string? CustomerOrderReference { get; set; }
    public string? SupplierOrderReference { get; set; }
    public string? WeighingTransactionId { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelledBy { get; set; }
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
    public string? TransporterId { get; set; }
    public string? RouteId { get; set; }
    public string? OriginLocation { get; set; }
    public string? DestinationLocation { get; set; }
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
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
    public string? TransporterId { get; set; }
    public string? RouteId { get; set; }
    public string? QualitySpecifications { get; set; }
    public string? SpecialInstructions { get; set; }
    public decimal? TolerancePercentage { get; set; }
    public string? WeighingTransactionId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? ChangedBy { get; set; }
}

public class CancelOrderDto
{
    public string? CancellationReason { get; set; }
    public string? CancelledBy { get; set; }
}

public class OrderStatusHistoryReadDto
{
    public string Id { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}