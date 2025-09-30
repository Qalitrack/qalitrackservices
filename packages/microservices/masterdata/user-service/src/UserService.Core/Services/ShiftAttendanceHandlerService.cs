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
                _logger.LogInformation("Processing logout attendance for user {UserId} and shift {ShiftId} at {LogoutTime}", 
                    userId, shift.Id, logoutTime);

                // Get all instances for this shift
                var shiftInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shift.Id);
                if (shiftInstances == null || !shiftInstances.Any())
                {
                    _logger.LogInformation("No shift instances found for shift {ShiftId}", shift.Id);
                    return false;
                }

                // Find the most relevant instance based on the logout time
                var currentInstance = shiftInstances
                    .Where(si => si.Status == ShiftInstanceStatus.InProgress || si.Status == ShiftInstanceStatus.Scheduled)
                    .OrderByDescending(si => si.ScheduledDate)
                    .FirstOrDefault();

                if (currentInstance == null)
                {
                    _logger.LogInformation("No active or scheduled instance found for shift {ShiftId}", shift.Id);
                    return false;
                }

                _logger.LogInformation("Processing attendance for user {UserId} in instance {InstanceId} (Status: {Status})",
                    userId, currentInstance.Id, currentInstance.Status);

                // Get attendance record
                var attendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
                
                if (attendance == null)
                {
                    _logger.LogWarning("No attendance record found for user {UserId} in instance {InstanceId}. Skipping logout attendance update (no prior clock-in).", 
                        userId, currentInstance.Id);
                    return false;
                }

                // If already clocked out, no need to do anything
                if (attendance.ClockOutTime.HasValue)
                {
                    _logger.LogInformation("User {UserId} is already clocked out at {ClockOutTime} for instance {InstanceId}", 
                        userId, attendance.ClockOutTime, currentInstance.Id);
                    return true;
                }

                // Update the attendance record with clock-out time
                attendance.ClockOutTime = logoutTime;
                attendance.UpdatedAt = DateTime.UtcNow;
                attendance.UpdatedBy = "System";
                
                var updatedAttendance = await _shiftAttendanceService.UpdateAsync(attendance);
                var updated = updatedAttendance != null;
                
                if (updated)
                {
                    _logger.LogInformation("Successfully updated attendance for user {UserId} in shift {ShiftId}. Clock out at {ClockOutTime}",
                        userId, shift.Id, logoutTime);
                    return true;
                }
                
                _logger.LogWarning("Failed to update attendance record for user {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling logout attendance for user {UserId} and shift {ShiftId}", userId, shift?.Id);
                // Don't throw - logout should proceed even if attendance fails
                return false;
            }
        }

        public async Task<bool> HandleEarlyArrivalAttendanceAsync(string userId, Shift shift, DateTime arrivalTime, string shiftInstanceId,
            double minutesEarly)
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
                _logger.LogDebug("Processing early arrival for user {UserId}, shift {ShiftId}, instance {InstanceId} at {ArrivalTime} ({MinutesEarly} minutes early)", 
                    userId, shift.Id, shiftInstanceId, arrivalTime, minutesEarly);

                // Get the shift instance to verify it exists and is valid
                var shiftInstance = await _shiftInstanceRepository.GetByIdAsync(shiftInstanceId);
                if (shiftInstance == null)
                {
                    _logger.LogWarning("Shift instance {InstanceId} not found for early arrival of user {UserId}", 
                        shiftInstanceId, userId);
                    return false;
                }

                // Verify the shift instance belongs to the specified shift
                if (shiftInstance.ShiftId != shift.Id)
                {
                    _logger.LogWarning("Shift instance {InstanceId} does not belong to shift {ShiftId}", 
                        shiftInstanceId, shift.Id);
                    return false;
                }
                
                if (shiftInstance.Status != ShiftInstanceStatus.Scheduled && shiftInstance.Status != ShiftInstanceStatus.InProgress)
                {
                    _logger.LogInformation("Shift instance {InstanceId} is in status {Status}, not eligible for early arrival. Only Scheduled or InProgress instances are allowed.", 
                        shiftInstanceId, shiftInstance.Status);
                    return false;
                }

                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, shiftInstanceId);
                
                if (existingAttendance != null)
                {
                    if (existingAttendance.ClockInTime.HasValue && !existingAttendance.ClockOutTime.HasValue)
                    {
                        _logger.LogInformation("User {UserId} is already clocked in for shift instance {InstanceId}", 
                            userId, shiftInstanceId);
                        return false;
                    }
                    
                    if (existingAttendance.ClockOutTime.HasValue)
                    {
                        _logger.LogInformation("User {UserId} has already completed attendance for shift instance {InstanceId}", 
                            userId, shiftInstanceId);
                        return false;
                    }
                }

                // Register the early arrival
                var attendance = await _shiftAttendanceService.ClockInAsync(
                    shiftInstanceId,
                    userId,
                    arrivalTime,
                    $"Early arrival: {minutesEarly:F0} minutes before shift start. Auto clock-in via {shift.Mode} shift login");

                if (attendance != null)
                {
                    _logger.LogInformation("User {UserId} successfully registered early arrival for {ShiftMode} shift {ShiftName} at {ArrivalTime} ({MinutesEarly} minutes early)", 
                        userId, shift.Mode, shift.Name, arrivalTime, minutesEarly);
                    return true;
                }

                _logger.LogWarning("Failed to register early arrival for user {UserId} and shift instance {InstanceId}", 
                    userId, shiftInstanceId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling early arrival attendance for user {UserId}, shift {ShiftId}, instance {InstanceId}", 
                    userId, shift?.Id, shiftInstanceId);
                // Don't throw - the main login flow should continue even if early arrival registration fails
                return false;
            }
        }
    }
}