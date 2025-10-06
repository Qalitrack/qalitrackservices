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

        public async Task<PagedResult<ShiftAttendanceResponse>> GetShiftInstanceAttendanceAsync(
            Guid instanceId, 
            int pageNumber = 1, 
            int pageSize = 50, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Retrieving attendance for shift instance {InstanceId} (Page: {PageNumber}, Size: {PageSize})", 
                    instanceId, pageNumber, pageSize);

                // Get the shift instance
                var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(instanceId.ToString());
                if (shiftInstance == null)
                {
                    _logger.LogWarning("Shift instance {InstanceId} not found", instanceId);
                    return new PagedResult<ShiftAttendanceResponse>
                    {
                        Items = new List<ShiftAttendanceResponse>(),
                        Page = pageNumber,
                        PageSize = pageSize,
                        TotalCount = 0
                    };
                }

                // Get paginated attendances
                var attendances = await _shiftAttendanceRepository.GetByInstanceIdAsync(
                    instanceId.ToString(), 
                    pageNumber, 
                    pageSize, 
                    cancellationToken);

                // Map to response DTO
                var response = new PagedResult<ShiftAttendanceResponse>
                {
                    Page = attendances.Page,
                    PageSize = attendances.PageSize,
                    TotalCount = attendances.TotalCount,
                    Items = _mapper.Map<IEnumerable<ShiftAttendanceResponse>>(attendances.Items)
                };

                _logger.LogInformation("Retrieved {Count} attendance records for shift instance {InstanceId}", 
                    response.Items.Count(), instanceId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for shift instance {InstanceId}", instanceId);
                throw;
            }
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

        public async Task<PaginatedShiftInstancesResponse> GetPaginatedShiftInstancesWithAttendanceAsync(
            int pageNumber = 1, 
            int pageSize = 10, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            try
            {
                // Validate pagination parameters
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100);
                

                // Try cache first
                var cacheKey = $"paginated_shift_instances:{pageNumber}:{pageSize}:{startDate:yyyyMMdd}:{endDate:yyyyMMdd}";
                var cachedResponse = await _cacheService.GetAsync<PaginatedShiftInstancesResponse>(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Retrieved paginated shift instances from cache");
                    return cachedResponse;
                }

                // Get from repository
                var response = await _shiftAttendanceRepository.GetPaginatedShiftInstancesWithAttendanceAsync(
                    pageNumber, pageSize, startDate, endDate);

                // Cache for 10 minutes
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

                _logger.LogInformation("Retrieved {Count} shift instances (page {Page}/{TotalPages}) with attendance summaries", 
                    response.ShiftInstances.Count, response.PageNumber, response.TotalPages);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated shift instances with attendance");
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


                // Try cache first
                var cacheKey = $"shift_attendance_entity:{id}";
                var cachedAttendance = await _cacheService.GetAsync<ShiftAttendance>(cacheKey);
                if (cachedAttendance != null)
                {
                    _logger.LogDebug("Retrieved attendance entity from cache for id {AttendanceId}", id);
                    return cachedAttendance;
                }

                // Get the entity directly from the repository
                var attendance = await _shiftAttendanceRepository.GetByIdAsync(id);
                if (attendance == null || attendance.IsDeleted)
                {
                    _logger.LogWarning("Shift attendance {AttendanceId} not found or is deleted", id);
                    return null;
                }

                // Cache for 30 minutes
                await _cacheService.SetAsync(cacheKey, attendance, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved shift attendance {AttendanceId}", id);
                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift attendance {AttendanceId}", id);
                throw;
            }
        }

        public async Task<ShiftAttendanceResponse?> GetAttendanceDetailsAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    _logger.LogWarning("GetAttendanceDetailsAsync called with null or empty id");
                    return null;
                }


                // Try cache first
                var cacheKey = $"shift_attendance_details:{id}";
                var cachedResponse = await _cacheService.GetAsync<ShiftAttendanceResponse>(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogDebug("Retrieved attendance details from cache for id {AttendanceId}", id);
                    return cachedResponse;
                }

                // Get from repository
                var response = await _shiftAttendanceRepository.GetAttendanceDetailsAsync(id);
                if (response == null)
                {
                    _logger.LogWarning("Attendance details not found for id {AttendanceId}", id);
                    return null;
                }

                // Cache for 30 minutes
                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));

                _logger.LogInformation("Retrieved attendance details for id {AttendanceId}", id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance details for id {AttendanceId}", id);
                throw;
            }
        }

        public async Task<ShiftAttendance> CreateAsync(ShiftAttendance attendance)
        {
            try
            {
                if (attendance == null)
                {
                    throw new ArgumentNullException(nameof(attendance));
                }

                _logger.LogInformation("Creating attendance record for employee {EmployeeId} in shift instance {ShiftInstanceId}",
                    attendance.EmployeeId, attendance.ShiftInstanceId);

                // Validate shift instance exists
                var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(attendance.ShiftInstanceId);
                if (shiftInstance == null)
                {
                    throw new InvalidOperationException($"Shift instance {attendance.ShiftInstanceId} not found");
                }

                // Validate employee exists
                var employee = await _userRepository.GetByIdAsync(attendance.EmployeeId, true);
                if (employee == null)
                {
                    throw new InvalidOperationException($"Employee {attendance.EmployeeId} not found");
                }

                // Create the attendance record
                var result = await _shiftAttendanceRepository.CreateAsync(attendance);

                // Invalidate relevant caches
                await InvalidateAttendanceCaches(attendance.ShiftInstanceId, attendance.EmployeeId);
                

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating attendance record for employee {EmployeeId}", attendance?.EmployeeId);
                throw;
            }
        }

        public async Task<ShiftAttendance?> UpdateAsync(ShiftAttendance attendance)
        {
            try
            {
                if (attendance == null)
                {
                    throw new ArgumentNullException(nameof(attendance));
                }


                var result = await _shiftAttendanceRepository.UpdateAsync(attendance);
                if (result == null)
                {
                    _logger.LogWarning("Attendance record {AttendanceId} not found for update", attendance.Id);
                    return null;
                }

                // Invalidate relevant caches
                await InvalidateAttendanceCaches(attendance.ShiftInstanceId, attendance.EmployeeId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance record {AttendanceId}", attendance?.Id);
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
                
                // Get the attendance record first to get the IDs for cache invalidation
                var attendance = await _shiftAttendanceRepository.GetByIdAsync(id);
                if (attendance == null)
                {
                    _logger.LogWarning("Attendance record {AttendanceId} not found for deletion", id);
                    return false;
                }
                
                var success = await _shiftAttendanceRepository.DeleteAsync(id);
                
                if (success)
                {
                    // Invalidate caches
                    await InvalidateAttendanceCaches(attendance.ShiftInstanceId, attendance.EmployeeId);
                    await _cacheService.RemoveAsync($"shift_attendance_entity:{id}");
                    await _cacheService.RemoveAsync($"shift_attendance_details:{id}");
                    
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

                // Use the repository's clock in method
                var result = await _shiftAttendanceRepository.ClockInAsync(shiftInstanceId, employeeId, clockInTime, notes);
                
                if (result != null)
                {
                    // Update shift instance status if needed
                    if (shiftInstance.Status == ShiftInstanceStatus.Scheduled)
                    {
                        shiftInstance.Status = ShiftInstanceStatus.InProgress;
                        await _shiftInstanceRepository.UpdateAsync(shiftInstance);
                    }

                    // Invalidate relevant caches
                    await InvalidateAttendanceCaches(shiftInstanceId, employeeId);
                    
                }

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

                // Use the repository's clock out method
                var result = await _shiftAttendanceRepository.ClockOutAsync(shiftInstanceId, employeeId, clockOutTime, notes);
                
                if (result != null)
                {
                    // Check if all employees have clocked out and update shift instance status
                    var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(shiftInstanceId);
                    if (shiftInstance != null && shiftInstance.Status == ShiftInstanceStatus.InProgress)
                    {
                        // Get all attendances for the shift instance
                        var allAttendances = await _shiftAttendanceRepository.GetByInstanceIdAsync(shiftInstanceId, 1, int.MaxValue);
                        var allClockedOut = allAttendances.Items.All(a => a.ClockOutTime.HasValue || a.Status == AttendanceStatus.Absent);
                        
                        if (allClockedOut)
                        {
                            shiftInstance.Status = ShiftInstanceStatus.Completed;
                            await _shiftInstanceRepository.UpdateAsync(shiftInstance);
                        }
                    }

                    // Invalidate relevant caches
                    await InvalidateAttendanceCaches(shiftInstanceId, employeeId);

                    _logger.LogInformation("Employee {EmployeeId} successfully clocked out for shift instance {ShiftInstanceId} at {ClockOutTime}", 
                        employeeId, shiftInstanceId, clockOutTime);
                }

                return result;
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

                var employee = await _userRepository.GetByIdAsync(employeeId, true);
                if (employee == null)
                {
                    _logger.LogWarning("Employee {EmployeeId} not found or is deleted", employeeId);
                    return Enumerable.Empty<ShiftAttendanceResponse>();
                }

                var response = await _shiftAttendanceRepository.GetEmployeeAttendancesAsync(employeeId);

                await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));
                

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

                var cacheKey = $"user_attendance:{userId}:{shiftInstanceId}";
                var cachedAttendance = await _cacheService.GetAsync<ShiftAttendance>(cacheKey);
                if (cachedAttendance != null)
                {
                    _logger.LogDebug("Retrieved attendance from cache for user {UserId} and shift instance {ShiftInstanceId}", 
                        userId, shiftInstanceId);
                    return cachedAttendance;
                }

                var attendance = await _shiftAttendanceRepository.GetByUserAndInstanceAsync(userId, shiftInstanceId);
                
                if (attendance != null)
                {
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
                throw;
            }
        }

        public async Task<ShiftAttendance?> GetByShiftInstanceAndEmployeeAsync(string shiftInstanceId, string employeeId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shiftInstanceId) || string.IsNullOrWhiteSpace(employeeId))
                {
                    _logger.LogWarning("GetByShiftInstanceAndEmployeeAsync called with null or empty parameters. ShiftInstanceId: {ShiftInstanceId}, EmployeeId: {EmployeeId}", 
                        shiftInstanceId, employeeId);
                    return null;
                }

                _logger.LogDebug("Retrieving attendance for shift instance {ShiftInstanceId} and employee {EmployeeId}", 
                    shiftInstanceId, employeeId);

                // Try cache first
                var cacheKey = $"shift_employee_attendance:{shiftInstanceId}:{employeeId}";
                var cachedAttendance = await _cacheService.GetAsync<ShiftAttendance>(cacheKey);
                if (cachedAttendance != null)
                {
                    _logger.LogDebug("Retrieved attendance from cache for shift instance {ShiftInstanceId} and employee {EmployeeId}", 
                        shiftInstanceId, employeeId);
                    return cachedAttendance;
                }

                var attendance = await _shiftAttendanceRepository.GetByShiftInstanceAndEmployeeAsync(shiftInstanceId, employeeId);
                
                if (attendance != null)
                {
                    await _cacheService.SetAsync(cacheKey, attendance, TimeSpan.FromMinutes(30));
                    _logger.LogDebug("Cached attendance for shift instance {ShiftInstanceId} and employee {EmployeeId}", 
                        shiftInstanceId, employeeId);
                }

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for shift instance {ShiftInstanceId} and employee {EmployeeId}", 
                    shiftInstanceId, employeeId);
                throw;
            }
        }

        private async Task InvalidateAttendanceCaches(string shiftInstanceId, string employeeId)
        {
            try
            {
                // Invalidate specific caches
                await _cacheService.RemoveAsync($"shift_instance_attendance:{shiftInstanceId}");
                await _cacheService.RemoveAsync($"employee_attendance:{employeeId}");
                await _cacheService.RemoveAsync($"user_attendance:{employeeId}:{shiftInstanceId}");
                await _cacheService.RemoveAsync($"shift_employee_attendance:{shiftInstanceId}:{employeeId}");
                
                // Invalidate pattern-based caches
                await _cacheService.RemovePatternAsync("shift_attendances:*");
                await _cacheService.RemovePatternAsync("paginated_shift_instances:*");
                await _cacheService.RemovePatternAsync("shifts_with_instances:*");
                
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