using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Core.DTOs.Common;
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

        /// <summary>
        /// Gets paginated shift instances with attendance summaries and detailed records
        /// </summary>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page (max 100)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Paginated response with shift instances, attendance summaries, and records</returns>
        Task<PaginatedShiftInstancesResponse> GetPaginatedShiftInstancesWithAttendanceAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            DateTime? startDate = null, 
            DateTime? endDate = null);

   
        /// <summary>
        /// Gets a shift attendance by ID
        /// </summary>
        /// <param name="id">Attendance ID</param>
        /// <returns>Shift attendance entity or null</returns>
        Task<ShiftAttendance?> GetByIdAsync(string id);

        /// <summary>
        /// Gets detailed attendance information by ID
        /// </summary>
        /// <param name="id">Attendance ID</param>
        /// <returns>Detailed attendance response or null</returns>
        Task<ShiftAttendanceResponse?> GetAttendanceDetailsAsync(string id);

        /// <summary>
        /// Creates a new attendance record
        /// </summary>
        /// <param name="attendance">Attendance to create</param>
        /// <returns>Created attendance record</returns>
        Task<ShiftAttendance> CreateAsync(ShiftAttendance attendance);

        /// <summary>
        /// Updates an existing attendance record
        /// </summary>
        /// <param name="attendance">Attendance to update</param>
        /// <returns>Updated attendance record or null if not found</returns>
        Task<ShiftAttendance?> UpdateAsync(ShiftAttendance attendance);

        /// <summary>
        /// Gets attendance details for a specific shift instance
        /// </summary>
        /// <param name="instanceId">The ID of the shift instance</param>
        /// <returns>Shift attendance response or null if not found</returns>
        Task<PagedResult<ShiftAttendanceResponse>> GetShiftInstanceAttendanceAsync(
            Guid instanceId, 
            int pageNumber = 1, 
            int pageSize = 50, 
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Soft deletes an attendance record
        /// </summary>
        /// <param name="id">Attendance ID</param>
        /// <returns>True if deleted successfully</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Clock in an employee for a shift instance
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="clockInTime">Clock in time</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>Updated attendance record or null</returns>
        Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null);

        /// <summary>
        /// Clock out an employee for a shift instance
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <param name="clockOutTime">Clock out time</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>Updated attendance record or null</returns>
        Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null);

        /// <summary>
        /// Gets attendance records for a specific shift instance
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <returns>List of attendance responses</returns>
        Task<IEnumerable<ShiftAttendanceResponse>> GetAttendanceByShiftInstanceAsync(string shiftInstanceId);

        /// <summary>
        /// Gets attendance records for a specific employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>List of attendance responses</returns>
        Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendanceAsync(string employeeId);

        /// <summary>
        /// Gets attendance record for a specific user and shift instance
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <returns>Attendance record or null</returns>
        Task<ShiftAttendance?> GetAttendanceByUserAndInstanceAsync(string userId, string shiftInstanceId);

        /// <summary>
        /// Gets attendance record by shift instance and employee
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <param name="employeeId">Employee ID</param>
        /// <returns>Attendance record or null</returns>
        Task<ShiftAttendance?> GetByShiftInstanceAndEmployeeAsync(string shiftInstanceId, string employeeId);
    }
}