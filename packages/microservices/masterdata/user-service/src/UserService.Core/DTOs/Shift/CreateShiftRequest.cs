using System.ComponentModel.DataAnnotations;
using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class CreateShiftRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
        
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
        
    [Required]
    public TimeSpan StartTime { get; set; }
        
    [Required]
    public TimeSpan EndTime { get; set; }
        
    public ShiftMode Mode { get; set; } = ShiftMode.Open;
        
    // Enhanced properties
    private DateTime _startDate = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
    [Required]
    public DateTime StartDate 
    { 
        get => _startDate;
        set => _startDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
        
    private DateTime? _endDate;
    public DateTime? EndDate 
    { 
        get => _endDate;
        set => _endDate = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
    }
        
    public ShiftType Type { get; set; } = ShiftType.Recurring;
        
    [Range(1, 100)]
    public int RequiredStaffCount { get; set; } = 1;
        
  // Recurrence settings
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
        
    [Range(1, 365)]
    public int RecurrenceInterval { get; set; } = 1;
        
    public DayOfWeek[]? CustomDays { get; set; }
    private DateTime[]? _exceptionDates;
    public DateTime[]? ExceptionDates 
    { 
        get => _exceptionDates;
        set => _exceptionDates = value?.Select(d => DateTime.SpecifyKind(d, DateTimeKind.Utc)).ToArray();
    }
        
}