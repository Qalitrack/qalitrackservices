namespace QaliTrack.DataManager.Core.Modules.Orders.DTOs;

// Customer Order DTOs
public class CustomerOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid FromSiteId { get; set; }
    public Guid? ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsVisibleAtGate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ICollection<CustomerOrderLineDto> OrderLines { get; set; } = new List<CustomerOrderLineDto>();
}

public class CreateCustomerOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid FromSiteId { get; set; }
    public Guid? ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsVisibleAtGate { get; set; } = false;
    public string CreatedBy { get; set; } = string.Empty;
}

public class UpdateCustomerOrderDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Guid FromSiteId { get; set; }
    public Guid? ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public bool IsVisibleAtGate { get; set; }
}

// Customer Order Line DTOs
public class CustomerOrderLineDto
{
    public Guid Id { get; set; }
    public Guid CustomerOrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityDelivered { get; set; }
    public decimal UnitPrice { get; set; }
    public string LineStatus { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCustomerOrderLineDto
{
    public Guid CustomerOrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public string LineStatus { get; set; } = "Pending";
    public string? SpecialInstructions { get; set; }
}

public class UpdateCustomerOrderLineDto
{
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityDelivered { get; set; }
    public decimal UnitPrice { get; set; }
    public string LineStatus { get; set; } = string.Empty;
    public string? SpecialInstructions { get; set; }
}

// Purchase Order DTOs
public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? TermsConditions { get; set; }
    public string? HodimReference { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ICollection<PurchaseOrderLineDto> OrderLines { get; set; } = new List<PurchaseOrderLineDto>();
}

public class CreatePurchaseOrderDto
{
    public string PoNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = "Open";
    public decimal TotalAmount { get; set; }
    public string? TermsConditions { get; set; }
    public string? HodimReference { get; set; }
}

public class UpdatePurchaseOrderDto
{
    public string PoNumber { get; set; } = string.Empty;
    public Guid SupplierId { get; set; }
    public Guid ToSiteId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? TermsConditions { get; set; }
    public string? HodimReference { get; set; }
}

// Purchase Order Line DTOs
public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitPrice { get; set; }
    public string? QualitySpecifications { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreatePurchaseOrderLineDto
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal UnitPrice { get; set; }
    public string? QualitySpecifications { get; set; }
}

public class UpdatePurchaseOrderLineDto
{
    public Guid ProductVariantId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitPrice { get; set; }
    public string? QualitySpecifications { get; set; }
}

// Inter Plant Transfer DTOs
public class InterPlantTransferDto
{
    public Guid Id { get; set; }
    public Guid FromSiteId { get; set; }
    public Guid ToSiteId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DispatchTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? QrCode { get; set; }
    public decimal QuantitySent { get; set; }
    public decimal QuantityReceived { get; set; }
    public string? TransferNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Related data
    public ICollection<TransferLineDto> TransferLines { get; set; } = new List<TransferLineDto>();
}

public class CreateInterPlantTransferDto
{
    public Guid FromSiteId { get; set; }
    public Guid ToSiteId { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Initiated";
    public DateTime? DispatchTime { get; set; }
    public string? QrCode { get; set; }
    public decimal QuantitySent { get; set; }
    public string? TransferNotes { get; set; }
}

public class UpdateInterPlantTransferDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime? DispatchTime { get; set; }
    public DateTime? ArrivalTime { get; set; }
    public string? QrCode { get; set; }
    public decimal QuantitySent { get; set; }
    public decimal QuantityReceived { get; set; }
    public string? TransferNotes { get; set; }
}

// Transfer Line DTOs
public class TransferLineDto
{
    public Guid Id { get; set; }
    public Guid InterPlantTransferId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? QualityGrade { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTransferLineDto
{
    public Guid InterPlantTransferId { get; set; }
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? QualityGrade { get; set; }
}

public class UpdateTransferLineDto
{
    public Guid ProductVariantId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? QualityGrade { get; set; }
}