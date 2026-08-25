namespace Transaction.Core.Entities;

public class TransactionAuditLog : BaseEntity
{
    public string WeighbridgeTransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Created", "Updated", "Completed", "ReweighRequested"
    public string ChangedBy { get; set; } = string.Empty;
    public string ChangedFields { get; set; } = string.Empty; // JSON of changed fields
    public string OldValues { get; set; } = string.Empty; // JSON of old values
    public string NewValues { get; set; } = string.Empty; // JSON of new values
    public DateTime ChangeTimestamp { get; set; }
    public string Reason { get; set; } = string.Empty;
}