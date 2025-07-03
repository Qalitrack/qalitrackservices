namespace WeighbridgeService.Core.Entities;

public enum OperatorStatus
{
    Active,
    Inactive,
    Training,
    Suspended
}

public class WeighbridgeOperator : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty; // Reference to user/employee ID
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EmployeeNumber { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? UnassignedDate { get; set; }
    public OperatorStatus Status { get; set; } = OperatorStatus.Active;
    public DateTime? CertificationDate { get; set; }
    public DateTime? CertificationExpiry { get; set; }
    public string? CertificationNumber { get; set; }
    public string? TrainingLevel { get; set; } // Basic, Intermediate, Advanced
    public DateTime? LastTrainingDate { get; set; }
    public DateTime? NextTrainingDate { get; set; }
    public string? Shift { get; set; } // Morning, Afternoon, Night, Rotating
    public bool CanOperateAlone { get; set; } = false;
    public bool CanCalibrate { get; set; } = false;
    public bool CanPerformMaintenance { get; set; } = false;
    public string? AccessLevel { get; set; } // Operator, Supervisor, Admin
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
}

public class WeighbridgeSchedule : BaseEntity
{
    public string WeighbridgeId { get; set; } = string.Empty;
    public string OperatorId { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ShiftType { get; set; } = string.Empty; // Morning, Afternoon, Night
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Weighbridge Weighbridge { get; set; } = null!;
    public virtual WeighbridgeOperator Operator { get; set; } = null!;
}