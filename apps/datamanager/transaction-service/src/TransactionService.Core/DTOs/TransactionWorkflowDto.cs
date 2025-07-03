using TransactionService.Core.Entities;

namespace TransactionService.Core.DTOs;

public class TransactionWorkflowDto
{
    public string Id { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public WorkflowStep WorkflowStep { get; set; }
    public StepStatus Status { get; set; }
    public int Order { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? ValidationData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}