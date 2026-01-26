using Transaction.Core.Entities;

namespace Transaction.Core.DTOs;

// Filter DTO for paginated queries
public class WeighbridgeTransactionFilter
{
    public string? ReceiptNo { get; set; }
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public int? VehicleID { get; set; }
    public int? CommodityID { get; set; }
    public int? SupplierID { get; set; }
    public int? CustomerID { get; set; }
    public int? TransporterID { get; set; }
    public int? OriginID { get; set; }
    public int? DestinationID { get; set; }
    public int? WeighBridgeID { get; set; }
    public int? OperatorID { get; set; }
    public string? Status { get; set; } // Changed from enum to string
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? WeighMode { get; set; }
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Sorting
    public string SortBy { get; set; } = "FirstWeightDate";
    public bool SortDescending { get; set; } = true;
}