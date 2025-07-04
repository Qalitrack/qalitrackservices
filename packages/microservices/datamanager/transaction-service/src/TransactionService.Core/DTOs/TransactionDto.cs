using TransactionService.Core.Entities;

namespace TransactionService.Core.DTOs;

public class TransactionDto
{
    public string Id { get; set; } = string.Empty;
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime TransactionDate { get; set; }
    
    public string VehicleId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public string SupplierId { get; set; } = string.Empty;
    public string? CustomerId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? EntryWeighingTime { get; set; }
    public DateTime? ExitWeighingTime { get; set; }
    
    public string? DeliveryNoteNumber { get; set; }
    public string? PermitNumber { get; set; }
    public string? Remarks { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public List<TransactionWorkflowDto> WorkflowSteps { get; set; } = new();
    public List<TransactionChargeDto> Charges { get; set; } = new();
    public List<TransactionDocumentDto> Documents { get; set; } = new();
}