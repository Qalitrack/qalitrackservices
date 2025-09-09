namespace QaliTrack.DataManager.Core.Modules.Transactions.DTOs;

public class WeighingTransactionDto
{
    public Guid Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Guid RouteId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? TareWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public DateTime? EntryWeighingTime { get; set; }
    public DateTime? ExitWeighingTime { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public int Priority { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWeighingTransactionDto
{
    public string TransactionType { get; set; } = string.Empty;
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public Guid RouteId { get; set; }
    public Guid WeighbridgeId { get; set; }
    public int Priority { get; set; } = 1;
    public decimal? UnitPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? PaymentTerms { get; set; }
    public string? DeliveryTerms { get; set; }
    public string? SpecialInstructions { get; set; }
    public Guid OrganizationId { get; set; }
}

public class UpdateTransactionWeightDto
{
    public decimal Weight { get; set; }
    public string MeasurementType { get; set; } = string.Empty; // Entry, Exit
    public string? Notes { get; set; }
}

public class TransactionWorkflowDto
{
    public Guid Id { get; set; }
    public Guid TransactionId { get; set; }
    public string WorkflowName { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int StepOrder { get; set; }
    public bool RequiresManualApproval { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string StepType { get; set; } = string.Empty;
    public int Order { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ExecutedAt { get; set; }
    public string? ExecutionResult { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? ExecutedBy { get; set; }
    public TimeSpan? Duration { get; set; }
}