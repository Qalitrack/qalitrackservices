using Newtonsoft.Json;

namespace TransactionService.Core.Entities;

public class TransactionAudit : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime AuditDate { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? SessionId { get; set; }
    public string? Reason { get; set; }
    public AuditAction AuditAction { get; set; }
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
    
    // Helper properties for JSON data
    public Dictionary<string, object>? OldValues
    {
        get => string.IsNullOrEmpty(OldValuesJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(OldValuesJson);
        set => OldValuesJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
    
    public Dictionary<string, object>? NewValues
    {
        get => string.IsNullOrEmpty(NewValuesJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(NewValuesJson);
        set => NewValuesJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
}

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3,
    StateChange = 4,
    WorkflowAdvance = 5,
    DocumentUpload = 6,
    ChargeApplied = 7,
    Approval = 8,
    Rejection = 9,
    Cancel = 10,
    Complete = 11
}