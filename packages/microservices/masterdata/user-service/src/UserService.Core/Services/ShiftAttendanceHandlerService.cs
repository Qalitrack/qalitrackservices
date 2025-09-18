using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services
{
    public class ShiftAttendanceHandlerService : IShiftAttendanceHandlerService
    {
        private readonly IShiftAttendanceService _shiftAttendanceService;
        private readonly IShiftInstanceRepository _shiftInstanceRepository;
        private readonly ILogger<ShiftAttendanceHandlerService> _logger;

        public ShiftAttendanceHandlerService(
            IShiftAttendanceService shiftAttendanceService,
            IShiftInstanceRepository shiftInstanceRepository,
            ILogger<ShiftAttendanceHandlerService> logger)
        {
            _shiftAttendanceService = shiftAttendanceService;
            _shiftInstanceRepository = shiftInstanceRepository;
            _logger = logger;
        }

        public async Task<bool> HandleLoginAttendanceAsync(string userId, Shift shift, DateTime loginTime)
        {
            try
            {
                // Get the current shift instance for this shift
                var today = loginTime.Date;
                var shiftInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shift.Id);
                
                var currentInstance = shiftInstances
                    .FirstOrDefault(si => si.ScheduledDate.Date == today && 
                                         si.Status != ShiftInstanceStatus.Cancelled &&
                                         si.Status != ShiftInstanceStatus.Completed);

                if (currentInstance == null)
                {
                    _logger.LogWarning("No active shift instance found for shift {ShiftId} on {Date}", shift.Id, today);
                    return false;
                }

                // Check if user is already clocked in for this instance
                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
                
                if (existingAttendance != null && existingAttendance.ClockInTime.HasValue && !existingAttendance.ClockOutTime.HasValue)
                {
                    _logger.LogInformation("User {UserId} is already clocked in for shift instance {InstanceId}", 
                        userId, currentInstance.Id);
                    return false; // Already clocked in, no action needed
                }

                // Auto clock-in the user
                var attendance = await _shiftAttendanceService.ClockInAsync(
                    currentInstance.Id,
                    userId,
                    loginTime,
                    $"Auto clock-in via {shift.Mode} shift login");

                if (attendance != null)
                {
                    _logger.LogInformation("User {UserId} auto-clocked in for {ShiftMode} shift {ShiftName} at {LoginTime}", 
                        userId, shift.Mode, shift.Name, loginTime);
                    return true;
                }

                _logger.LogWarning("Failed to auto clock-in user {UserId} for shift {ShiftId}", userId, shift.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling login attendance for user {UserId} and shift {ShiftId}", userId, shift.Id);
                // Don't throw - login should proceed even if attendance registration fails
                return false;
            }
        }

        public async Task<bool> HandleLogoutAttendanceAsync(string userId, Shift shift, DateTime logoutTime)
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID cannot be null or empty");
                return false;
            }

            if (shift == null)
            {
                _logger.LogWarning("Shift cannot be null for user {UserId}", userId);
                return false;
            }

            try
            {
                _logger.LogDebug("Processing logout attendance for user {UserId} and shift {ShiftId} at {LogoutTime}", 
                    userId, shift.Id, logoutTime);

                // Get the current shift instance for this shift
                var today = logoutTime.Date;
                var shiftInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shift.Id);
                
                if (shiftInstances == null || !shiftInstances.Any())
                {
                    _logger.LogInformation("No shift instances found for shift {ShiftId}", shift.Id);
                    return false;
                }
                
                var currentInstance = shiftInstances
                    .FirstOrDefault(si => si.ScheduledDate.Date == today && 
                                       (si.Status == ShiftInstanceStatus.InProgress || si.Status == ShiftInstanceStatus.Scheduled));

                if (currentInstance == null)
                {
                    _logger.LogInformation("No active shift instance found for shift {ShiftId} on {Date}", shift.Id, today);
                    return false;
                }

                _logger.LogDebug("Found shift instance {InstanceId} with status {Status}", 
                    currentInstance.Id, currentInstance.Status);

                // Check if user is currently clocked in
                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
                
                if (existingAttendance == null)
                {
                    _logger.LogInformation("No attendance record found for user {UserId} and instance {InstanceId}", 
                        userId, currentInstance.Id);
                    return false;
                }

                if (!existingAttendance.ClockInTime.HasValue)
                {
                    _logger.LogInformation("User {UserId} has no clock-in time for instance {InstanceId}", 
                        userId, currentInstance.Id);
                    return false;
                }

                if (existingAttendance.ClockOutTime.HasValue)
                {
                    _logger.LogInformation("User {UserId} is already clocked out at {ClockOutTime} for instance {InstanceId}", 
                        userId, existingAttendance.ClockOutTime, currentInstance.Id);
                    return false;
                }

                // Auto clock-out the user
                var clockOutReason = $"Auto clock-out via {shift.Mode} shift logout at {logoutTime:yyyy-MM-dd HH:mm:ss}";
                _logger.LogDebug("Attempting to clock out user {UserId} with reason: {Reason}", userId, clockOutReason);
                
                var attendance = await _shiftAttendanceService.ClockOutAsync(
                    currentInstance.Id,
                    userId,
                    logoutTime,
                    clockOutReason);

                if (attendance != null)
                {
                    _logger.LogInformation("Successfully clocked out user {UserId} for {ShiftMode} shift {ShiftName} at {LogoutTime}", 
                        userId, shift.Mode, shift.Name, logoutTime);
                    return true;
                }

                _logger.LogWarning("Failed to clock out user {UserId} for shift {ShiftId}", userId, shift.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling logout attendance for user {UserId} and shift {ShiftId}", userId, shift?.Id);
                // Don't throw - logout should proceed even if attendance fails
                return false;
            }
        }
    }
}