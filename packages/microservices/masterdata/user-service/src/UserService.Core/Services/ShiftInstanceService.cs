using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Utilities;

namespace UserService.Core.Services;

public class ShiftInstanceService : IShiftInstanceService
{
    private readonly IShiftInstanceRepository _shiftInstanceRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ShiftInstanceService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const int MaxInstances = 1000; // Prevent infinite loops and limit the number of instances

    public ShiftInstanceService(
        IShiftInstanceRepository shiftInstanceRepository,
        IMapper mapper,
        IShiftRepository shiftRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ShiftInstanceService> logger)
    {
        _shiftInstanceRepository = shiftInstanceRepository ?? throw new ArgumentNullException(nameof(shiftInstanceRepository));
        _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }


    public async Task<IEnumerable<ShiftInstanceResponse>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all shift instances");
        var instances = await _shiftInstanceRepository.GetAllAsync();
        var result = _mapper.Map<IEnumerable<ShiftInstanceResponse>>(instances);
        _logger.LogInformation("Retrieved {Count} shift instances", result.Count());
        return result;
    }

    public async Task<ShiftInstanceResponse?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Shift instance ID cannot be null or empty", nameof(id));

        _logger.LogInformation("Retrieving shift instance {ShiftInstanceId}", id);
        var instance = await _shiftInstanceRepository.GetByIdAsync(id);
        if (instance == null)
        {
            _logger.LogWarning("Shift instance {ShiftInstanceId} not found", id);
            return null;
        }

        return _mapper.Map<ShiftInstanceResponse>(instance);
    }

    public async Task<ShiftInstanceResponse> CreateAsync(ShiftInstance shiftInstance)
    {
        if (shiftInstance == null)
            throw new ArgumentNullException(nameof(shiftInstance));

        _logger.LogInformation("Creating shift instance for shift {ShiftId} on {ScheduledDate}", 
            shiftInstance.ShiftId, shiftInstance.ScheduledDate);

        // Validate shift existence
        var shift = await _shiftRepository.GetByIdAsync(shiftInstance.ShiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {shiftInstance.ShiftId} not found");

        var createdInstance = await _shiftInstanceRepository.CreateAsync(shiftInstance);
        var result = _mapper.Map<ShiftInstanceResponse>(createdInstance);
        
        _logger.LogInformation("Created shift instance {ShiftInstanceId}", createdInstance.Id);
        return result;
    }

    public async Task<ShiftInstanceResponse?> UpdateAsync(string id, ShiftInstance shiftInstance)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Shift instance ID cannot be null or empty", nameof(id));
        if (shiftInstance == null)
            throw new ArgumentNullException(nameof(shiftInstance));
        if (id != shiftInstance.Id)
            throw new ArgumentException("Shift instance ID in the payload does not match the provided ID");

        _logger.LogInformation("Updating shift instance {ShiftInstanceId}", id);
        
        // Validate shift existence
        var shift = await _shiftRepository.GetByIdAsync(shiftInstance.ShiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {shiftInstance.ShiftId} not found");

        var updatedInstance = await _shiftInstanceRepository.UpdateAsync(shiftInstance);
        if (updatedInstance == null)
        {
            _logger.LogWarning("Shift instance {ShiftInstanceId} not found for update", id);
            return null;
        }

        var result = _mapper.Map<ShiftInstanceResponse>(updatedInstance);
        _logger.LogInformation("Updated shift instance {ShiftInstanceId}", id);
        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Shift instance ID cannot be null or empty", nameof(id));

        _logger.LogInformation("Deleting shift instance {ShiftInstanceId}", id);
        var success = await _shiftInstanceRepository.DeleteAsync(id);
        if (!success)
        {
            _logger.LogWarning("Failed to delete shift instance {ShiftInstanceId}", id);
        }
        return success;
    }

    public async Task<IEnumerable<ShiftInstanceResponse>> GenerateInstancesAsync(ShiftInstanceGenerateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.StartDate > request.EndDate)
            throw new ArgumentException("Start date cannot be after end date");

        // Validate shift existence
        var shift = await _shiftRepository.GetByIdAsync(request.ShiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {request.ShiftId} not found");

        var instances = new List<ShiftInstance>();
        var currentDate = request.StartDate.Date; // Start from the beginning of the start date
        var endDate = request.EndDate?.Date ?? DateTime.MaxValue; // Use provided end date or a very far future date if null
        int instanceCount = 0;
        
        _logger.LogInformation("Generating shift instances for shift {ShiftId} from {StartDate} to {EndDate}", 
            request.ShiftId, request.StartDate, request.EndDate);

        // If this is a weekly recurrence with custom days, handle it specially
        if (request.RecurrenceType == RecurrenceType.Weekly && request.CustomDays != null && request.CustomDays.Length > 0)
        {
            _logger.LogInformation("Processing weekly recurrence with custom days: {Days}", 
                string.Join(", ", request.CustomDays.Select(d => d.ToString())));
                
            // Process week by week
            while (currentDate <= endDate && instanceCount < MaxInstances)
            {
                // For each custom day in the week
                foreach (var dayOfWeek in request.CustomDays)
                {
                    // Skip if we've reached the maximum number of instances
                    if (instanceCount >= MaxInstances) break;
                    
                    // Calculate the date for this day of the week in the current week
                    var dayDate = GetDateForDayOfWeek(currentDate, dayOfWeek);
                    
                    // Skip if this date is before the start date or after the end date
                    if (dayDate < request.StartDate.Date || dayDate > endDate)
                        continue;
                        
                    // Skip if this date is in the exception dates
                    if (request.ExceptionDates != null && request.ExceptionDates.Contains(dayDate))
                        continue;
                        
                    // Create the instance for this day
                    await CreateAndAddInstance(request, instances, dayDate);
                    instanceCount++;
                }
                
                // Move to the next week
                currentDate = currentDate.AddDays(7 * (request.RecurrenceInterval > 0 ? request.RecurrenceInterval : 1));
            }
        }
        else
        {
            // Original logic for non-weekly or weekly without custom days
            while (currentDate <= endDate && instanceCount < MaxInstances)
            {
                // Skip if current date is in exception dates
                if (request.ExceptionDates != null && request.ExceptionDates.Contains(currentDate.Date))
                {
                    currentDate = GetNextRecurrenceDate(shift, currentDate);
                    continue;
                }
                
                // Skip if current day of week is not in custom days (if specified and not weekly)
                if (request.CustomDays != null && request.CustomDays.Length > 0 && 
                    !request.CustomDays.Contains(currentDate.DayOfWeek))
                {
                    currentDate = GetNextRecurrenceDate(shift, currentDate);
                    continue;
                }
                
                // Create the instance
                await CreateAndAddInstance(request, instances, currentDate);
                instanceCount++;
                
                // Move to next occurrence
                currentDate = GetNextRecurrenceDate(shift, currentDate);
            }
        }
        
        // Save all instances in batches to avoid large transactions
        const int batchSize = 100;
        var savedInstances = new List<ShiftInstance>();
        
        for (int i = 0; i < instances.Count; i += batchSize)
        {
            var batch = instances.Skip(i).Take(batchSize).ToList();
            await _shiftInstanceRepository.AddRangeAsync(batch);
            savedInstances.AddRange(batch);
        }
        
        _logger.LogInformation("Generated {Count} shift instances for shift {ShiftId}", 
            savedInstances.Count, request.ShiftId);
            
        return _mapper.Map<IEnumerable<ShiftInstanceResponse>>(savedInstances);
    }

    private DateTime GetNextRecurrenceDate(Shift shift, DateTime currentDate)
    {
        // Ensure we have valid values
        var recurrenceType = shift.RecurrenceType;
        var interval = shift.RecurrenceInterval > 0 ? shift.RecurrenceInterval : 1;

        return recurrenceType switch
        {
            RecurrenceType.Weekly => currentDate.AddDays(7 * interval),
            RecurrenceType.Monthly => currentDate.AddMonths(interval),
            _ => currentDate.AddDays(interval) 
        };
    }
    
    private DateTime GetDateForDayOfWeek(DateTime currentDate, DayOfWeek targetDay)
    {
        int currentDayOfWeek = (int)currentDate.DayOfWeek;
        int targetDayOfWeek = (int)targetDay;
        
        // Calculate days to add to get to the target day of the week
        int daysToAdd = (7 + (targetDayOfWeek - currentDayOfWeek)) % 7;
        
        // If the target day is the same as current day, return next week's date
        if (daysToAdd == 0 && currentDate.TimeOfDay.TotalSeconds > 0)
        {
            daysToAdd = 7;
        }
        
        return currentDate.Date.AddDays(daysToAdd);
    }
    
    private Task CreateAndAddInstance(ShiftInstanceGenerateRequest request, List<ShiftInstance> instances, DateTime currentDate)
    {
        // Get Nairobi timezone
        var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        
        // Create local date time in Nairobi timezone
        var localStartDateTime = DateTime.SpecifyKind(currentDate.Date.Add(request.StartTime), DateTimeKind.Unspecified);
        var localEndDateTime = DateTime.SpecifyKind(currentDate.Date.Add(request.EndTime), DateTimeKind.Unspecified);
        
        // Handle overnight shifts (where end time is on the next day)
        if (request.EndTime < request.StartTime)
        {
            localEndDateTime = localEndDateTime.AddDays(1);
        }

        // Convert local Nairobi time to UTC for storage
        var startDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localStartDateTime, DateTimeKind.Unspecified), nairobiTimeZone);
        var endDateTime = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localEndDateTime, DateTimeKind.Unspecified), nairobiTimeZone);

        // Create shift instance with UTC times
        var instance = new ShiftInstance
        {
            Id = Guid.NewGuid().ToString(),
            ShiftId = request.ShiftId,
            ScheduledDate = currentDate.Date,
            ScheduledStartTime = startDateTime,
            ScheduledEndTime = endDateTime,
            Status = ShiftInstanceStatus.Scheduled,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy ?? AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User) ?? "System",
            UpdatedBy = request.UpdatedBy ?? AuthUtils.GetUserIdFromClaims(_httpContextAccessor.HttpContext?.User) ?? "System"
        };
        
        _logger.LogDebug("Created instance for {Date} from {LocalStartTime} to {LocalEndTime} (UTC: {UtcStartTime} to {UtcEndTime})", 
            currentDate.Date, 
            localStartDateTime, 
            localEndDateTime,
            startDateTime,
            endDateTime);
        
        instances.Add(instance);
        _logger.LogDebug("Created instance for {Date} from {StartTime} to {EndTime}",
            currentDate.Date, startDateTime, endDateTime);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ShiftInstanceResponse>> GetInstancesByShiftAsync(string shiftId)
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            throw new ArgumentException("Shift ID cannot be null or empty", nameof(shiftId));

        _logger.LogInformation("Retrieving shift instances for shift {ShiftId}", shiftId);
        var instances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shiftId);
        var result = _mapper.Map<IEnumerable<ShiftInstanceResponse>>(instances);
        _logger.LogInformation("Retrieved {Count} shift instances for shift {ShiftId}", result.Count(), shiftId);
        return result;
    }

    public async Task<IEnumerable<ShiftInstanceResponse>> GetInstancesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be after end date");

        _logger.LogInformation("Retrieving shift instances from {StartDate} to {EndDate}", startDate, endDate);
        var instances = await _shiftInstanceRepository.GetInstancesByDateRangeAsync(startDate, endDate);
        var result = _mapper.Map<IEnumerable<ShiftInstanceResponse>>(instances);
        _logger.LogInformation("Retrieved {Count} shift instances for date range", result.Count());
        return result;
    }

    public async Task<IEnumerable<ShiftInstanceResponse>> GetUpcomingInstancesAsync(int days = 7)
    {
        if (days < 1)
            throw new ArgumentException("Days must be greater than 0", nameof(days));

        _logger.LogInformation("Retrieving upcoming shift instances for the next {Days} days", days);
        var instances = await _shiftInstanceRepository.GetUpcomingInstancesAsync(days);
        var result = _mapper.Map<IEnumerable<ShiftInstanceResponse>>(instances);
        _logger.LogInformation("Retrieved {Count} upcoming shift instances", result.Count());
        return result;
    }

    public async Task<bool> CancelInstanceAsync(string shiftInstanceId, string reason)
    {
        if (string.IsNullOrWhiteSpace(shiftInstanceId))
            throw new ArgumentException("Shift instance ID cannot be null or empty", nameof(shiftInstanceId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Cancellation reason cannot be null or empty", nameof(reason));

        _logger.LogInformation("Cancelling shift instance {ShiftInstanceId} with reason: {Reason}", shiftInstanceId, reason);
        var success = await _shiftInstanceRepository.CancelShiftInstanceAsync(shiftInstanceId, reason);
        if (!success)
        {
            _logger.LogWarning("Failed to cancel shift instance {ShiftInstanceId}", shiftInstanceId);
        }
        return success;
    }

    // Shift instance status management is now handled by ShiftInstanceBackgroundService
    // which automatically starts and completes shift instances based on their scheduled times

    public async Task<ShiftInstanceResponse?> GetCurrentActiveInstanceAsync(string shiftId)
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            throw new ArgumentException("Shift ID cannot be null or empty", nameof(shiftId));

        _logger.LogInformation("Retrieving current active shift instance for shift {ShiftId}", shiftId);
        
        // Validate shift existence
        var shift = await _shiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {shiftId} not found");

        var currentTime = DateTime.UtcNow;
        var instance = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shiftId)
            .ContinueWith(task => task.Result
                .FirstOrDefault(si => si.Status == ShiftInstanceStatus.InProgress &&
                                     si.ScheduledStartTime <= currentTime &&
                                     si.ScheduledEndTime >= currentTime));

        if (instance == null)
        {
            _logger.LogInformation("No active shift instance found for shift {ShiftId}", shiftId);
            return null;
        }

        var result = _mapper.Map<ShiftInstanceResponse>(instance);
        _logger.LogInformation("Found active shift instance {ShiftInstanceId} for shift {ShiftId}", 
            instance.Id, shiftId);
        return result;
    }
    public async Task<IEnumerable<ShiftInstanceResponse>> UpdateShiftInstancesAsync(string shiftId, ShiftInstanceGenerateRequest request)
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            throw new ArgumentException("Shift ID cannot be null or empty", nameof(shiftId));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var shift = await _shiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {shiftId} not found");

        _logger.LogInformation("Updating shift instances for shift {ShiftId}", shiftId);

        var existingInstances = (await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shiftId)).ToList();
        var currentTime = DateTime.UtcNow;

        var pastInstances = existingInstances.Where(i => i.ScheduledEndTime < currentTime).ToList();
        var currentInstances = existingInstances.Where(i =>
            i.ScheduledStartTime <= currentTime &&
            i.ScheduledEndTime >= currentTime &&
            i.Status == ShiftInstanceStatus.InProgress).ToList();
        var scheduledInstances = existingInstances.Where(i => i.Status == ShiftInstanceStatus.Scheduled).ToList();

        _logger.LogInformation("Found {PastCount} past, {CurrentCount} current, {ScheduledCount} scheduled instances",
            pastInstances.Count, currentInstances.Count, scheduledInstances.Count);

        // Delete all scheduled instances (includes stuck past-Scheduled ones)
        if (scheduledInstances.Any())
        {
            _logger.LogInformation("Deleting {Count} scheduled instances", scheduledInstances.Count);
            foreach (var instance in scheduledInstances)
                await _shiftInstanceRepository.DeleteAsync(instance.Id);
        }

        // Determine generation window using Nairobi local time
        var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var currentTimeInNairobi = TimeZoneInfo.ConvertTimeFromUtc(currentTime, nairobiTimeZone);
        var currentDateInNairobi = currentTimeInNairobi.Date;
        var startTimeTodayInNairobi = currentDateInNairobi.Add(request.StartTime);

        var startDate = request.StartDate.Kind != DateTimeKind.Utc ? request.StartDate.ToUniversalTime() : request.StartDate;
        var endDate = request.EndDate.HasValue
            ? (request.EndDate.Value.Kind != DateTimeKind.Utc ? request.EndDate.Value.ToUniversalTime() : request.EndDate.Value)
            : (shift.EndDate ?? DateTime.UtcNow.AddYears(1));

        var generateStartDate = startTimeTodayInNairobi > currentTimeInNairobi
            ? currentDateInNairobi
            : currentDateInNairobi.AddDays(1);

        if (generateStartDate < startDate.Date)
            generateStartDate = startDate.Date;

        _logger.LogInformation("Generating instances from {GenerateStartDate} to {EndDate} (Nairobi time: {NairobiTime})",
            generateStartDate, endDate, currentTimeInNairobi);

        if (generateStartDate <= endDate)
        {
            var newInstances = await _shiftInstanceRepository.GenerateShiftInstancesAsync(shiftId, generateStartDate, endDate);
            _logger.LogInformation("Generated {Count} new instances for shift {ShiftId}", newInstances.Count(), shiftId);
        }
        else
        {
            _logger.LogInformation("No new instances to generate — start {Start} is after end {End}", generateStartDate, endDate);
        }

        // Clean up any instances that landed on exception dates
        var cleanedUp = await _shiftInstanceRepository.CleanupInstancesOnExceptionDatesAsync(shiftId);
        if (cleanedUp > 0)
            _logger.LogInformation("Cleaned up {Count} instances on exception dates for shift {ShiftId}", cleanedUp, shiftId);

        // Return all surviving instances ordered by date
        var allInstances = await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shiftId);
        return _mapper.Map<IEnumerable<ShiftInstanceResponse>>(allInstances.OrderBy(i => i.ScheduledDate));
    }

    public async Task<int> CleanupInstancesOnExceptionDatesAsync(string shiftId)
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            throw new ArgumentException("Shift ID cannot be null or empty", nameof(shiftId));

        _logger.LogInformation("Cleaning up instances on exception dates for shift {ShiftId}", shiftId);
        var count = await _shiftInstanceRepository.CleanupInstancesOnExceptionDatesAsync(shiftId);
        _logger.LogInformation("Cleaned up {Count} instances on exception dates for shift {ShiftId}", count, shiftId);
        return count;
    }

    public async Task<int> CancelPastExceptionInstancesAsync(string shiftId, string reason = "Exception date added retroactively")
    {
        if (string.IsNullOrWhiteSpace(shiftId))
            throw new ArgumentException("Shift ID cannot be null or empty", nameof(shiftId));

        _logger.LogInformation("Cancelling past instances on exception dates for shift {ShiftId}", shiftId);

        var shift = await _shiftRepository.GetByIdAsync(shiftId);
        if (shift == null)
            throw new ArgumentException($"Shift with ID {shiftId} not found");

        if (shift.ExceptionDates == null || shift.ExceptionDates.Length == 0)
            return 0;

        var exceptionSet = shift.ExceptionDates.Select(d => d.Date).ToHashSet();
        var currentTime = DateTime.UtcNow;

        var allInstances = (await _shiftInstanceRepository.GetInstancesByShiftIdAsync(shiftId)).ToList();
        var pastExceptionInstances = allInstances
            .Where(i => i.ScheduledEndTime < currentTime &&
                        exceptionSet.Contains(i.ScheduledDate.Date) &&
                        i.Status != ShiftInstanceStatus.Cancelled &&
                        i.Status != ShiftInstanceStatus.Completed)
            .ToList();

        int cancelledCount = 0;
        foreach (var instance in pastExceptionInstances)
        {
            var success = await _shiftInstanceRepository.CancelShiftInstanceAsync(instance.Id, reason);
            if (success) cancelledCount++;
        }

        _logger.LogInformation("Cancelled {Count} past instances on exception dates for shift {ShiftId}", cancelledCount, shiftId);
        return cancelledCount;
    }
}