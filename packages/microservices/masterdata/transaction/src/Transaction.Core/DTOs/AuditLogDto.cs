namespace Transaction.Core.DTOs;

public class AuditLogDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeTransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangedFields { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty;
    public string NewValues { get; set; } = string.Empty;
    public DateTime ChangeTimestamp { get; set; }
    public string Reason { get; set; } = string.Empty;
}