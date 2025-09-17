using UserService.Core.DTOs.Shift;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services;

public class ShiftReportingService:IShiftReportingService
{
    public Task<ShiftCoverageReport> GetShiftCoverageAsync(string shiftInstanceId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ShiftCoverageReport>> GetDailyCoverageReportAsync(DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ShiftCoverageReport>> GetCoverageReportAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeAttendanceReport> GetEmployeeAttendanceReportAsync(string employeeId, DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<EmployeeAttendanceReport>> GetTeamAttendanceReportAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<object> GetAttendanceSummaryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }

    public Task<object> GetShiftUtilizationSummaryAsync(DateTime startDate, DateTime endDate)
    {
        throw new NotImplementedException();
    }
}