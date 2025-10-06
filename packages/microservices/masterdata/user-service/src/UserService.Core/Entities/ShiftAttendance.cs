using System.ComponentModel.DataAnnotations;
using UserService.Core.Enums;

namespace UserService.Core.Entities;

public class ShiftAttendance : BaseEntity
{
    [Required]
    public string ShiftInstanceId { get; set; } = string.Empty;
        
    [Required]
    public string EmployeeId { get; set; } = string.Empty;
        
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }
    public bool IsLate { get; set; } = false;
    public bool IsEarlyDeparture { get; set; } = false;
    public TimeSpan? ActualHoursWorked { get; set; }
        
    // Navigation properties
    public virtual ShiftInstance ShiftInstance { get; set; } = null!;
    public virtual User Employee { get; set; } = null!;
}
