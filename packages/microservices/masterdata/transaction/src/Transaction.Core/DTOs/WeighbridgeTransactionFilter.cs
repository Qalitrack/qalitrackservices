namespace Transaction.Core.DTOs;

public class WeighbridgeTransactionFilter
{
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    
    // Sorting
    public string SortBy { get; set; } = "FirstWeightDate";
    public bool SortDescending { get; set; } = true;
    
    // Filters
    public string? ReceiptNo { get; set; }
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public string? VehicleID { get; set; }
    public string? CommodityID { get; set; }
    public string? SupplierID { get; set; }
    public string? CustomerID { get; set; }
    public string? TransporterID { get; set; }
    public string? OriginID { get; set; }
    public string? DestinationID { get; set; }
    public string? WeighBridgeID { get; set; }
    public string? OperatorID { get; set; }
    public string? Status { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? WeighMode { get; set; }
}