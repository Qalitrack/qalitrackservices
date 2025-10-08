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
        private readonly IUserStatusService _userStatusService;
        private readonly ILogger<ShiftLoginRestrictionService> _logger;

        public ShiftLoginRestrictionService(
            UserServiceDbContext context,
            IShiftAttendanceHandlerService shiftAttendanceHandlerService,
            IShiftInstanceRepository shiftInstanceRepository,
            IUserStatusService userStatusService,
            ILogger<ShiftLoginRestrictionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _shiftAttendanceHandlerService = shiftAttendanceHandlerService ?? throw new ArgumentNullException(nameof(shiftAttendanceHandlerService));
            _shiftInstanceRepository = shiftInstanceRepository ?? throw new ArgumentNullException(nameof(shiftInstanceRepository));
            _userStatusService = userStatusService ?? throw new ArgumentNullException(nameof(userStatusService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<(bool IsAllowed, string Reason)> CanUserLoginAsync(string userId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _logger.LogInformation("=== LOGIN ATTEMPT FOR USER {UserId} ===", userId);
                
                // 1. Check if user is an admin first (bypass all restrictions)
                var isPrivilegedUser = await IsUserAdminAsync(userId);
                if (isPrivilegedUser)
                {
                    _logger.LogInformation("User {UserId} is a privileged user, allowing login", userId);
                    return (true, "Privileged user - access granted");
                }

                // 2. Get currently active instances using shared logic
                var relevantInstances = await GetCurrentlyActiveInstancesAsync(DateTime.UtcNow);

                _logger.LogInformation("Found {Count} active shift instances", relevantInstances.Count);
                
                // If no active instances, allow login
                if (!relevantInstances.Any())
                {
                    _logger.LogInformation("No active shift instances found, allowing login for user {UserId}", userId);
                    return (true, "No active shift instances - access granted");
                }

                // 3. Get shift IDs for the relevant instances
                var relevantShiftIds = relevantInstances.Select(i => i.ShiftId).ToList();
                
                // 4. Get all shifts for the relevant instances in a single query
                var shifts = await _context.Shifts
                    .Where(s => relevantShiftIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s);
                
                // 5. Separate Strict and Open instances
                var strictInstances = relevantInstances
                    .Where(i => shifts.TryGetValue(i.ShiftId, out var shift) && shift.Mode == ShiftMode.Strict)
                    .ToList();
                
                // 6. Get user assignments for these instances
                var userAssignments = await _context.UserShifts
                    .Where(us => us.UserId == userId && 
                               !us.IsDeleted && 
                               relevantShiftIds.Contains(us.ShiftId))
                    .ToListAsync();
                
                // Materialize the shift data for user assignments
                var userAssignmentShifts = await _context.Shifts
                    .Where(s => userAssignments.Select(ua => ua.ShiftId).Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id, s => s);
                
                // Attach shifts to user assignments
                foreach (var assignment in userAssignments)
                {
                    if (userAssignmentShifts.TryGetValue(assignment.ShiftId, out var shift))
                    {
                        assignment.Shift = shift;
                    }
                }

                // 7. Check strict shift rules first (if any strict instances are active)
                if (strictInstances.Any())
                {
                    var strictShiftIds = strictInstances.Select(i => i.ShiftId).ToHashSet();
                    var assignedToStrictInstance = userAssignments
                        .Any(us => strictShiftIds.Contains(us.ShiftId));

                    if (!assignedToStrictInstance)
                    {
                        var shiftNames = string.Join(", ", strictInstances.Select(i => i.Shift?.Name ?? "Unknown"));
                        _logger.LogWarning("User {UserId} denied login - not assigned to active strict shift instance(s): {ShiftNames}", 
                            userId, shiftNames);
                        return (false, $"Access denied. Active strict shift(s): {shiftNames}");
                    }
                    
                    _logger.LogInformation("User {UserId} is assigned to an active strict shift instance", userId);
                    return (true, "Access granted - assigned to active strict shift");
                }

                // 8. If only Open shifts are active, check if user is assigned to any of them
                var assignedToAnyInstance = userAssignments.Any();
                if (assignedToAnyInstance)
                {
                    var assignedShift = userAssignments.First().Shift;
                    _logger.LogInformation("User {UserId} allowed login - assigned to open shift: {ShiftName}", 
                        userId, assignedShift?.Name ?? "Unknown");
                    return (true, $"Assigned to open shift: {assignedShift?.Name ?? "Unknown"}");
                }

                // 9. If no strict shifts and user not assigned to any active instances
                _logger.LogInformation("User {UserId} not assigned to any active shift instances, but no strict shifts active", userId);
                return (true, "No active strict shifts - access granted");
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

                var currentDateTime = DateTime.UtcNow;

                // Get both currently active AND upcoming instances
                var (activeInstances, upcomingInstances) = await GetActiveAndUpcomingInstancesAsync();
                
                if (!activeInstances.Any() && !upcomingInstances.Any())
                {
                    _logger.LogInformation("No active or upcoming shift instances found for attendance handling");
                    return false;
                }

                var allRelevantShiftIds = activeInstances.Concat(upcomingInstances)
                    .Select(i => i.ShiftId)
                    .Distinct()
                    .ToList();

                var userAssignments = await _context.UserShifts
                    .Where(us => us.UserId == userId && 
                               !us.IsDeleted && 
                               allRelevantShiftIds.Contains(us.ShiftId))
                    .Select(us => new { us.Id, us.ShiftId })
                    .ToListAsync();

                if (!userAssignments.Any())
                {
                    _logger.LogInformation("No user assignments found for active or upcoming shift instances");
                    return false;
                }

                var shiftIds = userAssignments.Select(ua => ua.ShiftId).Distinct().ToList();
                var shifts = await _context.Shifts
                    .Where(s => shiftIds.Contains(s.Id))
                    .ToDictionaryAsync(s => s.Id);

                bool attendanceHandled = false;

                // Handle active instances first
                foreach (var assignment in userAssignments)
                {
                    if (!shifts.TryGetValue(assignment.ShiftId, out var shift))
                        continue;

                    // Check if this shift has an active instance
                    var activeInstance = activeInstances.FirstOrDefault(i => i.ShiftId == shift.Id);
                    if (activeInstance != null)
                    {
                        var handled = await _shiftAttendanceHandlerService.HandleLoginAttendanceAsync(
                            userId, shift, currentDateTime);
                        
                        if (handled)
                        {
                            attendanceHandled = true;
                            _logger.LogInformation("User {UserId} clocked-in for active {ShiftMode} shift: {ShiftName}", 
                                userId, shift.Mode, shift.Name);
                        }
                        continue; // Skip upcoming check for this shift
                    }

                    // Check if this shift has an upcoming instance (early arrival)
                    var upcomingInstance = upcomingInstances.FirstOrDefault(i => i.ShiftId == shift.Id);
                    if (upcomingInstance != null)
                    {
                        // Calculate how early they are
                        var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
                        var scheduledStartUtc = upcomingInstance.ScheduledDate.Date.Add(upcomingInstance.ScheduledStartTime.TimeOfDay);
                        var scheduledStartNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledStartUtc, nairobiTimeZone);
                        var currentNairobi = TimeZoneInfo.ConvertTimeFromUtc(currentDateTime, nairobiTimeZone);
                        var minutesEarly = (scheduledStartNairobi - currentNairobi).TotalMinutes;

                        var handled = await _shiftAttendanceHandlerService.HandleEarlyArrivalAttendanceAsync(
                            userId, shift, currentDateTime, upcomingInstance.Id, minutesEarly);
                        
                        if (handled)
                        {
                            attendanceHandled = true;
                            _logger.LogInformation("User {UserId} recorded early arrival for {ShiftMode} shift: {ShiftName} ({MinutesEarly:F0} minutes early)", 
                                userId, shift.Mode, shift.Name, minutesEarly);
                        }
                    }
                }

                // **NEW: Update user status to online after successful login attendance**
                if (attendanceHandled)
                {
                    _userStatusService.EnqueueStatusUpdate(userId, isActive: true);
                    _logger.LogInformation("Enqueued status update to online for user {UserId}", userId);
                }

                return attendanceHandled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling login attendance for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Gets both currently active instances and upcoming instances (for early arrival tracking)
        /// </summary>
        private async Task<(List<ShiftInstance> ActiveInstances, List<ShiftInstance> UpcomingInstances)> GetActiveAndUpcomingInstancesAsync()
        {
            var currentTime = DateTime.UtcNow;
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(currentTime, nairobiTimeZone);

            _logger.LogInformation("Getting active and upcoming instances - UTC: {UtcNow}, Nairobi: {NairobiNow}", 
                currentTime, nairobiNow);

            // Get instances that could be relevant (InProgress and Scheduled for today)
            var inProgressInstances = await _shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.InProgress);
            var scheduledInstances = await _shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.Scheduled);
            var potentialInstances = inProgressInstances.Concat(scheduledInstances).ToList();

            var todayInstances = potentialInstances
                .Where(instance => 
                {
                    var scheduledDateUtc = instance.ScheduledDate.Date;
                    var scheduledDateInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledDateUtc, nairobiTimeZone).Date;
                    return scheduledDateInNairobi == nairobiNow.Date; // Only today's instances
                })
                .ToList();

            var activeInstances = new List<ShiftInstance>();
            var upcomingInstances = new List<ShiftInstance>();

            foreach (var instance in todayInstances)
            {
                var scheduledDateUtc = instance.ScheduledDate.Date;
                var startTimeUtc = scheduledDateUtc.Add(instance.ScheduledStartTime.TimeOfDay);
                var endTimeUtc = scheduledDateUtc.Add(instance.ScheduledEndTime.TimeOfDay);
                
                var startTimeNairobi = TimeZoneInfo.ConvertTimeFromUtc(startTimeUtc, nairobiTimeZone);
                var endTimeNairobi = TimeZoneInfo.ConvertTimeFromUtc(endTimeUtc, nairobiTimeZone);
                
                // Check if currently active
                if (nairobiNow >= startTimeNairobi && nairobiNow <= endTimeNairobi)
                {
                    activeInstances.Add(instance);
                    _logger.LogInformation("Instance {InstanceId} for shift {ShiftId} is active: {StartTime} - {EndTime} (Nairobi)", 
                        instance.Id, instance.ShiftId, startTimeNairobi, endTimeNairobi);
                }
                // Check if upcoming (within next 2 hours - configurable)
                else if (nairobiNow < startTimeNairobi && (startTimeNairobi - nairobiNow).TotalHours <= 2)
                {
                    upcomingInstances.Add(instance);
                    _logger.LogInformation("Instance {InstanceId} for shift {ShiftId} is upcoming: starts at {StartTime} (Nairobi)", 
                        instance.Id, instance.ShiftId, startTimeNairobi);
                }
            }

            _logger.LogInformation("Found {ActiveCount} active and {UpcomingCount} upcoming instances", 
                activeInstances.Count, upcomingInstances.Count);

            return (activeInstances, upcomingInstances);
        }

        public async Task HandleUserLogoutAsync(string userId, DateTime currentDateTime)
        {
            try
            {
                _logger.LogInformation("Handling logout for user {UserId} at {LogoutTime}", userId, currentDateTime);

                _logger.LogDebug("Looking for active instances at {CurrentDateTime} (UTC) for user {UserId}", currentDateTime, userId);
                
                // Get active instances at the time of logout
                var activeInstances = await GetCurrentlyActiveInstancesAsync(currentDateTime);
                var activeShiftIds = activeInstances.Select(i => i.ShiftId).ToList();
                
                _logger.LogDebug("Found {Count} active instances for user {UserId} at {CurrentDateTime}", 
                    activeInstances.Count, userId, currentDateTime);
                
                // Get user assignments to active shifts in a single query
                var userAssignments = await _context.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == userId && 
                                !us.IsDeleted && 
                                activeShiftIds.Contains(us.ShiftId))
                    .ToListAsync();

                bool logoutHandled = false;

                foreach (var assignment in userAssignments)
                {
                    var shift = assignment.Shift;
                    
                    // Process clock-out for all shift types
                    var handled = await _shiftAttendanceHandlerService.HandleLogoutAttendanceAsync(userId, shift, currentDateTime);
                    
                    if (handled)
                    {
                        logoutHandled = true;
                    }
                    
                    _logger.LogInformation("User {UserId} logout attendance handled for shift {ShiftName} (Mode: {ShiftMode}): {Handled}", 
                        userId, shift.Name, shift.Mode, handled);
                }

                // **NEW: Update user status to offline after logout**
                _userStatusService.EnqueueStatusUpdate(userId, isActive: false);
                _logger.LogInformation("Enqueued status update to offline for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling logout for user {UserId}", userId);
                
                // **IMPORTANT: Still update user status to offline even if attendance fails**
                try
                {
                    _userStatusService.EnqueueStatusUpdate(userId, isActive: false);
                    _logger.LogInformation("Status updated to offline for user {UserId} despite logout error", userId);
                }
                catch (Exception statusEx)
                {
                    _logger.LogError(statusEx, "Failed to update status for user {UserId} after logout error", userId);
                }
                // Don't throw - logout should proceed even if attendance fails
            }
        }

        /// <summary>
        /// Gets shift instances that should be considered currently active.
        /// This includes both InProgress instances and Scheduled instances that fall within their time window.
        /// </summary>
        private async Task<List<ShiftInstance>> GetCurrentlyActiveInstancesAsync(DateTime currentTime)
        {
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(currentTime, nairobiTimeZone);

            _logger.LogInformation("Getting active instances - UTC: {UtcNow}, Nairobi: {NairobiNow}", 
                currentTime, nairobiNow);

            // Get instances that could be active (both InProgress and Scheduled)
            // This handles the case where instances haven't been updated to InProgress yet
            var inProgressInstances = await _shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.InProgress);
            var scheduledInstances = await _shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.Scheduled);
            var potentialInstances = inProgressInstances.Concat(scheduledInstances).ToList();

            var activeInstances = potentialInstances
                .Where(instance => 
                {
                    var scheduledDateUtc = instance.ScheduledDate.Date;
                    var scheduledDateInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledDateUtc, nairobiTimeZone).Date;
                    
                    // Only consider instances for today (Nairobi time)
                    if (scheduledDateInNairobi != nairobiNow.Date)
                        return false;
                        
                    var startTimeUtc = scheduledDateUtc.Add(instance.ScheduledStartTime.TimeOfDay);
                    var endTimeUtc = scheduledDateUtc.Add(instance.ScheduledEndTime.TimeOfDay);
                    
                    var startTimeNairobi = TimeZoneInfo.ConvertTimeFromUtc(startTimeUtc, nairobiTimeZone);
                    var endTimeNairobi = TimeZoneInfo.ConvertTimeFromUtc(endTimeUtc, nairobiTimeZone);
                    
                    // Check if current Nairobi time is within the shift time window
                    var isInTimeWindow = nairobiNow >= startTimeNairobi && nairobiNow <= endTimeNairobi;
                    
                    if (isInTimeWindow)
                    {
                        _logger.LogInformation("Instance {InstanceId} for shift {ShiftId} is active: {StartTime} - {EndTime} (Nairobi)", 
                            instance.Id, instance.ShiftId, startTimeNairobi, endTimeNairobi);
                    }
                    
                    return isInTimeWindow;
                })
                .ToList();

            _logger.LogInformation("Found {ActiveCount} active instances out of {TotalCount} potential instances", 
                activeInstances.Count, potentialInstances.Count());

            return activeInstances;
        }

        // This method is kept for backward compatibility but is deprecated
        private async Task<List<Shift>> GetCurrentlyRunningShifts(TimeOnly currentTime)
        {
            _logger.LogWarning("GetCurrentlyRunningShifts is deprecated. Use GetCurrentlyActiveInstancesAsync instead.");
            
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var utcNow = DateTime.UtcNow;
            
            // Get all shifts that are within their date range
            var shifts = await _context.Shifts
                .Where(s => !s.IsDeleted &&
                    s.StartDate <= utcNow &&
                    (s.EndDate == null || s.EndDate >= utcNow.Date))
                .ToListAsync();
                
            // Filter for shifts that should be active now in Nairobi time
            return shifts.Where(s => 
            {
                var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nairobiTimeZone);
                var nairobiTime = nairobiNow.TimeOfDay;
                
                if (s.StartTime < s.EndTime)
                {
                    // Normal shift (not overnight)
                    return nairobiTime >= s.StartTime && nairobiTime <= s.EndTime;
                }
                
                // Overnight shift
                return nairobiTime >= s.StartTime || nairobiTime <= s.EndTime;
            }).ToList();
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