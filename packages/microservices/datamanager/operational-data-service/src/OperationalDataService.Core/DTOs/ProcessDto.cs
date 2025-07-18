using OperationalDataService.Core.Entities;

namespace OperationalDataService.Core.DTOs;

public class ProcessDto
{
    public string Id { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public string OperationalId { get; set; } = string.Empty;
    public ProcessType ProcessType { get; set; }
    public string? Description { get; set; }
    public ProcessStatus Status { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int StepNumber { get; set; }
    public int TotalSteps { get; set; }
    public string? PreviousProcessId { get; set; }
    public string? NextProcessId { get; set; }
    public decimal ProgressPercentage { get; set; }
    public string? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public TimeSpan? ActualDuration { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Notes { get; set; }
    public bool IsAutomated { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsParallel { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Dictionary<string, object>? WorkflowData { get; set; }
    public List<string>? ValidationRules { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProcessRequest
{
    public string ProcessName { get; set; } = string.Empty;
    public string OperationalId { get; set; } = string.Empty;
    public ProcessType ProcessType { get; set; }
    public string? Description { get; set; }
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 1;
    public string? PreviousProcessId { get; set; }
    public string? NextProcessId { get; set; }
    public string? AssignedUserId { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public bool IsAutomated { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public bool IsParallel { get; set; } = false;
    public Dictionary<string, object>? WorkflowData { get; set; }
    public List<string>? ValidationRules { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}

public class UpdateProcessRequest
{
    public string? ProcessName { get; set; }
    public string? Description { get; set; }
    public string? AssignedUserId { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public decimal? ProgressPercentage { get; set; }
    public string? Notes { get; set; }
    public Dictionary<string, object>? WorkflowData { get; set; }
    public List<string>? ValidationRules { get; set; }
    public Dictionary<string, object>? Configuration { get; set; }
}

public class ProcessProgressDto
{
    public string ProcessId { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public ProcessStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public int StepNumber { get; set; }
    public int TotalSteps { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public TimeSpan? ElapsedTime { get; set; }
    public TimeSpan? RemainingTime { get; set; }
}

public class OperationProgressDto
{
    public string OperationId { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public OperationStatus Status { get; set; }
    public decimal OverallProgress { get; set; }
    public int TotalProcesses { get; set; }
    public int CompletedProcesses { get; set; }
    public int InProgressProcesses { get; set; }
    public int PendingProcesses { get; set; }
    public int FailedProcesses { get; set; }
    public List<ProcessProgressDto> ProcessProgress { get; set; } = new List<ProcessProgressDto>();
}