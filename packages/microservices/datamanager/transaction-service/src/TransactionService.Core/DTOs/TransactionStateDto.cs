namespace TransactionService.Core.DTOs;

public class TransactionStateDto
{
    public Guid Id { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string FromState { get; set; } = string.Empty;
    public string ToState { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public DateTime TransitionDate { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ValidationErrors { get; set; }
    public Dictionary<string, object>? TransitionData { get; set; }
    public string? Reason { get; set; }
}

public class StateTransitionRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Dictionary<string, object>? TransitionData { get; set; }
}

public class StateValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public string? NextState { get; set; }
    public List<string> AvailableTriggers { get; set; } = new();
}