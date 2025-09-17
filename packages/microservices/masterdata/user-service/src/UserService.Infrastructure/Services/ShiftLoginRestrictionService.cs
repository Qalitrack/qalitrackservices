using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Services;
using UserService.Core.Interfaces.Repositories;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Services
{
    public class ShiftLoginRestrictionService : IShiftLoginRestrictionService
    {
        private readonly UserServiceDbContext _context;
        private readonly IShiftAttendanceService _shiftAttendanceService;
        private readonly IShiftInstanceRepository _shiftInstanceRepository;
        private readonly ILogger<ShiftLoginRestrictionService> _logger;

        public ShiftLoginRestrictionService(
            UserServiceDbContext context,
            IShiftAttendanceService shiftAttendanceService,
            IShiftInstanceRepository shiftInstanceRepository,
            ILogger<ShiftLoginRestrictionService> logger)
        {
            _context = context;
            _shiftAttendanceService = shiftAttendanceService;
            _shiftInstanceRepository = shiftInstanceRepository;
            _logger = logger;
        }

        public async Task<(bool IsAllowed, string Reason)> CanUserLoginAsync(string userId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Check if user is an admin first (bypass all restrictions)
                var isPrivilegedUser = await IsUserAdminAsync(userId);
                if (isPrivilegedUser)
                {
                    _logger.LogInformation("User {UserId} is a privileged user, allowing login", userId);
                    return (true, "Privileged user - access granted");
                }

                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);
                var currentDateTime = DateTime.UtcNow;

                // 1. Get all currently running shifts (both Strict and Open)
                var runningShifts = await GetCurrentlyRunningShifts(currentTime);
                
                // If no shifts are running, allow login
                if (!runningShifts.Any())
                {
                    _logger.LogInformation("No active shifts found, allowing login for user {UserId}", userId);
                    return (true, "No active shifts - access granted");
                }

                // 2. Separate Strict shifts
                var strictShifts = runningShifts.Where(s => s.Mode == ShiftMode.Strict).ToList();

                // 3. Get user assignments
                var userAssignments = await _context.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == userId && !us.IsDeleted)
                    .ToListAsync();
                
                // 4. Check strict shift rules
                if (strictShifts.Any())
                {
                    var assignedToStrictShift = userAssignments
                        .Any(us => strictShifts.Any(s => s.Id == us.ShiftId));

                    if (!assignedToStrictShift)
                    {
                        var shiftNames = string.Join(", ", strictShifts.Select(s => s.Name));
                        _logger.LogWarning("User {UserId} denied login - not assigned to active strict shift(s): {ShiftNames}", 
                            userId, shiftNames);
                        return (false, $"Access denied. Active strict shift(s): {shiftNames}");
                    }
                }

                // 5. Check if user is assigned to any running shift and handle attendance
                var assignedToAnyRunningShift = userAssignments
                    .Any(us => runningShifts.Any(s => s.Id == us.ShiftId));

                if (assignedToAnyRunningShift)
                {
                    var assignedShift = userAssignments
                        .First(us => runningShifts.Any(s => s.Id == us.ShiftId)).Shift;

                    // Handle attendance registration for both Strict and Open shifts
                    var attendanceHandled = await HandleLoginAttendanceAsync(userId, assignedShift, currentDateTime);

                    _logger.LogInformation("User {UserId} allowed login - assigned to {ShiftMode} shift: {ShiftName}. Attendance handled: {AttendanceHandled}", 
                        userId, assignedShift.Mode, assignedShift.Name, attendanceHandled);
                    
                    var message = $"Assigned to {assignedShift.Mode} shift: {assignedShift.Name}";
                    if (attendanceHandled)
                    {
                        message += " - Auto clocked-in";
                    }
                    
                    return (true, message);
                }

                // 6. If only Open shifts are running (no Strict shifts), allow login
                if (!strictShifts.Any())
                {
                    _logger.LogInformation("User {UserId} allowed login - only open shifts are active", userId);
                    return (true, "Access granted. Only open shifts are active");
                }

                // Default deny if we get here
                _logger.LogWarning("User {UserId} denied login - no valid shift assignment", userId);
                return (false, "Access denied. No valid shift assignment");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login restriction check for user {UserId}", userId);
                // In case of any error, allow login by default to avoid locking users out
                return (true, "System error - access granted");
            }
            finally
            {
                _logger.LogInformation("Login restriction check for user {UserId} completed in {ElapsedMs}ms", 
                    userId, stopwatch.ElapsedMilliseconds);
            }
        }

        public async Task HandleUserLogoutAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Handling logout for user {UserId}", userId);

                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);
                var currentDateTime = DateTime.UtcNow;

                // Get currently running shifts
                var runningShifts = await GetCurrentlyRunningShifts(currentTime);
                
                // Get user assignments to running shifts
                var userAssignments = await _context.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == userId && !us.IsDeleted)
                    .Where(us => runningShifts.Any(s => s.Id == us.ShiftId))
                    .ToListAsync();

                foreach (var assignment in userAssignments)
                {
                    var shift = assignment.Shift;
                    
                    // Only auto-clock-out for Strict shifts
                    if (shift.Mode == ShiftMode.Strict)
                    {
                        var logoutHandled = await HandleLogoutAttendanceAsync(userId, shift, currentDateTime);
                        _logger.LogInformation("User {UserId} logout attendance handled for strict shift {ShiftName}: {Handled}", 
                            userId, shift.Name, logoutHandled);
                    }
                    // For Open shifts, we let users manually clock out when they choose
                    else
                    {
                        _logger.LogInformation("User {UserId} logged out from open shift {ShiftName} - manual clock-out required", 
                            userId, shift.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling logout for user {UserId}", userId);
                // Don't throw - logout should proceed even if attendance fails
            }
        }

        private async Task<bool> HandleLoginAttendanceAsync(string userId, Shift shift, DateTime loginTime)
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

        private async Task<bool> HandleLogoutAttendanceAsync(string userId, Shift shift, DateTime logoutTime)
        {
            try
            {
                // Get the current shift instance for this shift
                var today = logoutTime.Date;
                var shiftInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shift.Id);
                
                var currentInstance = shiftInstances
                    .FirstOrDefault(si => si.ScheduledDate.Date == today && 
                                         si.Status == ShiftInstanceStatus.InProgress);

                if (currentInstance == null)
                {
                    _logger.LogWarning("No in-progress shift instance found for shift {ShiftId} on {Date}", shift.Id, today);
                    return false;
                }

                // Check if user is currently clocked in
                var existingAttendance = await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, currentInstance.Id);
                
                if (existingAttendance == null || !existingAttendance.ClockInTime.HasValue || existingAttendance.ClockOutTime.HasValue)
                {
                    _logger.LogInformation("User {UserId} is not currently clocked in for shift instance {InstanceId}", 
                        userId, currentInstance.Id);
                    return false; // Not clocked in or already clocked out
                }

                // Auto clock-out the user (only for Strict shifts)
                var attendance = await _shiftAttendanceService.ClockOutAsync(
                    currentInstance.Id,
                    userId,
                    logoutTime,
                    "Auto clock-out via strict shift logout");

                if (attendance != null)
                {
                    _logger.LogInformation("User {UserId} auto-clocked out for strict shift {ShiftName} at {LogoutTime}", 
                        userId, shift.Name, logoutTime);
                    return true;
                }

                _logger.LogWarning("Failed to auto clock-out user {UserId} for shift {ShiftId}", userId, shift.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling logout attendance for user {UserId} and shift {ShiftId}", userId, shift.Id);
                // Don't throw - logout should proceed even if attendance fails
                return false;
            }
        }

        private async Task<List<Shift>> GetCurrentlyRunningShifts(TimeOnly currentTime)
        {
            var currentTimeOfDay = currentTime.ToTimeSpan();
            
            return await _context.Shifts
                .Where(s => s.IsActive && !s.IsDeleted &&
                    // Normal shift (start < end)
                    ((s.StartTime <= s.EndTime && 
                     s.StartTime <= currentTimeOfDay && 
                     s.EndTime >= currentTimeOfDay) ||
                    // Overnight shift (start > end)
                    (s.StartTime > s.EndTime && 
                     (s.StartTime <= currentTimeOfDay || 
                      s.EndTime >= currentTimeOfDay)))
                )
                .ToListAsync();
        }

        /// <summary>
        /// Determines if a user is an admin and should bypass shift restrictions
        /// Checks if user has Admin, Manager, or Supervisor roles
        /// </summary>
        private async Task<bool> IsUserAdminAsync(string userId)
        {
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            // Allow Admin, Manager, and Supervisor roles to bypass shift restrictions
            var privilegedRoles = new HashSet<string>
            {
                "Admin",
                "Manager", 
                "Supervisor"
            };

            return userRoles.Any(role => privilegedRoles.Contains(role));
        }
    }
}