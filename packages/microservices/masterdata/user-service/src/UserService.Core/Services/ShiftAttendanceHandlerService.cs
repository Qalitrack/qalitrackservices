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

                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
                
                if (existingAttendance != null && existingAttendance.ClockInTime.HasValue && !existingAttendance.ClockOutTime.HasValue)
                {
                    return false; 
                }

                // Auto clock-in the user
                var attendance = await _shiftAttendanceService.ClockInAsync(
                    currentInstance.Id,
                    userId,
                    
                    loginTime,
                    $"Auto clock-in via {shift.Mode} shift login");

                if (attendance != null)
                {
                    return true;
                }

                _logger.LogWarning("Failed to auto clock-in user {UserId} for shift {ShiftId}", userId, shift.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling login attendance for user {UserId} and shift {ShiftId}", userId, shift.Id);
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

            // Get all instances for this shift
            var shiftInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shift.Id);
            if (shiftInstances == null || !shiftInstances.Any())
            {
                return false;
            }

            // Find the most relevant instance based on the logout time
            var currentInstance = shiftInstances
                .Where(si => si.Status == ShiftInstanceStatus.InProgress || si.Status == ShiftInstanceStatus.Scheduled)
                .OrderByDescending(si => si.ScheduledDate)
                .FirstOrDefault();

            if (currentInstance == null)
            {
                return false;
            }

           

            // Calculate scheduled times for notes (defined once to avoid conflict)
            var scheduledStartTime = currentInstance.ScheduledDate.Date.Add(currentInstance.ScheduledStartTime.TimeOfDay);
            var scheduledEndTime = currentInstance.ScheduledDate.Date.Add(currentInstance.ScheduledEndTime.TimeOfDay);

            // Get attendance record
            var attendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
            
            if (attendance == null)
            {
              
                // Create a new attendance record with Absent status
                var absentAttendance = new ShiftAttendance
                {
                    ShiftInstanceId = currentInstance.Id,
                    EmployeeId = userId,
                    Status = AttendanceStatus.Absent,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    Notes = $"Marked absent: User did not clock in for shift instance {currentInstance.Id}. " +
                            $"Scheduled start: {scheduledStartTime:yyyy-MM-dd HH:mm:ss UTC}, end: {scheduledEndTime:yyyy-MM-dd HH:mm:ss UTC}."
                };

                var createdAttendance = await _shiftAttendanceService.CreateAsync(absentAttendance);
                if (createdAttendance != null)
                {
                  
                    return true; // Successfully marked as Absent
                }
                else
                {
                    _logger.LogWarning("Failed to create Absent attendance record for user {UserId} in shift instance {InstanceId}", 
                        userId, currentInstance.Id);
                    return false;
                }
            }

            // If already clocked out, no need to do anything
            if (attendance.ClockOutTime.HasValue)
            {
           
                return true;
            }

            // Calculate clock-out timing for notes
            string clockOutNote = "";
            if (logoutTime < scheduledEndTime)
            {
                var minutesEarly = (scheduledEndTime - logoutTime).TotalMinutes;
                clockOutNote = $"Clocked out early: {minutesEarly:F0} minutes before shift end.";
            }
            else if (logoutTime > scheduledEndTime)
            {
                var minutesLate = (logoutTime - scheduledEndTime).TotalMinutes;
                clockOutNote = $"Clocked out late: {minutesLate:F0} minutes after shift end.";
            }
            else
            {
                clockOutNote = "Clocked out on time.";
            }

            // Preserve existing notes (e.g., from early login) and append clock-out note
            attendance.Notes = string.IsNullOrEmpty(attendance.Notes) ? clockOutNote : $"{attendance.Notes} {clockOutNote}";
            attendance.ClockOutTime = logoutTime;
            attendance.UpdatedAt = DateTime.UtcNow;
            attendance.UpdatedBy = "System";
            var updatedAttendance = await _shiftAttendanceService.UpdateAsync(attendance);
            var updated = updatedAttendance != null;
            
            if (updated)
            {
                return true;
            }
            
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
                  
                    return false;
                }

                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, shiftInstanceId);
                
                if (existingAttendance != null)
                {
                    if (existingAttendance.ClockInTime.HasValue && !existingAttendance.ClockOutTime.HasValue)
                    {
                       
                        return false;
                    }
                    
                    if (existingAttendance.ClockOutTime.HasValue)
                    {
                      
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
                return false;
            }
            
        }
    }
}