using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
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
                _logger.LogInformation("[START] Login restriction check for user {UserId}", userId);
                var currentTime = TimeOnly.FromDateTime(DateTime.UtcNow);

                // 1. Get all currently running shifts (both Strict and Open)
                var runningShifts = await GetCurrentlyRunningShifts(currentTime);

                _logger.LogDebug("Found {Count} running shifts: {Shifts}",
                    runningShifts.Count,
                    string.Join(", ", runningShifts.Select(s => $"{s.Name} (ID: {s.Id}, Mode: {s.Mode})")));

                if (!runningShifts.Any())
                {
                    _logger.LogInformation("[END] No shifts running - access granted");
                    return (true, "No active shifts");
                }

                // 2. Separate Strict and Open shifts
                var strictShifts = runningShifts.Where(s => s.Mode == ShiftMode.Strict).ToList();
                var openShifts = runningShifts.Where(s => s.Mode == ShiftMode.Open).ToList();

                // 3. Get user assignments (like ReportService does)
                var userAssignments = await _context.UserShifts
                    .Include(us => us.Shift)
                    .Where(us => us.UserId == userId)
                    .ToListAsync();

                _logger.LogDebug("User {UserId} has {Count} shift assignments: {Assignments}",
                    userId,
                    userAssignments.Count,
                    string.Join(", ", userAssignments.Select(a => $"{a.ShiftId} (Mode: {a.Shift?.Mode})")));

                // 4. Check strict shift rules first
                if (strictShifts.Any())
                {
                    var assignedToStrictShift = userAssignments
                        .Any(us => strictShifts.Any(s => s.Id == us.ShiftId));

                    if (!assignedToStrictShift)
                    {
                        var shiftNames = string.Join(", ", strictShifts.Select(s => s.Name));
                        _logger.LogWarning("[END] User {UserId} denied login - not assigned to any running strict shifts: {ShiftNames}", 
                            userId, shiftNames);
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
                    
                    _logger.LogInformation("[END] User {UserId} is assigned to running {Mode} shift {ShiftName} - access granted", 
                        userId, assignedShift.Mode == ShiftMode.Strict ? "Strict" : "Open", assignedShift.Name);
                    return (true, $"Assigned to {assignedShift.Mode} shift: {assignedShift.Name}");
                }

                // 6. If not assigned to any running shift
                _logger.LogWarning("[END] User {UserId} denied login - not assigned to any running shifts", userId);
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
    }
}