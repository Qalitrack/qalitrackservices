using Transaction.Core.Entities;

namespace Transaction.Core.DTOs;

// Filter DTO for paginated queries
public class WeighbridgeTransactionFilter
{
    public string? ReceiptNo { get; set; }
    public string? NoPlate { get; set; }
    public string? DriverName { get; set; }
    public Guid? VehicleId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? TransporterId { get; set; }
    public Guid? OriginId { get; set; }
    public Guid? DestinationId { get; set; }
    public Guid? WeighBridgeId { get; set; }
    public Guid? OperatorId { get; set; }
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