using TransactionService.Core.Entities;

namespace TransactionService.Core.DTOs;

public class CreateTransactionRequest
{
    public TransactionType TransactionType { get; set; }
    public string VehicleId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    
    public string? DeliveryNoteNumber { get; set; }
    public string? PermitNumber { get; set; }
    public string? Remarks { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}