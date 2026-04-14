using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
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

        public Task<ShiftInstance?> GetByIdAsync(string id, bool b)
        {
            throw new NotImplementedException();
        }

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
            if (shiftInstance == null)
                throw new ArgumentNullException(nameof(shiftInstance));

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
            if (shiftInstance == null)
                throw new ArgumentNullException(nameof(shiftInstance));

            var existingInstance = await _context.ShiftInstances
                .AsTracking()
                .FirstOrDefaultAsync(si => si.Id == shiftInstance.Id && !si.IsDeleted);

            if (existingInstance == null)
                return null;

            // Update specific fields
            existingInstance.ScheduledDate = shiftInstance.ScheduledDate;
            existingInstance.ScheduledStartTime = shiftInstance.ScheduledStartTime;
            existingInstance.ScheduledEndTime = shiftInstance.ScheduledEndTime;
            existingInstance.Status = shiftInstance.Status;
            existingInstance.Notes = shiftInstance.Notes;
            existingInstance.UpdatedAt = DateTime.UtcNow;
            existingInstance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

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
                    var shiftInstance = await _context.ShiftInstances
                        .AsTracking()
                        .FirstOrDefaultAsync(si => si.Id == id);

                    if (shiftInstance == null)
                    {
                        _logger.LogWarning("Shift instance {ShiftInstanceId} not found for deletion", id);
                        return false;
                    }

                    if (shiftInstance.IsDeleted)
                    {
                        _logger.LogWarning("Shift instance {ShiftInstanceId} already marked as deleted", id);
                        return false;
                    }

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
            var statusSet = statuses.ToHashSet();
            return await _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => statusSet.Contains(si.Status) && !si.IsDeleted)
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
            ShiftInstanceStatus status, 
            DateTime startTime, 
            DateTime endTime)
        {
            // First get all instances with the specified status
            var query = _context.ShiftInstances
                .Include(si => si.Shift)
                .Include(si => si.Attendances)
                .Where(si => !si.IsDeleted && si.Status == status);

            // Materialize the query to avoid EF Core translation issues with complex date/time operations
            var allInstances = await query.ToListAsync();

            // Now filter in memory where ScheduledDate + ScheduledStartTime is within the time range
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

                    // Check if shift is one-time or recurring
                    if (shift.Type == ShiftType.OneTime)
                    {
                        // For one-time shifts, create a single instance on the start date
                        var existingInstance = await _context.ShiftInstances
                            .FirstOrDefaultAsync(si => si.ShiftId == shiftId && 
                                                      si.ScheduledDate.Date == startDate.Date &&
                                                      !si.IsDeleted);

                        if (existingInstance == null)
                        {
                            var instance = CreateShiftInstance(shift, startDate, currentUserId);
                            await _context.ShiftInstances.AddAsync(instance);
                            generatedInstances.Add(instance);
                        }
                    }
                    else if (shift.Type == ShiftType.Recurring)
                    {
                        var currentDate = startDate.Date;
                        
                        while (currentDate <= endDate.Date)
                        {
                            var shouldCreateInstance = ShouldCreateInstanceForDate(shift, currentDate);

                            if (shouldCreateInstance)
                            {
                                // Check if instance already exists
                                var existingInstance = await _context.ShiftInstances
                                    .FirstOrDefaultAsync(si => si.ShiftId == shiftId && 
                                                              si.ScheduledDate.Date == currentDate &&
                                                              !si.IsDeleted);

                                if (existingInstance == null)
                                {
                                    var instance = CreateShiftInstance(shift, currentDate, currentUserId);
                                    await _context.ShiftInstances.AddAsync(instance);
                                    generatedInstances.Add(instance);
                                }
                            }

                            currentDate = GetNextDate(shift, currentDate);
                            
                            // Prevent infinite loops
                            if (currentDate <= startDate.Date)
                                break;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Generated {Count} shift instances for shift {ShiftId} between {StartDate} and {EndDate}", 
                        generatedInstances.Count, shiftId, startDate, endDate);

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

                    if (shiftInstance == null)
                    {
                        _logger.LogWarning("Shift instance {ShiftInstanceId} not found for cancellation", shiftInstanceId);
                        return false;
                    }

                    if (shiftInstance.Status == ShiftInstanceStatus.Cancelled)
                    {
                        _logger.LogWarning("Shift instance {ShiftInstanceId} already cancelled", shiftInstanceId);
                        return true; // Already cancelled
                    }

                    if (shiftInstance.Status == ShiftInstanceStatus.Completed)
                    {
                        _logger.LogWarning("Cannot cancel completed shift instance {ShiftInstanceId}", shiftInstanceId);
                        return false;
                    }

                    shiftInstance.Status = ShiftInstanceStatus.Cancelled;
                    shiftInstance.Notes = string.IsNullOrEmpty(shiftInstance.Notes) 
                        ? $"Cancelled: {reason}" 
                        : $"{shiftInstance.Notes}; Cancelled: {reason}";
                    shiftInstance.UpdatedAt = DateTime.UtcNow;
                    shiftInstance.UpdatedBy = AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Cancelled shift instance {ShiftInstanceId} with reason: {Reason}", 
                        shiftInstanceId, reason);
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
            if (batch == null || !batch.Any())
                return;

            await _context.ShiftInstances.AddRangeAsync(batch);
            await _context.SaveChangesAsync();
        }

        private ShiftInstance CreateShiftInstance(Shift shift, DateTime date, string? currentUserId)
        {
            var scheduledStartTime = date.Date.Add(shift.StartTime);
            var scheduledEndTime = date.Date.Add(shift.EndTime);
            
            // Handle overnight shifts
            if (shift.EndTime < shift.StartTime)
            {
                scheduledEndTime = scheduledEndTime.AddDays(1);
            }

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
            // Check if date is in exception dates
            if (shift.ExceptionDates.Contains(date.Date))
                return false;

            // Check if shift has ended
            if (shift.EndDate.HasValue && date > shift.EndDate.Value)
                return false;

            // Check if date is before shift start
            if (date < shift.StartDate.Date)
                return false;

            return shift.RecurrenceType switch
            {
                RecurrenceType.None => date.Date == shift.StartDate.Date,
                RecurrenceType.Daily => true,
                RecurrenceType.Weekly => IsValidWeeklyDate(shift, date),
                RecurrenceType.Monthly => IsValidMonthlyDate(shift, date),
                RecurrenceType.Custom => shift.CustomDays.Contains(date.DayOfWeek),
                _ => false
            };
        }

        private bool IsValidWeeklyDate(Shift shift, DateTime date)
        {
            var daysSinceStart = (date.Date - shift.StartDate.Date).Days;
            return daysSinceStart % (shift.RecurrenceInterval * 7) == 0;
        }

        private bool IsValidMonthlyDate(Shift shift, DateTime date)
        {
            var monthsSinceStart = (date.Year - shift.StartDate.Year) * 12 + 
                                   (date.Month - shift.StartDate.Month);
            
            return monthsSinceStart % shift.RecurrenceInterval == 0 && 
                   date.Day == shift.StartDate.Day;
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
            var maxDays = 14; // Prevent infinite loops
            var dayCount = 0;

            while (dayCount < maxDays)
            {
                if (shift.CustomDays.Contains(nextDate.DayOfWeek))
                    return nextDate;
                    
                nextDate = nextDate.AddDays(1);
                dayCount++;
            }

            return currentDate.AddDays(7); // Fallback
        }
        
    }
}