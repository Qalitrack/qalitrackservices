using Newtonsoft.Json;

namespace TransactionService.Core.Entities;

public class TransactionState : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public DateTime TransitionDate { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public bool IsValid { get; set; } = true;
    public string? ValidationErrors { get; set; }
    public string? TransitionDataJson { get; set; }
    public string? Reason { get; set; }
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
    
    // Helper property for transition data
    public Dictionary<string, object>? TransitionData
    {
        get => string.IsNullOrEmpty(TransitionDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(TransitionDataJson);
        set => TransitionDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
}

public static class TransactionStates
{
    public const string Pending = "Pending";
    public const string InProgress = "InProgress";
    public const string Weighing = "Weighing";
    public const string Documentation = "Documentation";
    public const string Approval = "Approval";
    public const string Charging = "Charging";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";
    public const string Failed = "Failed";
}

public static class TransactionTriggers
{
    public const string Start = "Start";
    public const string BeginWeighing = "BeginWeighing";
    public const string CompleteWeighing = "CompleteWeighing";
    public const string SubmitDocuments = "SubmitDocuments";
    public const string Approve = "Approve";
    public const string Reject = "Reject";
    public const string ApplyCharges = "ApplyCharges";
    public const string Complete = "Complete";
    public const string Cancel = "Cancel";
    public const string Fail = "Fail";
}