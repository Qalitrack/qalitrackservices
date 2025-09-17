namespace UserService.Core.DTOs.Shift;

public class EmployeeAttendanceReport
{
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } = string.Empty;
    public int ScheduledShifts { get; set; }
    public int AttendedShifts { get; set; }
    public int LateArrivals { get; set; }
    public int EarlyDepartures { get; set; }
    public TimeSpan TotalHoursWorked { get; set; }
    public decimal AttendanceRate { get; set; }
        
    public DateTime ReportStartDate { get; set; }
    public DateTime ReportEndDate { get; set; }
}