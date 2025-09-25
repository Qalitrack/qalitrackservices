using System.ComponentModel.DataAnnotations;
using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class UpdateShiftRequest
{
    [Required]
    public string Id { get; set; } = string.Empty;
        
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
        
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
        
    [Required]
    public TimeSpan StartTime { get; set; }
        
    [Required]
    public TimeSpan EndTime { get; set; }
        
    public ShiftMode Mode { get; set; }
        
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public int RequiredStaffCount { get; set; }
        
    public RecurrenceType RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
    public DayOfWeek[]? CustomDays { get; set; }
    public DateTime[]? ExceptionDates { get; set; }
}
