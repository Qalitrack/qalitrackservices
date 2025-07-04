using TransactionService.Core.Entities;

namespace TransactionService.Core.DTOs;

public class UpdateTransactionRequest
{
    public TransactionStatus? Status { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? EntryWeighingTime { get; set; }
    public DateTime? ExitWeighingTime { get; set; }
    public string? DeliveryNoteNumber { get; set; }
    public string? PermitNumber { get; set; }
    public string? Remarks { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}