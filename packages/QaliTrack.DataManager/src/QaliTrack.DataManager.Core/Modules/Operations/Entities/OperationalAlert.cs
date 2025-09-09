using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Core.Modules.Operations.Entities;

public class OperationalAlert : TenantEntity
{
    public string AlertType { get; set; } = string.Empty; // System, Process, Compliance, Performance
    public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Message { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // Transaction, Vehicle, Driver, Weighbridge
    public Guid? EntityId { get; set; }
    public DateTime AlertTime { get; set; }
    public string Status { get; set; } = "Open"; // Open, Acknowledged, Resolved, Closed
    public Guid? AssignedTo { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? AlertSource { get; set; }
    public string? AdditionalData { get; set; } // JSON additional context
    public int EscalationLevel { get; set; } = 0;
    public DateTime? NextEscalationAt { get; set; }
}

public class MaintenanceSchedule : TenantEntity
{
    public Guid? WeighbridgeId { get; set; }
    public Guid? VehicleId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty; // Preventive, Corrective, Calibration
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed, Cancelled
    public string? Notes { get; set; }
    public string? MaintenanceProvider { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? Currency { get; set; }
    public TimeSpan? EstimatedDuration { get; set; }
    public TimeSpan? ActualDuration { get; set; }
    public int Priority { get; set; } = 1;
    public string? WorkOrderNumber { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    
    // Navigation properties
    public virtual ICollection<MaintenanceTask> Tasks { get; set; } = new List<MaintenanceTask>();
}

public class MaintenanceTask : TenantEntity
{
    public Guid MaintenanceScheduleId { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Failed
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? AssignedTo { get; set; }
    public string? CompletionNotes { get; set; }
    public string? PartsUsed { get; set; }
    public decimal? TaskCost { get; set; }
    public int Order { get; set; }
    
    // Navigation properties
    public virtual MaintenanceSchedule MaintenanceSchedule { get; set; } = null!;
}

public class ProcessDefinition : TenantEntity
{
    public string ProcessName { get; set; } = string.Empty;
    public string ProcessType { get; set; } = string.Empty; // Business, Technical, Operational
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Status { get; set; } = "Active"; // Active, Inactive, Draft
    public string ProcessDefinitionData { get; set; } = string.Empty; // JSON/XML process definition
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ProcessOwner { get; set; }
    public int MaxExecutionTimeMinutes { get; set; } = 60;
    public bool RequiresApproval { get; set; } = false;
    public string? ApprovalMatrix { get; set; } // JSON approval configuration
}

public class WorkflowExecution : TenantEntity
{
    public Guid ProcessDefinitionId { get; set; }
    public string ExecutionId { get; set; } = string.Empty; // Unique execution identifier
    public Guid? TransactionId { get; set; } // If workflow is transaction-related
    public string Status { get; set; } = "Running"; // Running, Completed, Failed, Suspended
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ExecutionData { get; set; } // JSON execution context
    public Guid? InitiatedBy { get; set; }
    public int CurrentStepIndex { get; set; } = 0;
    public string? CurrentStepName { get; set; }
    public decimal PercentageComplete { get; set; } = 0;
    
    // Navigation properties
    public virtual ProcessDefinition ProcessDefinition { get; set; } = null!;
}