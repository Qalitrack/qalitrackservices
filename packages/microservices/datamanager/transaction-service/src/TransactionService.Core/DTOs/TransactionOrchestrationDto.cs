namespace TransactionService.Core.DTOs;

public class TransactionOrchestrationDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string CurrentState { get; set; } = string.Empty;
    public List<WorkflowStepDto> WorkflowSteps { get; set; } = new();
    public List<TransactionStateDto> StateHistory { get; set; } = new();
    public List<ServiceIntegrationDto> ServiceIntegrations { get; set; } = new();
    public bool RequiresApproval { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int Priority { get; set; }
    public string? ExternalReferenceId { get; set; }
}

public class WorkflowStepDto
{
    public string StepName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? ValidationData { get; set; }
}

public class ServiceIntegrationDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastCalled { get; set; }
    public string? Response { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

public class ServiceCoordinationRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public List<string> ServiceNames { get; set; } = new();
    public Dictionary<string, object>? CoordinationData { get; set; }
    public bool RequireAllSuccess { get; set; } = true;
}