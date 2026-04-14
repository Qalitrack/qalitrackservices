using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Utilities;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class ShiftInstanceRepository : Repository<ShiftInstance>, IShiftInstanceRepository
    {
        private readonly UserServiceDbContext _context;
        private new readonly ILogger<ShiftInstanceRepository> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShiftInstanceRepository(
            UserServiceDbContext context,
            ILogger<ShiftInstanceRepository> logger,
            IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor, logger)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // === Existing methods (kept mostly as-is, with minor cleanups) ===

        public async Task<IEnumerable<ShiftInstance>> GetAllAsync()
        {
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                    .ThenInclude(a => a.Employee)
                .Where(si => !si.IsDeleted)
                .OrderBy(si => si.ScheduledDate)
                .ToListAsync();
        }

        public Task<ShiftInstance?> GetByIdAsync(string id, bool b) => throw new NotImplementedException();

        public async Task<ShiftInstance?> GetByIdAsync(string id)
        {
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                    .ThenInclude(a => a.Employee)
                .Include(si => si.Notifications)
                .FirstOrDefaultAsync(si => si.Id == id && !si.IsDeleted);
        }

        public new async Task<ShiftInstance> CreateAsync(ShiftInstance shiftInstance)
        {
            if (shiftInstance == null) throw new ArgumentNullException(nameof(shiftInstance));

            shiftInstance.CreatedAt = DateTime.UtcNow;
            shiftInstance.UpdatedAt = DateTime.UtcNow;
            shiftInstance.CreatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);
            shiftInstance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            await _context.ShiftInstances.AddAsync(shiftInstance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created shift instance {ShiftInstanceId} for shift {ShiftId} on {ScheduledDate}", 
                shiftInstance.Id, shiftInstance.ShiftId, shiftInstance.ScheduledDate);

            return shiftInstance;
        }

        public new async Task<ShiftInstance?> UpdateAsync(ShiftInstance shiftInstance)
        {
            if (shiftInstance == null) throw new ArgumentNullException(nameof(shiftInstance));

            var existingInstance = await _context.ShiftInstances
                .AsTracking()
                .FirstOrDefaultAsync(si => si.Id == shiftInstance.Id && !si.IsDeleted);

            if (existingInstance == null) return null;

            existingInstance.ScheduledDate = shiftInstance.ScheduledDate;
            existingInstance.ScheduledStartTime = shiftInstance.ScheduledStartTime;
            existingInstance.ScheduledEndTime = shiftInstance.ScheduledEndTime;
            existingInstance.Status = shiftInstance.Status;
            existingInstance.Notes = shiftInstance.Notes;
            existingInstance.UpdatedAt = DateTime.UtcNow;
            existingInstance.UpdatedBy = shiftInstance.UpdatedBy 
                ?? AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated shift instance {ShiftInstanceId}", shiftInstance.Id);
            return existingInstance;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var shiftInstance = await _context.ShiftInstances.AsTracking()
                        .FirstOrDefaultAsync(si => si.Id == id);

                    if (shiftInstance == null || shiftInstance.IsDeleted)
                        return false;

                    shiftInstance.IsDeleted = true;
                    shiftInstance.UpdatedAt = DateTime.UtcNow;
                    shiftInstance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Soft deleted shift instance {ShiftInstanceId}", id);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error deleting shift instance {ShiftInstanceId}", id);
                    throw;
                }
            });
        }

        // === Other Get methods (unchanged for brevity - keep your original implementations) ===
        // GetInstancesByShiftIdAsync, GetInstancesByDateRangeAsync, GetInstancesByStatusAsync, etc.

        public async Task<IEnumerable<ShiftInstance>> GetInstancesByShiftIdAsync(string shiftId)
        {
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => si.ShiftId == shiftId && !si.IsDeleted)
                .OrderBy(si => si.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftInstance>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                    .ThenInclude(a => a.Employee)
                .Where(si => !si.IsDeleted &&
                            si.ScheduledDate >= startDate.Date &&
                            si.ScheduledDate <= endDate.Date)
                .OrderBy(si => si.ScheduledDate)
                .ThenBy(si => si.ScheduledStartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAsync(ShiftInstanceStatus status)
        {
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => si.Status == status && !si.IsDeleted)
                .OrderBy(si => si.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftInstance>> GetInstancesByStatusesAsync(IEnumerable<ShiftInstanceStatus> statuses)
        {
            var statusList = statuses.ToList();
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => statusList.Contains(si.Status) && !si.IsDeleted)
                .OrderBy(si => si.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftInstance>> GetUpcomingInstancesAsync(int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(days);

            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => !si.IsDeleted &&
                            si.ScheduledDate >= today &&
                            si.ScheduledDate <= endDate)
                .OrderBy(si => si.ScheduledDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftInstance>> GetInstancesByStatusAndTimeAsync(
            ShiftInstanceStatus status, DateTime startTime, DateTime endTime)
        {
            var query = _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => !si.IsDeleted && si.Status == status);

            var allInstances = await query.ToListAsync();

            var filteredInstances = allInstances
                .Where(si => 
                {
                    var scheduledDateTime = si.ScheduledDate.Date.Add(si.ScheduledStartTime.TimeOfDay);
                    return scheduledDateTime >= startTime && scheduledDateTime <= endTime;
                })
                .OrderBy(si => si.ScheduledDate)
                .ThenBy(si => si.ScheduledStartTime)
                .ToList();

            _logger.LogInformation("Found {Count} instances matching the criteria", filteredInstances.Count);
            return filteredInstances;
        }

        // === NEW & IMPROVED METHODS ===

        public async Task<IEnumerable<ShiftInstance>> GenerateShiftInstancesAsync(string shiftId, DateTime startDate, DateTime endDate)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var shift = await _context.Shifts
                        .FirstOrDefaultAsync(s => s.Id == shiftId && !s.IsDeleted);

                    if (shift == null)
                    {
                        _logger.LogWarning("Shift {ShiftId} not found for instance generation", shiftId);
                        return new List<ShiftInstance>();
                    }

                    var generatedInstances = new List<ShiftInstance>();
                    var currentUserId = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    if (shift.Type == ShiftType.OneTime)
                    {
                        var existing = await _context.ShiftInstances
                            .AnyAsync(si => si.ShiftId == shiftId 
                                         && si.ScheduledDate.Date == startDate.Date 
                                         && !si.IsDeleted);

                        if (!existing)
                        {
                            var instance = CreateShiftInstance(shift, startDate.Date, currentUserId);
                            await _context.ShiftInstances.AddAsync(instance);
                            generatedInstances.Add(instance);
                        }
                    }
                    else if (shift.Type == ShiftType.Recurring)
                    {
                        var exceptionSet = shift.ExceptionDates.Length > 0 
                            ? shift.ExceptionDates.Select(d => d.Date).ToHashSet() 
                            : new HashSet<DateTime>();

                        var currentDate = startDate.Date;

                        while (currentDate <= endDate.Date)
                        {
                            if (!exceptionSet.Contains(currentDate) && ShouldCreateInstanceForDate(shift, currentDate))
                            {
                                var existing = await _context.ShiftInstances
                                    .AnyAsync(si => si.ShiftId == shiftId 
                                                 && si.ScheduledDate.Date == currentDate 
                                                 && !si.IsDeleted);

                                if (!existing)
                                {
                                    var instance = CreateShiftInstance(shift, currentDate, currentUserId);
                                    await _context.ShiftInstances.AddAsync(instance);
                                    generatedInstances.Add(instance);
                                }
                            }

                            currentDate = GetNextDate(shift, currentDate);
                            if (currentDate <= startDate.Date) break; // prevent infinite loop
                        }
                    }

                    await _context.SaveChangesAsync();

                    // Automatic cleanup after generation
                    if (shift.Type == ShiftType.Recurring && shift.ExceptionDates.Length > 0)
                    {
                        await CleanupInstancesOnExceptionDatesAsync(shiftId);
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation("Generated {Count} shift instances for shift {ShiftId}", 
                        generatedInstances.Count, shiftId);

                    return generatedInstances;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error generating shift instances for shift {ShiftId}", shiftId);
                    throw;
                }
            });
        }

        public async Task<int> CleanupInstancesOnExceptionDatesAsync(string shiftId)
        {
            var shift = await _context.Shifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == shiftId && !s.IsDeleted);

            if (shift == null || shift.ExceptionDates.Length == 0)
                return 0;

            var exceptionSet = shift.ExceptionDates.Select(d => d.Date).ToHashSet();

            // Only remove Scheduled instances — Completed/InProgress/Cancelled are kept as historical records
            var instancesToCleanup = await _context.ShiftInstances
                .Where(si => si.ShiftId == shiftId
                          && !si.IsDeleted
                          && si.Status == ShiftInstanceStatus.Scheduled
                          && exceptionSet.Contains(si.ScheduledDate.Date))
                .ToListAsync();

            if (instancesToCleanup.Count == 0)
                return 0;

            var currentUserId = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

            foreach (var instance in instancesToCleanup)
            {
                instance.IsDeleted = true;
                instance.UpdatedAt = DateTime.UtcNow;
                instance.UpdatedBy = currentUserId;
                instance.Notes = string.IsNullOrEmpty(instance.Notes) 
                    ? "Automatically removed due to exception date" 
                    : $"{instance.Notes}; Removed due to exception date";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} shift instances on exception dates for shift {ShiftId}", 
                instancesToCleanup.Count, shiftId);

            return instancesToCleanup.Count;
        }

        public async Task<bool> CancelShiftInstanceAsync(string shiftInstanceId, string reason)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var shiftInstance = await _context.ShiftInstances
                        .AsTracking()
                        .FirstOrDefaultAsync(si => si.Id == shiftInstanceId && !si.IsDeleted);

                    if (shiftInstance == null) return false;
                    if (shiftInstance.Status == ShiftInstanceStatus.Cancelled) return true;
                    if (shiftInstance.Status == ShiftInstanceStatus.Completed) return false;

                    shiftInstance.Status = ShiftInstanceStatus.Cancelled;
                    shiftInstance.Notes = string.IsNullOrEmpty(shiftInstance.Notes) 
                        ? $"Cancelled: {reason}" 
                        : $"{shiftInstance.Notes}; Cancelled: {reason}";
                    shiftInstance.UpdatedAt = DateTime.UtcNow;
                    shiftInstance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Cancelled shift instance {ShiftInstanceId}", shiftInstanceId);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error cancelling shift instance {ShiftInstanceId}", shiftInstanceId);
                    throw;
                }
            });
        }

        public async Task AddRangeAsync(List<ShiftInstance> batch)
        {
            if (batch == null || !batch.Any()) return;

            await _context.ShiftInstances.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
        }

        // === Private Helper Methods ===

        private ShiftInstance CreateShiftInstance(Shift shift, DateTime date, string? currentUserId)
        {
            var scheduledStartTime = date.Date.Add(shift.StartTime);
            var scheduledEndTime = date.Date.Add(shift.EndTime);

            if (shift.EndTime < shift.StartTime)
                scheduledEndTime = scheduledEndTime.AddDays(1);

            return new ShiftInstance
            {
                ShiftId = shift.Id,
                ScheduledDate = date,
                ScheduledStartTime = scheduledStartTime,
                ScheduledEndTime = scheduledEndTime,
                Status = ShiftInstanceStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            };
        }

        private bool ShouldCreateInstanceForDate(Shift shift, DateTime date)
        {
            var checkDate = date.Date;

            // Fast exception date check using HashSet
            if (shift.ExceptionDates.Length > 0)
            {
                var exceptionSet = shift.ExceptionDates.Select(d => d.Date).ToHashSet();
                if (exceptionSet.Contains(checkDate))
                    return false;
            }

            if (checkDate < shift.StartDate.Date)
                return false;

            if (shift.EndDate.HasValue && checkDate > shift.EndDate.Value.Date)
                return false;

            return shift.RecurrenceType switch
            {
                RecurrenceType.None => checkDate == shift.StartDate.Date,
                RecurrenceType.Daily => true,
                RecurrenceType.Weekly => IsValidWeeklyDate(shift, checkDate),
                RecurrenceType.Monthly => IsValidMonthlyDate(shift, checkDate),
                RecurrenceType.Custom => shift.CustomDays.Contains(checkDate.DayOfWeek),
                _ => false
            };
        }

        private bool IsValidWeeklyDate(Shift shift, DateTime date)
        {
            var daysSinceStart = (date - shift.StartDate.Date).Days;
            return daysSinceStart % (shift.RecurrenceInterval * 7) == 0;
        }

        private bool IsValidMonthlyDate(Shift shift, DateTime date)
        {
            var monthsSinceStart = (date.Year - shift.StartDate.Year) * 12 + (date.Month - shift.StartDate.Month);
            return monthsSinceStart % shift.RecurrenceInterval == 0 && date.Day == shift.StartDate.Day;
        }

        private DateTime GetNextDate(Shift shift, DateTime currentDate)
        {
            return shift.RecurrenceType switch
            {
                RecurrenceType.Daily => currentDate.AddDays(shift.RecurrenceInterval),
                RecurrenceType.Weekly => currentDate.AddDays(shift.RecurrenceInterval * 7),
                RecurrenceType.Monthly => currentDate.AddMonths(shift.RecurrenceInterval),
                RecurrenceType.Custom => GetNextCustomDate(shift, currentDate),
                _ => currentDate.AddDays(1)
            };
        }

        private DateTime GetNextCustomDate(Shift shift, DateTime currentDate)
        {
            var nextDate = currentDate.AddDays(1);
            for (int i = 0; i < 14; i++)
            {
                if (shift.CustomDays.Contains(nextDate.DayOfWeek))
                    return nextDate;
                nextDate = nextDate.AddDays(1);
            }
            return currentDate.AddDays(7);
        }
    }
}