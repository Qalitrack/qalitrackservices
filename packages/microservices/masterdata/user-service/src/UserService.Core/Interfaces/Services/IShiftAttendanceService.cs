using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Services
{
    public interface IShiftAttendanceService
    {
        /// <summary>
        /// Gets all shift attendances with pagination support
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page (max 100)</param>
        /// <returns>Paginated list of shift attendance responses</returns>
        Task<IEnumerable<ShiftAttendanceResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 20);
        Task<ShiftAttendance?> GetByIdAsync(string id);
        Task<bool> DeleteAsync(string id);
        Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null);
        Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null);
        Task<IEnumerable<ShiftAttendanceResponse>> GetAttendanceByShiftInstanceAsync(string shiftInstanceId);
        Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendanceAsync(string employeeId);
        Task<ShiftAttendance?> GetAttendanceByUserAndInstanceAsync(string userId, string shiftInstanceId);
    }
}