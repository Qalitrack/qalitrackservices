using System.ComponentModel.DataAnnotations;

namespace OperationalDataService.Core.Entities;

public class MaintenanceSchedule : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public required string MaintenanceId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string WeighbridgeId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string OrganizationId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public MaintenanceType Type { get; set; }
    
    [Required]
    public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Scheduled;
    
    [Required]
    public MaintenancePriority Priority { get; set; } = MaintenancePriority.Normal;
    
    [Required]
    public DateTime ScheduledDate { get; set; }
    
    [Required]
    public TimeSpan EstimatedDuration { get; set; }
    
    public DateTime? ActualStartDate { get; set; }
    
    public DateTime? ActualEndDate { get; set; }
    
    [MaxLength(100)]
    public string? AssignedTechnician { get; set; }
    
    [MaxLength(100)]
    public string? SupervisorId { get; set; }
    
    public List<MaintenanceTask> Tasks { get; set; } = new();
    
    public List<MaintenanceResource> RequiredResources { get; set; } = new();
    
    public List<string> RequiredParts { get; set; } = new();
    
    public decimal EstimatedCost { get; set; } = 0;
    
    public decimal ActualCost { get; set; } = 0;
    
    [MaxLength(1000)]
    public string? PreMaintenanceNotes { get; set; }
    
    [MaxLength(1000)]
    public string? PostMaintenanceNotes { get; set; }
    
    public bool IsRecurring { get; set; } = false;
    
    public MaintenanceRecurrence? Recurrence { get; set; }
    
    public DateTime? LastPerformed { get; set; }
    
    public DateTime? NextDueDate { get; set; }
    
    public List<MaintenanceAlert> Alerts { get; set; } = new();
    
    public MaintenanceResult? Result { get; set; }
    
    public List<string> DocumentPaths { get; set; } = new();
    
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MaintenanceTask
{
    [Required]
    [MaxLength(100)]
    public required string TaskId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    
    [Required]
    public TimeSpan EstimatedDuration { get; set; }
    
    public TimeSpan? ActualDuration { get; set; }
    
    [MaxLength(100)]
    public string? AssignedTo { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    public int Sequence { get; set; } = 0;
    
    public bool IsRequired { get; set; } = true;
    
    public List<string> Dependencies { get; set; } = new();
}

public class MaintenanceResource
{
    [Required]
    [MaxLength(100)]
    public required string ResourceId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }
    
    [Required]
    public ResourceType Type { get; set; }
    
    [Required]
    public int Quantity { get; set; }
    
    [MaxLength(50)]
    public string? Unit { get; set; }
    
    public decimal? CostPerUnit { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class MaintenanceRecurrence
{
    [Required]
    public RecurrenceType Type { get; set; }
    
    public int Interval { get; set; } = 1;
    
    public int? DayOfMonth { get; set; }
    
    public List<DayOfWeek> DaysOfWeek { get; set; } = new();
    
    public DateTime? EndDate { get; set; }
    
    public int? MaxOccurrences { get; set; }
    
    public bool IsActive { get; set; } = true;
}

public class MaintenanceAlert
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

public class MaintenanceResult
{
    [Required]
    public ResultStatus Status { get; set; }
    
    [MaxLength(1000)]
    public string? Summary { get; set; }
    
    public List<string> IssuesFound { get; set; } = new();
    
    public List<string> IssuesResolved { get; set; } = new();
    
    public List<string> Recommendations { get; set; } = new();
    
    public DateTime? NextRecommendedMaintenance { get; set; }
    
    public List<string> PartsReplaced { get; set; } = new();
    
    public decimal QualityScore { get; set; } = 0;
    
    [MaxLength(100)]
    public string? CompletedBy { get; set; }
    
    [MaxLength(100)]
    public string? VerifiedBy { get; set; }
    
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

public enum MaintenanceType
{
    Preventive,
    Predictive,
    Corrective,
    Emergency,
    Calibration,
    Inspection,
    Cleaning,
    Upgrade
}

public enum MaintenanceStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
    Postponed,
    Failed,
    PartiallyComplete
}

public enum MaintenancePriority
{
    Low,
    Normal,
    High,
    Critical,
    Emergency
}

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    Failed,
    Skipped
}

public enum ResourceType
{
    Tool,
    Equipment,
    Part,
    Material,
    Personnel,
    Other
}

public enum ResultStatus
{
    Success,
    PartialSuccess,
    Failed,
    Cancelled,
    Incomplete
}

public enum RecurrenceType
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly,
    Custom
}