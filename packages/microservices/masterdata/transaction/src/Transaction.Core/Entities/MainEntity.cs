using System.ComponentModel.DataAnnotations;
using Transaction.Core.Validation;

namespace Transaction.Core.Entities;

public class WeighbridgeTransaction : BaseEntity
{
    // Primary Key
    [Key]
    public int TicketID { get; set; }
    
    // Receipt and Identification
    [Required]
    
    public string ReceiptNo { get; set; } = string.Empty;
    
    // Weight Information
    [Required]
    public string FirstWeight { get; set; } = string.Empty;
    public string? SecondWeight { get; set; }
    public string? NetWeight { get; set; }
    
    // Vehicle Information
    public int? VehicleID { get; set; }
    [Required]
    public string NoPlate { get; set; } = string.Empty;
    [Required]
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
    [Required]
    public int TransporterID { get; set; }
    [Required]
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
    public string? WeighMode { get; set; }
    public string? Operation { get; set; }
    
    // Date Information
    [Required]
    [NotFutureDate]                      
    public DateTime FirstWeightDate { get; set; } = DateTime.Now;
    [Required]
    [NotFutureDate]                      
    public DateTime SecondWeightDate { get; set; } = DateTime.Now;
    
    // Transaction Status and Modifications
    [Required]
    public string Status { get; set; } = "Active";
    public string? ReweighPermission { get; set; }
    public string? ChangeDesc { get; set; }
    public DateTime? ChangeDate { get; set; }
    
    // API Integration
    public int? ApiId { get; set; }
}

// If you still need the ReweighRecord and enum, keep them separate or remove them
// since they're not part of the MySQL tickets table structure

public class ReweighRecord
{
    [Required]
    public int Id { get; set; }
    [Required]
    public int WeighbridgeTransactionId { get; set; }
    [Required]
    public int AttemptNumber { get; set; }
    [Required]
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    [Required]
    public string Status { get; set; } = "Pending";
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public string? PerformedBy { get; set; }
    
    // Weight measurements
    public decimal? Weight1 { get; set; }
    public string? Operator1 { get; set; }
    public DateTime? Weight1Timestamp { get; set; }
    
    public decimal? Weight2 { get; set; }
    public string? Operator2 { get; set; }
    public DateTime? Weight2Timestamp { get; set; }
    
    public decimal? NetWeight { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; }
    
    // Navigation property
    public virtual WeighbridgeTransaction? WeighbridgeTransaction { get; set; }
}

// Value Object for weight measurement (if still needed)
public class WeightMeasurement
{
    public decimal Value { get; private set; }
    public string Unit { get; private set; } = "KG";
    public DateTime MeasuredAt { get; private set; }
    
    public WeightMeasurement(decimal value, DateTime measuredAt, string unit = "KG")
    {
        if (value < 0)
            throw new ArgumentException("Weight cannot be negative", nameof(value));
         
        Value = value;
        Unit = unit;
        MeasuredAt = measuredAt;
    }
}