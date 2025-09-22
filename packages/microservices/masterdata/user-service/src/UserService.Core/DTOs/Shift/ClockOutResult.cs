namespace UserService.Core.DTOs.Shift;

public class ClockOutResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShiftAttendanceResponse? Attendance { get; set; }
    public bool IsEarlyDeparture { get; set; }
    public TimeSpan? TotalHoursWorked { get; set; }
}
