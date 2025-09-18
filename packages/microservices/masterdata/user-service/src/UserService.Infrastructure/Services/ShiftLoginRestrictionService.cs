using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly IShiftAttendanceHandlerService _shiftAttendanceHandlerService;
        private readonly IShiftInstanceRepository _shiftInstanceRepository;
        private readonly ILogger<ShiftLoginRestrictionService> _logger;

        public ShiftLoginRestrictionService(
            UserServiceDbContext context,
            IShiftAttendanceHandlerService shiftAttendanceHandlerService,
            IShiftInstanceRepository shiftInstanceRepository,
            ILogger<ShiftLoginRestrictionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _shiftAttendanceHandlerService = shiftAttendanceHandlerService ?? throw new ArgumentNullException(nameof(shiftAttendanceHandlerService));
            _shiftInstanceRepository = shiftInstanceRepository ?? throw new ArgumentNullException(nameof(shiftInstanceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

                // 5. Check if user is assigned to any running shift (NO ATTENDANCE HANDLING HERE)
                var assignedToAnyRunningShift = userAssignments
                    .Any(us => runningShifts.Any(s => s.Id == us.ShiftId));

                if (assignedToAnyRunningShift)
                {
                    var assignedShift = userAssignments
                        .First(us => runningShifts.Any(s => s.Id == us.ShiftId)).Shift;

                    _logger.LogInformation("User {UserId} allowed login - assigned to {ShiftMode} shift: {ShiftName}", 
                        userId, assignedShift.Mode, assignedShift.Name);
                    
                    return (true, $"Assigned to {assignedShift.Mode} shift: {assignedShift.Name}");
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

        public async Task<bool> HandleLoginAttendanceAsync(string userId)
        {
            try
            {
                _logger.LogInformation("Handling login attendance for user {UserId}", userId);

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

                bool attendanceHandled = false;

                foreach (var assignment in userAssignments)
                {
                    var shift = assignment.Shift;
                    
                    // Handle attendance registration for both Strict and Open shifts
                    var handled = await _shiftAttendanceHandlerService.HandleLoginAttendanceAsync(userId, shift, currentDateTime);
                    
                    if (handled)
                    {
                        attendanceHandled = true;
                        _logger.LogInformation("User {UserId} auto clocked-in for {ShiftMode} shift: {ShiftName}", 
                            userId, shift.Mode, shift.Name);
                    }
                }

                return attendanceHandled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling login attendance for user {UserId}", userId);
                // Don't throw - login should succeed even if attendance fails
                return false;
            }
        }

        public async Task HandleUserLogoutAsync(string userId, DateTime currentDateTime)
        {
            try
            {
                _logger.LogInformation("Handling logout for user {UserId} at {LogoutTime}", userId, currentDateTime);

                var currentTime = TimeOnly.FromDateTime(currentDateTime);

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
                        var logoutHandled = await _shiftAttendanceHandlerService.HandleLogoutAttendanceAsync(userId, shift, currentDateTime);
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

        private async Task<List<Shift>> GetCurrentlyRunningShifts(TimeOnly currentTime)
        {
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var utcNow = DateTime.UtcNow;
            var currentTimeOfDay = currentTime.ToTimeSpan();
            
            _logger.LogInformation($"[Time Debug] Current UTC time: {utcNow:yyyy-MM-dd HH:mm:ss}");
            _logger.LogInformation($"[Time Debug] Current Nairobi time: {TimeZoneInfo.ConvertTimeFromUtc(utcNow, nairobiTimeZone):yyyy-MM-dd HH:mm:ss}");
            
            // First, get all shifts that are within their date range (using UTC for database query)
            var shifts = await _context.Shifts
                .Where(s => !s.IsDeleted &&
                    s.StartDate <= utcNow &&
                    (s.EndDate == null || s.EndDate >= utcNow.Date))
                .ToListAsync();
                
            _logger.LogInformation($"[Time Debug] Found {shifts.Count} shifts within date range");
            
            // Then filter in memory for shifts that should be active now in Nairobi time
            var result = shifts.Where(s => 
            {
                // Convert shift times to Nairobi time for comparison
                var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nairobiTimeZone);
                var nairobiTime = nairobiNow.TimeOfDay;
                
                _logger.LogInformation(@"
[Time Debug] Checking shift: {0}
  - Shift times: {1:hh\:mm} - {2:hh\:mm}
  - Current Nairobi time: {3:hh\:mm}
  - Shift dates: {4:yyyy-MM-dd} to {5}",
                    s.Name,
                    s.StartTime,
                    s.EndTime,
                    nairobiTime,
                    s.StartDate,
                    s.EndDate?.ToString("yyyy-MM-dd") ?? "No end date");
                
                bool isActive;
                // Check if current time in Nairobi is within the shift time window
                if (s.StartTime < s.EndTime)
                {
                    // Normal shift (not overnight)
                    isActive = nairobiTime >= s.StartTime && nairobiTime <= s.EndTime;
                    _logger.LogInformation(@"  - Normal shift check: {0:hh\:mm} between {1:hh\:mm} and {2:hh\:mm} = {3}",
                        nairobiTime, s.StartTime, s.EndTime, isActive);
                }
                else
                {
                    // Overnight shift
                    isActive = nairobiTime >= s.StartTime || nairobiTime <= s.EndTime;
                    _logger.LogInformation(@"  - Overnight shift check: {0:hh\:mm} >= {1:hh\:mm} OR <= {2:hh\:mm} = {3}",
                        nairobiTime, s.StartTime, s.EndTime, isActive);
                }
                
                if (isActive)
                {
                    _logger.LogInformation(@"  - Shift {0} is ACTIVE at current time", s.Name);
                }
                
                return isActive;
            }).ToList();
            
            _logger.LogInformation($"[Time Debug] Found {result.Count} active shifts");
            return result;
        }

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