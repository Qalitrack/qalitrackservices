using System.ComponentModel.DataAnnotations;
using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class ShiftInstanceGenerateRequest
{
    [Required]
    public string ShiftId { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public int RecurrenceInterval { get; set; } = 1;
    public DayOfWeek[]? CustomDays { get; set; }
    public DateTime[]? ExceptionDates { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}