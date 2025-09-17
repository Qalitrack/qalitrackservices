using UserService.Core.DTOs.Shift;

namespace UserService.Core.Interfaces.Services;

public interface IShiftReportingService
{
    // Coverage reports
    Task<ShiftCoverageReport> GetShiftCoverageAsync(string shiftInstanceId);
    Task<IEnumerable<ShiftCoverageReport>> GetDailyCoverageReportAsync(DateTime date);
    Task<IEnumerable<ShiftCoverageReport>> GetCoverageReportAsync(DateTime startDate, DateTime endDate);

    // Attendance reports
    Task<EmployeeAttendanceReport> GetEmployeeAttendanceReportAsync(string employeeId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<EmployeeAttendanceReport>> GetTeamAttendanceReportAsync(DateTime startDate, DateTime endDate);
        
    // Summary reports
    Task<object> GetAttendanceSummaryAsync(DateTime startDate, DateTime endDate);
    Task<object> GetShiftUtilizationSummaryAsync(DateTime startDate, DateTime endDate);
}