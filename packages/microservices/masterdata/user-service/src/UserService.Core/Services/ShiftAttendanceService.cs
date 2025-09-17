using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class ShiftAttendanceService : IShiftAttendanceService
    {
        private readonly IShiftAttendanceRepository _shiftAttendanceRepository;
        private readonly IShiftInstanceRepository _shiftInstanceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;
        private readonly ILogger<ShiftAttendanceService> _logger;

        public ShiftAttendanceService(
            IShiftAttendanceRepository shiftAttendanceRepository,
            IShiftInstanceRepository shiftInstanceRepository,
            IUserRepository userRepository,
            ICacheService cacheService,
            IMapper mapper,
            ILogger<ShiftAttendanceService> logger)
        {
            _shiftAttendanceRepository = shiftAttendanceRepository;
            _shiftInstanceRepository = shiftInstanceRepository;
            _userRepository = userRepository;
            _cacheService = cacheService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<ShiftAttendanceResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("Retrieving shift attendances - Page {PageNumber}, Size {PageSize}", pageNumber, pageSize);
                
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100); // Limit page size to 100 for performance
                
                var attendances = await _shiftAttendanceRepository.GetAllAsync(pageNumber, pageSize);
                var response = _mapper.Map<IEnumerable<ShiftAttendanceResponse>>(attendances);
                
                _logger.LogInformation("Retrieved {Count} shift attendances for page {PageNumber}", response.Count(), pageNumber);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift attendances for page {PageNumber}", pageNumber);
                throw;
            }
        }

        public async Task<ShiftAttendance?> GetByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("GetByIdAsync called with null or empty id");
                    return null;
                }

                _logger.LogInformation("Retrieving attendance with id {AttendanceId}", id);

                // Get the entity directly from the repository
                var attendance = await _shiftAttendanceRepository.GetByIdAsync(id);
                if (attendance == null || attendance.IsDeleted)
                {
                    _logger.LogWarning("Shift attendance {AttendanceId} not found or is deleted", id);
                    return null;
                }

                _logger.LogInformation("Retrieved shift attendance {AttendanceId}", id);
                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift attendance {AttendanceId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("DeleteAsync called with null or empty id");
                    return false;
                }

                _logger.LogInformation("Deleting shift attendance {AttendanceId}", id);
                
                var success = await _shiftAttendanceRepository.DeleteAsync(id);
                
                if (success)
                {
                    // Invalidate cache
                    await _cacheService.RemoveAsync($"shift_attendance:{id}");
                    await _cacheService.RemovePatternAsync("shift_attendances:*");
                    await _cacheService.SetAsync("shift_attendances:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(15));
                    
                    _logger.LogInformation("Successfully deleted shift attendance {AttendanceId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete shift attendance {AttendanceId}", id);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift attendance {AttendanceId}", id);
                throw;
            }
        }

        public async Task<ShiftAttendance?> ClockInAsync(string shiftInstanceId, string employeeId, DateTime clockInTime, string? notes = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shiftInstanceId))
                {
                    _logger.LogWarning("ClockInAsync called with null or empty shiftInstanceId");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    _logger.LogWarning("ClockInAsync called with null or empty employeeId");
                    return null;
                }

                _logger.LogInformation("Employee {EmployeeId} clocking in for shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);

                // Validate shift instance exists and is not deleted
                var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(shiftInstanceId);
                if (shiftInstance == null)
                {
                    _logger.LogWarning("Shift instance {ShiftInstanceId} not found for clock in", shiftInstanceId);
                    return null;
                }

                // Validate employee exists and is not deleted
                var employee = await _userRepository.GetByIdAsync(employeeId, true);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found or is deleted", employeeId);
                    return null;
                }

                // Check if shift instance is in a valid state for clock in
                if (shiftInstance.Status == ShiftInstanceStatus.Cancelled)
                {
                    _logger.LogWarning("Cannot clock in to cancelled shift instance {ShiftInstanceId}", shiftInstanceId);
                    return null;
                }

                if (shiftInstance.Status == ShiftInstanceStatus.Completed)
                {
                    _logger.LogWarning("Cannot clock in to completed shift instance {ShiftInstanceId}", shiftInstanceId);
                    return null;
                }

                // Check if employee is already clocked in for this shift
                var existingAttendance = await _shiftAttendanceRepository.GetByShiftInstanceAndEmployeeAsync(shiftInstanceId, employeeId);
                if (existingAttendance != null && existingAttendance.ClockInTime.HasValue && !existingAttendance.ClockOutTime.HasValue)
                {
                    _logger.LogWarning("Employee {EmployeeId} is already clocked in for shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                    return existingAttendance;
                }

                // Create or update attendance record
                var attendance = existingAttendance ?? new ShiftAttendance
                {
                    ShiftInstanceId = shiftInstanceId,
                    EmployeeId = employeeId
                };

                attendance.ClockInTime = clockInTime;
                attendance.Status = AttendanceStatus.Present;
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    attendance.Notes = string.IsNullOrEmpty(attendance.Notes) ? notes : $"{attendance.Notes}; {notes}";
                }

                ShiftAttendance result;
                if (existingAttendance == null)
                {
                    result = await _shiftAttendanceRepository.CreateAsync(attendance);
                }
                else
                {
                    result = await _shiftAttendanceRepository.UpdateAsync(attendance) ?? attendance;
                }

                // Update shift instance status if needed
                if (shiftInstance.Status == ShiftInstanceStatus.Scheduled)
                {
                    shiftInstance.Status = ShiftInstanceStatus.InProgress;
                    await _shiftInstanceRepository.UpdateAsync(shiftInstance);
                }

                // Invalidate relevant caches
                await InvalidateAttendanceCaches(shiftInstanceId, employeeId);

                _logger.LogInformation("Employee {EmployeeId} successfully clocked in for shift instance {ShiftInstanceId} at {ClockInTime}", 
                    employeeId, shiftInstanceId, clockInTime);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clock in for employee {EmployeeId} and shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                throw;
            }
        }

        public async Task<ShiftAttendance?> ClockOutAsync(string shiftInstanceId, string employeeId, DateTime clockOutTime, string? notes = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shiftInstanceId))
                {
                    _logger.LogWarning("ClockOutAsync called with null or empty shiftInstanceId");
                    return null;
                }

                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    _logger.LogWarning("ClockOutAsync called with null or empty employeeId");
                    return null;
                }

                _logger.LogInformation("Employee {EmployeeId} clocking out for shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);

                // Get existing attendance record
                var attendance = await _shiftAttendanceRepository.GetByShiftInstanceAndEmployeeAsync(shiftInstanceId, employeeId);
                if (attendance == null)
                {
                    _logger.LogWarning("No attendance record found for employee {EmployeeId} and shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                    return null;
                }

                // Check if employee is already clocked out
                if (attendance.ClockOutTime.HasValue)
                {
                    _logger.LogWarning("Employee {EmployeeId} is already clocked out for shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                    return attendance;
                }

                // Check if employee is clocked in
                if (!attendance.ClockInTime.HasValue)
                {
                    _logger.LogWarning("Cannot clock out employee {EmployeeId} who has not clocked in for shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                    return null;
                }

                // Validate clock out time is after clock in time
                if (clockOutTime < attendance.ClockInTime.Value)
                {
                    _logger.LogWarning("Clock out time {ClockOutTime} is before clock in time {ClockInTime} for employee {EmployeeId}", 
                        clockOutTime, attendance.ClockInTime.Value, employeeId);
                    return null;
                }

                // Update attendance record
                attendance.ClockOutTime = clockOutTime;
                attendance.ActualHoursWorked = (clockOutTime - attendance.ClockInTime.Value);
                if (!string.IsNullOrWhiteSpace(notes))
                {
                    attendance.Notes = string.IsNullOrEmpty(attendance.Notes) ? notes : $"{attendance.Notes}; {notes}";
                }

                var result = await _shiftAttendanceRepository.UpdateAsync(attendance);

                // Check if all employees have clocked out and update shift instance status
                var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(shiftInstanceId);
                if (shiftInstance != null && shiftInstance.Status == ShiftInstanceStatus.InProgress)
                {
                    var allAttendances = await _shiftAttendanceRepository.GetByShiftInstanceAsync(shiftInstanceId);
                    var allClockedOut = allAttendances.All(a => a.ClockOutTime.HasValue || a.Status == AttendanceStatus.Absent);
                    
                    if (allClockedOut)
                    {
                        shiftInstance.Status = ShiftInstanceStatus.Completed;
                        await _shiftInstanceRepository.UpdateAsync(shiftInstance);
                    }
                }

                // Invalidate relevant caches
                await InvalidateAttendanceCaches(shiftInstanceId, employeeId);

                _logger.LogInformation("Employee {EmployeeId} successfully clocked out for shift instance {ShiftInstanceId} at {ClockOutTime}. Hours worked: {HoursWorked:F2}", 
                    employeeId, shiftInstanceId, clockOutTime, attendance.ActualHoursWorked);

                return result ?? attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clock out for employee {EmployeeId} and shift instance {ShiftInstanceId}", employeeId, shiftInstanceId);
                throw;
            }
        }

        public async Task<IEnumerable<ShiftAttendanceResponse>> GetAttendanceByShiftInstanceAsync(string shiftInstanceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shiftInstanceId))
                {
                    _logger.LogWarning("GetAttendanceByShiftInstanceAsync called with null or empty shiftInstanceId");
                    return Enumerable.Empty<ShiftAttendanceResponse>();
                }

                _logger.LogInformation("Retrieving attendance for shift instance {ShiftInstanceId}", shiftInstanceId);

                // Try cache first
                var cacheKey = $"shift_instance_attendance:{shiftInstanceId}";
                var cachedAttendances = await _cacheService.GetAsync<IEnumerable<ShiftAttendanceResponse>>(cacheKey);
                if (cachedAttendances != null)
                {
                    _logger.LogDebug("Retrieved attendance for shift instance {ShiftInstanceId} from cache", shiftInstanceId);
                    return cachedAttendances;
                }

                // Use the DTO-returning repository method
                var response = await _shiftAttendanceRepository.GetShiftAttendancesForInstanceAsync(shiftInstanceId);

                // Cache for 15 minutes
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

                _logger.LogInformation("Retrieved {Count} attendance records for shift instance {ShiftInstanceId}", 
                    response.Count(), shiftInstanceId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for shift instance {ShiftInstanceId}", shiftInstanceId);
                throw;
            }
        }

        public async Task<IEnumerable<ShiftAttendanceResponse>> GetEmployeeAttendanceAsync(string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(employeeId))
                {
                    _logger.LogWarning("GetEmployeeAttendanceAsync called with null or empty employeeId");
                    return Enumerable.Empty<ShiftAttendanceResponse>();
                }

                _logger.LogInformation("Retrieving attendance for employee {EmployeeId}", employeeId);

                // Try cache first
                var cacheKey = $"employee_attendance:{employeeId}";
                var cachedAttendances = await _cacheService.GetAsync<IEnumerable<ShiftAttendanceResponse>>(cacheKey);
                if (cachedAttendances != null)
                {
                    _logger.LogDebug("Retrieved attendance for employee {EmployeeId} from cache", employeeId);
                    return cachedAttendances;
                }

                // Validate employee exists
                var employee = await _userRepository.GetByIdAsync(employeeId, true);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found or is deleted", employeeId);
                    return Enumerable.Empty<ShiftAttendanceResponse>();
                }

                // Use the DTO-returning repository method
                var response = await _shiftAttendanceRepository.GetEmployeeAttendancesAsync(employeeId);

                // Cache for 30 minutes
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved {Count} attendance records for employee {EmployeeId}", 
                    response.Count(), employeeId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<ShiftAttendance?> GetAttendanceByUserAndInstanceAsync(string userId, string shiftInstanceId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(shiftInstanceId))
                {
                    _logger.LogWarning("GetAttendanceByUserAndInstanceAsync called with null or empty parameters. UserId: {UserId}, ShiftInstanceId: {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                    return null;
                }

                _logger.LogDebug("Retrieving attendance for user {UserId} and shift instance {ShiftInstanceId}", 
                    userId, shiftInstanceId);

                // Try to get from cache first
                var cacheKey = $"user_attendance:{userId}:{shiftInstanceId}";
                var cachedAttendance = await _cacheService.GetAsync<ShiftAttendance>(cacheKey);
                if (cachedAttendance != null)
                {
                    _logger.LogDebug("Retrieved attendance from cache for user {UserId} and shift instance {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                    return cachedAttendance;
                }

                // Get from repository
                var attendance = await _shiftAttendanceRepository.GetByUserAndInstanceAsync(userId, shiftInstanceId);
                
                if (attendance != null)
                {
                    // Cache the result for 1 hour
                    await _cacheService.SetAsync(cacheKey, attendance, TimeSpan.FromHours(1));
                    _logger.LogDebug("Cached attendance for user {UserId} and shift instance {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                }
                else
                {
                    _logger.LogDebug("No attendance record found for user {UserId} and shift instance {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                }

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for user {UserId} and shift instance {ShiftInstanceId}", 
                    userId, shiftInstanceId);
                throw; // Re-throw to be handled by the caller
            }
        }

        private async Task InvalidateAttendanceCaches(string shiftInstanceId, string employeeId)
        {
            try
            {
                // Invalidate specific caches
                await _cacheService.RemoveAsync($"shift_instance_attendance:{shiftInstanceId}");
                await _cacheService.RemoveAsync($"employee_attendance:{employeeId}");
                
                // Invalidate pattern-based caches
                await _cacheService.RemovePatternAsync("shift_attendances:*");
                
                // Set recent change flag
                await _cacheService.SetAsync("shift_attendances:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(15));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error invalidating attendance caches for shift instance {ShiftInstanceId} and employee {EmployeeId}", 
                    shiftInstanceId, employeeId);
                // Don't throw - cache invalidation failure shouldn't break the main operation
            }
        }
    }
}