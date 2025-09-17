using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IShiftAttendanceRepository : IRepository<ShiftAttendance>
    {
        // Existing entity-returning methods
        new Task<IEnumerable<ShiftAttendance>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
        Task<ShiftAttendance?> GetByIdAsync(string id);
        Task<ShiftAttendance> CreateAsync(ShiftAttendance attendance);
        Task<ShiftAttendance?> UpdateAsync(ShiftAttendance attendance);
        Task<bool> DeleteAsync(string id);
        Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null);
        Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null);
        Task<ShiftAttendance?> GetByShiftInstanceAndEmployeeAsync(string shiftInstanceId, string employeeId);
        
        // Entity-returning query methods (keep these for write operations)
        Task<IEnumerable<ShiftAttendance>> GetByShiftInstanceAsync(string shiftInstanceId);
        Task<IEnumerable<ShiftAttendance>> GetByEmployeeAsync(string employeeId);
        Task<ShiftAttendance?> GetByUserAndInstanceAsync(string userId, string shiftInstanceId);
        
        // New DTO-returning methods for read operations
        Task<IEnumerable<ShiftAttendanceResponse>> GetShiftAttendancesForInstanceAsync(string shiftInstanceId);
        Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendancesAsync(string employeeId);
        Task<ShiftAttendanceResponse?> GetAttendanceDetailsAsync(string id);
    }
}
