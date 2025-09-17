using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class ShiftAttendanceResponse
{
    public string Id { get; set; } = string.Empty;
    public string ShiftInstanceId { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
        
    public DateTime? ClockInTime { get; set; }
    public DateTime? ClockOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsLate { get; set; }
    public bool IsEarlyDeparture { get; set; }
    public TimeSpan? ActualHoursWorked { get; set; }
        
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}