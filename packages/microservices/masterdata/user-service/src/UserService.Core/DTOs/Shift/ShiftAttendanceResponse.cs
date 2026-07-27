using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift
{
    // Paginated response for shift instances with attendance
    public class PaginatedShiftInstancesResponse
    {
        public List<ShiftInstanceWithAttendanceSummary> ShiftInstances { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    // Shift instance with attendance summary and detailed records
    public class ShiftInstanceWithAttendanceSummary
    {
        public string InstanceId { get; set; } = string.Empty;
        public string ShiftId { get; set; } = string.Empty;
        public string ShiftName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime ScheduledEndTime { get; set; }
        public bool IsCancelled { get; set; }
        public string? CancellationReason { get; set; }
        
        // Attendance summary
        public AttendanceSummary AttendanceSummary { get; set; } = new();
        
        // Detailed attendance records
        public List<ShiftAttendanceResponse> AttendanceRecords { get; set; } = new();
    }

    // Attendance summary statistics
    public class AttendanceSummary
    {
        public int TotalScheduled { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int EarlyDepartureCount { get; set; }
        public double AttendanceRate { get; set; } // Percentage
    }

    public class ShiftAttendanceResponse
    {
        public string Id { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string ShiftInstanceId { get; set; } = string.Empty;
        public string? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
        public bool IsLate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    // Request DTO for filtering
    public class AttendanceFilterRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ShiftId { get; set; }
        public AttendanceStatus? Status { get; set; }
        public bool? IncludeCancelled { get; set; } = false;
    }
}