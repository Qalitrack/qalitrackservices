using Microsoft.AspNetCore.Http;
using Transaction.Core.Entities;

namespace Transaction.Core.DTOs;

public class TransactionReadDto
{
    public int TicketID { get; set; }
    public string ReceiptNo { get; set; } = string.Empty;
    
    // Weight Information
    public string FirstWeight { get; set; } = string.Empty;
    public string? SecondWeight { get; set; }
    public string? NetWeight { get; set; }
    
    // Vehicle Information
    public int? VehicleID { get; set; }
    public string NoPlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    
    // Commodity Information
    public int? CommodityID { get; set; }
    public string? CommodityName { get; set; }
    
    // Supplier Information
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    
    // Customer Information
    public int? CustomerID { get; set; }
    public string? CustomerName { get; set; }
    
    // Transporter Information
    public int TransporterID { get; set; }
    public string TransporterName { get; set; } = string.Empty;
    
    // Origin and Destination
    public int? OriginID { get; set; }
    public string? OriginName { get; set; }
    public int? DestinationID { get; set; }
    public string? DestinationName { get; set; }
    
    // Weighbridge Information - First Weighing
    public int? WeighBridgeID { get; set; }
    public string? WeighBridgeName { get; set; }
    public string? ScaleName { get; set; }
    public int? OperatorID { get; set; }
    public string? OperatorName { get; set; }
    
    // Weighbridge Information - Second Weighing
    public string? WeighBridgeName2nd { get; set; }
    public string? ScaleName2nd { get; set; }
    public string? OperatorID2nd { get; set; }
    public string? OperatorName2nd { get; set; }
    
    // Operational Details
    public WeighingDirection WeighMode { get; set; } = WeighingDirection.Unknown;
    
    // Transaction Status
    public string Status { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    // Reweigh Information
    public bool ReweighPermissionGranted { get; set; }
    public string? ReweighPermissionReason { get; set; }
    public string? ChangeDescription { get; set; }
    public DateTime? ChangeDate { get; set; }
    
    // API Integration
    public int? ApiId { get; set; }
    
    // Base properties
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateTransactionDto
{
    // Vehicle Information (Required)
    public string NoPlate { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public int? VehicleID { get; set; }
    
    // Weight Information (First weight is required)
    public string FirstWeight { get; set; } = string.Empty;
    
    // Transporter Information (Required)
    public int TransporterID { get; set; }
    public string TransporterName { get; set; } = string.Empty;
    
    // Weighing Information (Optional)
    public int? WeighBridgeID { get; set; }
    public string? WeighBridgeName { get; set; }
    public string? ScaleName { get; set; }
    public int? OperatorID { get; set; }
    public string? OperatorName { get; set; }
    
    // Commodity Information (Optional)
    public int? CommodityID { get; set; }
    public string? CommodityName { get; set; }
    
    // Supplier Information (Optional)
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    
    // Customer Information (Optional)
    public int? CustomerID { get; set; }
    public string? CustomerName { get; set; }
    
    // Origin and Destination (Optional)
    public int? OriginID { get; set; }
    public string? OriginName { get; set; }
    public int? DestinationID { get; set; }
    public string? DestinationName { get; set; }
    
    // Direction of the weighing operation (Optional)
    public WeighingDirection WeighMode { get; set; } = WeighingDirection.Unknown;
    
    public string? Notes { get; set; }
}

public class UpdateTransactionDto
{
    // Vehicle Information
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public int? VehicleID { get; set; }
    
    // Weight Information
    public string? SecondWeight { get; set; }
    
    // Commodity Information
    public int? CommodityID { get; set; }
    public string? CommodityName { get; set; }
    
    // Supplier Information
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    
    // Customer Information
    public int? CustomerID { get; set; }
    public string? CustomerName { get; set; }
    
    // Transporter Information
    public int? TransporterID { get; set; }
    public string? TransporterName { get; set; }
    
    // Origin and Destination
    public int? OriginID { get; set; }
    public string? OriginName { get; set; }
    public int? DestinationID { get; set; }
    public string? DestinationName { get; set; }
    
    // Direction of the weighing operation
    public WeighingDirection? WeighMode { get; set; }
    
    public string? ChangeDesc { get; set; }
}

public class AddSecondWeightDto
{
    public int TicketID { get; set; }
    public string SecondWeight { get; set; } = string.Empty;
    public string? WeighBridgeName2nd { get; set; }
    public string? ScaleName2nd { get; set; }
    public string? OperatorID2nd { get; set; }
    public string? OperatorName2nd { get; set; }
    public string? Notes { get; set; }
}

public class CompleteTransactionDto
{
    public int TicketID { get; set; }
}

public class RequestReweighDto
{
    public int TicketID { get; set; }
    public string Reason { get; set; } = string.Empty;
}
