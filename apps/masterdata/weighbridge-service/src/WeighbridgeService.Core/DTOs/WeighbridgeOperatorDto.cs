using WeighbridgeService.Core.Entities;

namespace WeighbridgeService.Core.DTOs;

public class WeighbridgeOperatorDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public OperatorStatus Status { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string? CertificationNumber { get; set; }
    public string? TrainingLevel { get; set; }
    public DateTime? LastTrainingDate { get; set; }
    public DateTime? NextTrainingDate { get; set; }
    public string? Shift { get; set; }
    public bool CanOperateAlone { get; set; }
    public bool CanCalibrate { get; set; }
    public bool CanPerformMaintenance { get; set; }
    public string? AccessLevel { get; set; }
    public string? Notes { get; set; }
}

public class AssignOperatorRequest
{
    public string OperatorId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string? CertificationNumber { get; set; }
    public string? TrainingLevel { get; set; }
    public string? Shift { get; set; }
    public bool CanOperateAlone { get; set; }
    public bool CanCalibrate { get; set; }
    public bool CanPerformMaintenance { get; set; }
    public string? AccessLevel { get; set; }
    public string? Notes { get; set; }
}

public class WeighbridgeScheduleDto
{
    public string Id { get; set; } = string.Empty;
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ShiftType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public WeighbridgeOperatorDto? Operator { get; set; }
}