namespace WeighbridgeService.Core.Entities;

public enum MaintenanceType
{
    Preventive,
    Corrective,
    Emergency,
    Scheduled,
    Inspection
}

public enum MaintenanceStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Overdue
}

public enum MaintenancePriority
{
    Low,
    Medium,
    High,
    Critical
}

public class WeighbridgeMaintenance : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? MaintenanceCompany { get; set; }
    public decimal EstimatedDuration { get; set; } // in hours
    public decimal? ActualDuration { get; set; } // in hours
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? PartsUsed { get; set; } // JSON or comma-separated list
    public string? WorkPerformed { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public bool RequiresCalibration { get; set; } = false;
    public DateTime? NextMaintenanceDate { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Photos { get; set; } // JSON array of file paths
    public string? Documents { get; set; } // JSON array of file paths
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}