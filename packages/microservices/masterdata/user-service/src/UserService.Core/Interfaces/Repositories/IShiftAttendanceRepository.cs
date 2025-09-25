using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IShiftAttendanceRepository : IRepository<ShiftAttendance>
    {
        // Existing entity-returning methods
        new Task<IEnumerable<ShiftAttendance>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        
        // New paginated method with attendance summaries
        Task<PaginatedShiftInstancesResponse> GetPaginatedShiftInstancesWithAttendanceAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            DateTime? startDate = null, 
            DateTime? endDate = null);
        
        // Existing method to get shifts with their instances and attendances
        
        Task<ShiftAttendance?> GetByIdAsync(string id);
        Task<ShiftAttendance> CreateAsync(ShiftAttendance attendance);
        Task<ShiftAttendance?> UpdateAsync(ShiftAttendance attendance);
        Task<bool> DeleteAsync(string id);
        Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null);
        Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null);
        Task<ShiftAttendance?> GetByShiftInstanceAndEmployeeAsync(string shiftInstanceId, string employeeId);
        
        // Entity-returning query methods (keep these for write operations)
        
        Task<PagedResult<ShiftAttendance>> GetByInstanceIdAsync(
            string instanceId, 
            int pageNumber = 1, 
            int pageSize = 50, 
            CancellationToken cancellationToken = default);
        Task<ShiftAttendance?> GetByUserAndInstanceAsync(string userId, string shiftInstanceId);
        
        // DTO-returning methods for read operations
        Task<IEnumerable<ShiftAttendanceResponse>> GetShiftAttendancesForInstanceAsync(string shiftInstanceId);
        Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendancesAsync(string employeeId);
        Task<ShiftAttendanceResponse?> GetAttendanceDetailsAsync(string id);
    }
}