using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class OperationalSchedule : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string ScheduleId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string WeighbridgeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public ScheduleType Type { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    [Required]
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Scheduled;
    
    [Required]
    public SchedulePriority Priority { get; set; } = SchedulePriority.Normal;
    
    [MaxLength(100)]
    public string? AssignedTo { get; set; }
    
    [MaxLength(100)]
    public string? VehicleId { get; set; }
    
    [MaxLength(100)]
    public string? TransactionId { get; set; }
    
    public List<string> RequiredResources { get; set; } = new();
    
    public List<string> Dependencies { get; set; } = new();
    
    public bool IsRecurring { get; set; } = false;
    
    public RecurrencePattern? RecurrencePattern { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    [MaxLength(100)]
    public string? CompletedBy { get; set; }
    
    public TimeSpan? ActualDuration { get; set; }
    
    public List<ScheduleAlert> Alerts { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class RecurrencePattern
{
    [Required]
    public RecurrenceType Type { get; set; }
    
    public int Interval { get; set; } = 1;
    
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    
    public int? DayOfMonth { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    public int? MaxOccurrences { get; set; }
}

public class ScheduleAlert
{
    [Required]
    [MaxLength(100)]
    public required string Type { get; set; }
    
    [Required]
    public TimeSpan TriggerBefore { get; set; }
    
    [Required]
    [MaxLength(500)]
    public required string Message { get; set; }
    
    public List<string> Recipients { get; set; } = new();
    
    public bool IsActive { get; set; } = true;
    
    public DateTime? LastTriggered { get; set; }
}

public enum ScheduleType
{
    Transaction,
    Maintenance,
    Inspection,
    Calibration,
    Training,
    Meeting,
    Other
}

public enum ScheduleStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed,
    Failed
}

public enum SchedulePriority
{
    Low,
    Normal,
    High,
    Critical,
    Emergency
}

public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}