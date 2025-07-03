using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeMaintenanceDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    public MaintenancePriority Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? MaintenanceCompany { get; set; }
    public decimal EstimatedDuration { get; set; }
    public decimal? ActualDuration { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? PartsUsed { get; set; }
    public string? WorkPerformed { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public bool RequiresCalibration { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ScheduleMaintenanceRequest
{
    public DateTime ScheduledDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? MaintenanceCompany { get; set; }
    public decimal EstimatedDuration { get; set; }
    public decimal? EstimatedCost { get; set; }
    public bool RequiresCalibration { get; set; }
}

public class UpdateMaintenanceRequest
{
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public MaintenanceStatus Status { get; set; }
    public decimal? ActualDuration { get; set; }
    public decimal? ActualCost { get; set; }
    public string? PartsUsed { get; set; }
    public string? WorkPerformed { get; set; }
    public string? Findings { get; set; }
    public string? Recommendations { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? WorkOrderNumber { get; set; }
    public string? InvoiceNumber { get; set; }
}