using Transaction.Core.Entities;

namespace Transaction.Core.DTOs;

// Filter DTO for paginated queries
public class WeighbridgeTransactionFilter
{
    public string? ReceiptNo { get; set; }
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public int? VehicleId { get; set; }
    public int? ProductId { get; set; }
    public int? SupplierId { get; set; }
    public int? CustomerId { get; set; }
    public int? TransporterId { get; set; }
    public int? OriginId { get; set; }
    public int? DestinationId { get; set; }
    public int? WeighBridgeId { get; set; }
    public int? OperatorId { get; set; }
    public WeighbridgeTransactionStatus? Status { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? WeighMode { get; set; }
    
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    
    // Sorting
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}