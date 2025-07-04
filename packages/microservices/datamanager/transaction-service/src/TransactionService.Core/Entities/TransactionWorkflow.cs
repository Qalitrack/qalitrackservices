using Newtonsoft.Json;

namespace TransactionService.Core.Entities;

public class TransactionWorkflow : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public WorkflowStep WorkflowStep { get; set; }
    public StepStatus Status { get; set; } = StepStatus.NotStarted;
    public int Order { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public string? ValidationDataJson { get; set; }
    
    // Navigation Properties
    public virtual WeighingTransaction Transaction { get; set; } = null!;
    
    // Helper property for validation data
    public Dictionary<string, object>? ValidationData
    {
        get => string.IsNullOrEmpty(ValidationDataJson) ? null : JsonConvert.DeserializeObject<Dictionary<string, object>>(ValidationDataJson);
        set => ValidationDataJson = value == null ? null : JsonConvert.SerializeObject(value);
    }
}