using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class ShiftResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public ShiftMode Mode { get; set; }
    public bool IsActive { get; set; }
        
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ShiftStatus Status { get; set; }
    public ShiftType Type { get; set; }
    public int RequiredStaffCount { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
    public DayOfWeek[]? CustomDays { get; set; }
    public DateTime[]? ExceptionDates { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
        
    // Statistics (can be populated when needed)
    public int TotalInstances { get; set; } = 0;
    public int AssignedUsers { get; set; } = 0;
}
