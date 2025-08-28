using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserService.Core.Entities;
using UserService.Core.Interfaces.Services;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Services
{
    public class ShiftLoginRestrictionService : IShiftLoginRestrictionService
    {
        private readonly UserServiceDbContext _context;
        private readonly ILogger<ShiftLoginRestrictionService> _logger;

        public ShiftLoginRestrictionService(
            UserServiceDbContext context,
            ILogger<ShiftLoginRestrictionService> logger)
        {
            _context = context;
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
                    return (true, "Privileged user - access granted");
                }

                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);

                // 1. Get all currently running shifts (both Strict and Open)
                var runningShifts = await GetCurrentlyRunningShifts(currentTime);
                
                if (!runningShifts.Any())
                {
                    return (true, "No active shifts");
                }

                // 2. Separate Strict and Open shifts
                var strictShifts = runningShifts.Where(s => s.Mode == ShiftMode.Strict).ToList();

                // 3. Get user assignments (like ReportService does)
                var userAssignments = await _context.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == userId)
                    .ToListAsync();
                
                // 4. Check strict shift rules first
                if (strictShifts.Any())
                {
                    var assignedToStrictShift = userAssignments
                        .Any(us => strictShifts.Any(s => s.Id == us.ShiftId));

                    if (!assignedToStrictShift)
                    {
                        var shiftNames = string.Join(", ", strictShifts.Select(s => s.Name));
                        return (false, $"Access denied. Active strict shift(s): {shiftNames}");
                    }
                }

                // 5. Check if assigned to any running shift (including Open shifts)
                var assignedToAnyRunningShift = userAssignments
                    .Any(us => runningShifts.Any(s => s.Id == us.ShiftId));

                if (assignedToAnyRunningShift)
                {
                    var assignedShift = userAssignments
                        .First(us => runningShifts.Any(s => s.Id == us.ShiftId)).Shift;
                    return (true, $"Assigned to {assignedShift.Mode} shift: {assignedShift.Name}");
                }
                
                return (false, "Access denied. Not assigned to any active shifts");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ERROR] in login restriction check for user {UserId}", userId);
                return (true, "System error - access granted");
            }
            finally
            {
                _logger.LogInformation("[TIME] Login restriction check completed in {ElapsedMs}ms", 
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task<List<Shift>> GetCurrentlyRunningShifts(TimeOnly currentTime)
        {
            var currentTimeOfDay = currentTime.ToTimeSpan();
            
            return await _context.Shifts
                .Where(s => 
                    // Normal shift (start < end)
                    (s.StartTime <= s.EndTime && 
                     s.StartTime <= currentTimeOfDay && 
                     s.EndTime >= currentTimeOfDay) ||
                    // Overnight shift (start > end)
                    (s.StartTime > s.EndTime && 
                     (s.StartTime <= currentTimeOfDay || 
                      s.EndTime >= currentTimeOfDay))
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