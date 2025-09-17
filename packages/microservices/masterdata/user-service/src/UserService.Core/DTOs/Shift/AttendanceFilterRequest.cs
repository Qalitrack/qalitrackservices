using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class AttendanceFilterRequest
{
    public string? EmployeeId { get; set; }
    public string? ShiftId { get; set; }
    public string? ShiftInstanceId { get; set; }
    public AttendanceStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsLate { get; set; }
    public bool? IsEarlyDeparture { get; set; }
        
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}