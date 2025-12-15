namespace Transaction.Core.DTOs;

public class TransactionReadDto
{
    public string Id { get; set; } = string.Empty;
    public string ReceiptNo { get; set; } = string.Empty;
    public int ExpectedWeighings { get; set; }
    public int CompletedWeighings { get; set; }
    
    // Weight Information
    public decimal? FirstWeight { get; set; }
    public DateTime? FirstWeightTimestamp { get; set; }
    public decimal? SecondWeight { get; set; }
    public DateTime? SecondWeightTimestamp { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? NetWeightCalculatedTimestamp { get; set; }
    
    // All weighings in sequence
    public List<WeighingRecordDto> WeighingRecords { get; set; } = new();
    
    // Vehicle Information
    public Guid? VehicleId { get; set; }
    public string NoPlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    
    // Commodity Information
    public Guid? CommodityId { get; set; }
    public string CommodityName { get; set; } = string.Empty;
    
    // Supplier Information
    public Guid? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    
    // Customer Information
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    // Transporter Information
    public Guid TransporterId { get; set; }
    public string TransporterName { get; set; } = string.Empty;
    
    // Origin and Destination
    public Guid? OriginId { get; set; }
    public string OriginName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    
    // Weighbridge Information - First Weighing
    public Guid? WeighBridgeId { get; set; }
    public string WeighBridgeName { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public Guid? OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    
    // Weighbridge Information - Second Weighing
    public string WeighBridgeName2nd { get; set; } = string.Empty;
    public string ScaleName2nd { get; set; } = string.Empty;
    public Guid? OperatorId2nd { get; set; }
    public string OperatorName2nd { get; set; } = string.Empty;
    
    // Operational Details
    public string WeighMode { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    
    // Transaction Status
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Reweigh Information
    public bool ReweighPermissionGranted { get; set; }
    public string? ReweighPermissionReason { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime? ChangeDate { get; set; }
    
    // Base properties
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTransactionDto
{
    public string ReceiptNo { get; set; } = string.Empty;
    public int ExpectedWeighings { get; set; } = 2; // Default to 2 weighings
    
    // Vehicle Information (Required)
    public string NoPlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    
    // Transporter Information (Required)
    public Guid TransporterId { get; set; }
    public string TransporterName { get; set; } = string.Empty;
    
    // First Weight Information (Optional - can be added later)
    public decimal? FirstWeight { get; set; }
    public Guid? WeighBridgeId { get; set; }
    public string WeighBridgeName { get; set; } = string.Empty;
    public string ScaleName { get; set; } = string.Empty;
    public Guid? OperatorId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    
    // Commodity Information (Optional)
    public Guid? CommodityId { get; set; }
    public string CommodityName { get; set; } = string.Empty;
    
    // Supplier Information (Optional)
    public Guid? SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    
    // Customer Information (Optional)
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    
    // Origin and Destination (Optional)
    public Guid? OriginId { get; set; }
    public string OriginName { get; set; } = string.Empty;
    public Guid? DestinationId { get; set; }
    public string DestinationName { get; set; } = string.Empty;
    
    // Operational Details (Optional)
    public string WeighMode { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
}

public class UpdateTransactionDto
{
    
    // Vehicle Information
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public Guid? VehicleId { get; set; }
    
    // Commodity Information
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    
    // Supplier Information
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    
    // Customer Information
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    
    // Transporter Information
    public Guid? TransporterId { get; set; }
    public string? TransporterName { get; set; }
    
    // Origin and Destination
    public Guid? OriginId { get; set; }
    public string? OriginName { get; set; }
    public Guid? DestinationId { get; set; }
    public string? DestinationName { get; set; }
    
    // Operational Details
    public string? WeighMode { get; set; }
    public string? Operation { get; set; }
    
    public string? ChangeDescription { get; set; }
}


public class CompleteTransactionDto
{
    public string TransactionId { get; set; } = string.Empty;
}

public class RequestReweighDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}