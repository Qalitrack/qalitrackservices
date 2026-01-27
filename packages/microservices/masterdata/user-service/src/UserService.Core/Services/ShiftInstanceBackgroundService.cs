using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Core.Services;

public class ShiftInstanceBackgroundService : BackgroundService
{
    private readonly ILogger<ShiftInstanceBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    
    // FIXED: Increased intervals to reduce database load
    private readonly TimeSpan _frequentCheckInterval = TimeSpan.FromMinutes(2);    // Changed from 1 to 2 minutes
    private readonly TimeSpan _standardCheckInterval = TimeSpan.FromMinutes(10);   // Changed from 5 to 10 minutes
    private DateTime _lastFrequentCheck = DateTime.MinValue;
    private DateTime _lastStandardCheck = DateTime.MinValue;

    public ShiftInstanceBackgroundService(
        ILogger<ShiftInstanceBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // FIXED: Add startup delay to allow app initialization
        _logger.LogInformation("ShiftInstanceBackgroundService starting with {FrequentInterval}min frequent checks and {StandardInterval}min standard checks", 
            _frequentCheckInterval.TotalMinutes, _standardCheckInterval.TotalMinutes);
        
        await Task.Delay(TimeSpan.FromSeconds(45), stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            bool runFrequentChecks = (now - _lastFrequentCheck) >= _frequentCheckInterval;
            bool runStandardChecks = (now - _lastStandardCheck) >= _standardCheckInterval;

            try
            {
                // FIXED: Single scope per cycle instead of multiple nested scopes
                using var scope = _serviceProvider.CreateScope();
                var shiftInstanceRepository = scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
                var shiftNotificationService = scope.ServiceProvider.GetRequiredService<IShiftNotificationService>();
                var userShiftRepository = scope.ServiceProvider.GetRequiredService<IUserShiftRepository>();

                // Run time-critical checks more frequently
                if (runFrequentChecks)
                {
                    // Check for instances that need to start (time-critical)
                    await CheckForShiftStarts(shiftInstanceRepository, now);
                    
                    // Check for instances that need ending alerts (time-sensitive)
                    await CheckForShiftEndingAlerts(shiftInstanceRepository, shiftNotificationService, userShiftRepository, now);
                    
                    _lastFrequentCheck = now;
                }

                // Run less critical checks less frequently
                if (runStandardChecks)
                {
                    // Check for instances that need reminder notifications (less critical)
                    await CheckForShiftReminders(shiftInstanceRepository, shiftNotificationService, userShiftRepository, now);
                    
                    // Check for instances that need to be marked as completed (less time-critical)
                    await CheckForShiftEnds(shiftInstanceRepository, now);
                    
                    _lastStandardCheck = now;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("ShiftInstanceBackgroundService is stopping...");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift instances");
            }
            
            // FIXED: Better delay calculation and fallback
            try
            {
                var nextFrequentCheck = _lastFrequentCheck.Add(_frequentCheckInterval);
                var nextStandardCheck = _lastStandardCheck.Add(_standardCheckInterval);
                var nextCheck = new[] { nextFrequentCheck, nextStandardCheck }.Min();
                var delay = nextCheck > now ? nextCheck - now : TimeSpan.FromSeconds(30);
                
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
        
        _logger.LogInformation("ShiftInstanceBackgroundService stopped");
    }

    private async Task CheckForShiftReminders(
        IShiftInstanceRepository shiftInstanceRepository, 
        IShiftNotificationService shiftNotificationService,
        IUserShiftRepository userShiftRepository,
        DateTime now)
    {
        try
        {
            var reminderTime = now.AddMinutes(10);
            var instancesNeedingReminders = await shiftInstanceRepository.GetInstancesByStatusAndTimeAsync(
                ShiftInstanceStatus.Scheduled, 
                reminderTime.AddMinutes(-1), 
                reminderTime.AddMinutes(1));

            foreach (var instance in instancesNeedingReminders)
            {
                try
                {
                    // Get users assigned to this shift
                    var assignedUserShifts = await userShiftRepository.GetUsersAssignedToShiftAsync(instance.ShiftId);
                    var activeUsers = assignedUserShifts.Where(us => us.User != null).Select(us => us.User).ToList();

                    if (activeUsers.Any())
                    {
                        var userNotificationData = activeUsers.Select(u => (u.Email, $"{u.FirstName} {u.LastName}")).ToList();
                        
                        await shiftNotificationService.SendShiftNotificationsAsync(
                            userNotificationData,
                            instance.Id,
                            instance.ScheduledStartTime,
                            instance.ScheduledEndTime,
                            instance.Shift?.Name ?? "Unknown Shift",
                            NotificationType.ShiftReminder);

                        _logger.LogInformation("Sent reminder notifications for shift instance {InstanceId} to {UserCount} users", 
                            instance.Id, activeUsers.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending reminder notifications for shift instance {InstanceId}", instance.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckForShiftReminders");
        }
    }

    private async Task CheckForShiftStarts(IShiftInstanceRepository shiftInstanceRepository, DateTime now)
    {
        try
        {
            // Get Nairobi timezone
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(now, nairobiTimeZone);
            var nairobiToday = nairobiNow.Date;
            
            // FIXED: Get only scheduled instances (more efficient query)
            var allScheduledInstances = (await shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.Scheduled))
                .ToList();
            
            if (!allScheduledInstances.Any())
            {
                return; // Early return if no scheduled instances
            }
            
            // Filter instances that should have started by now in Nairobi time
            // Only include instances scheduled for today
            var pastDueInstances = allScheduledInstances
                .Where(instance => 
                {
                    var scheduledDateUtc = instance.ScheduledDate.Date;
                    var scheduledDateInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledDateUtc, nairobiTimeZone).Date;
                    
                    // Only process instances scheduled for today in Nairobi time
                    if (scheduledDateInNairobi != nairobiToday)
                        return false;
                        
                    var scheduledTimeUtc = instance.ScheduledDate.Date.Add(instance.ScheduledStartTime.TimeOfDay);
                    var scheduledTimeInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledTimeUtc, nairobiTimeZone);
                    
                    // Include if the scheduled time has passed in Nairobi time
                    return scheduledTimeInNairobi <= nairobiNow;
                })
                .OrderBy(instance => instance.ScheduledDate)
                .ThenBy(instance => instance.ScheduledStartTime)
                .ToList();
            
            // Get shifts starting soon (in the next check interval) in Nairobi time
            var upcomingInstances = allScheduledInstances
                .Where(instance => 
                {
                    if (instance.ScheduledDate.Date != DateTime.UtcNow.Date)
                        return false;
        
                    var scheduledTimeUtc = instance.ScheduledDate.Date.Add(instance.ScheduledStartTime.TimeOfDay);
                    var scheduledTimeInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledTimeUtc, nairobiTimeZone);
                    return scheduledTimeInNairobi > nairobiNow && 
                           scheduledTimeInNairobi <= nairobiNow.Add(_frequentCheckInterval);
                })
                .OrderBy(instance => instance.ScheduledDate)
                .ToList();

            // Combine and deduplicate
            var allInstances = pastDueInstances
                .Concat(upcomingInstances)
                .GroupBy(i => i.Id)
                .Select(g => g.First())
                .ToList();
            
            if (!allInstances.Any())
            {
                return; // Early return if no instances to process
            }
            
            _logger.LogInformation("Found {Count} shift instances to start", allInstances.Count);
            
            // Process each instance
            foreach (var instance in allInstances)
            {
                try
                {
                    // Double-check the status to avoid race conditions
                    if (instance.Status != ShiftInstanceStatus.Scheduled)
                    {
                        _logger.LogWarning("Skipping shift instance {InstanceId} - Status is already {Status}", 
                            instance.Id, instance.Status);
                        continue;
                    }
                    
                    instance.Status = ShiftInstanceStatus.InProgress;
                    instance.UpdatedAt = now;
                    instance.UpdatedBy = "System";
                    
                    // Save changes
                    var updatedInstance = await shiftInstanceRepository.UpdateAsync(instance);
                    
                    if (updatedInstance != null && updatedInstance.Status == ShiftInstanceStatus.InProgress)
                    {
                        _logger.LogInformation("Successfully started shift instance {InstanceId}", instance.Id);
                    }
                    else
                    {
                        _logger.LogError("Failed to update status for shift instance {InstanceId}", instance.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift instance {InstanceId}", instance.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CheckForShiftStarts");
        }
    }

    private async Task CheckForShiftEndingAlerts(
        IShiftInstanceRepository shiftInstanceRepository, 
        IShiftNotificationService shiftNotificationService,
        IUserShiftRepository userShiftRepository,
        DateTime now)
    {
        try
        {
            var alertTime = now.AddMinutes(10);
            var instancesNeedingEndingAlerts = await shiftInstanceRepository.GetInstancesByStatusAndTimeAsync(
                ShiftInstanceStatus.InProgress, 
                alertTime.AddMinutes(-1), 
                alertTime.AddMinutes(1));

            foreach (var instance in instancesNeedingEndingAlerts.Where(i => i.ScheduledEndTime <= alertTime))
            {
                try
                {
                    // Get users assigned to this shift
                    var assignedUserShifts = await userShiftRepository.GetUsersAssignedToShiftAsync(instance.ShiftId);
                    var activeUsers = assignedUserShifts.Where(us => !us.IsDeleted && us.User != null).ToList();

                    if (activeUsers.Any())
                    {
                        var userNotificationData = activeUsers.Select(us => (us.User.Email, $"{us.User.FirstName} {us.User.LastName}")).ToList();
                        
                        await shiftNotificationService.SendShiftNotificationsAsync(
                            userNotificationData,
                            instance.Id,
                            instance.ScheduledStartTime,
                            instance.ScheduledEndTime,
                            instance.Shift?.Name ?? "Unknown Shift",
                            NotificationType.ShiftEndingAlert);

                        _logger.LogInformation("Sent ending alert for shift instance {InstanceId} to {UserCount} users",
                            instance.Id, activeUsers.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending ending alert notifications for shift instance {InstanceId}", instance.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckForShiftEndingAlerts");
        }
    }

    private async Task CheckForShiftEnds(IShiftInstanceRepository shiftInstanceRepository, DateTime now)
    {
        try
        {
            // Convert current UTC time to Nairobi time
            var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(now, nairobiTimeZone);
            
            // Get all incomplete instances
            var allIncompleteInstances = (await shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.InProgress))
                .ToList();
            
            if (!allIncompleteInstances.Any())
            {
                return; // Early return if no instances to check
            }
            
            // Filter instances where the scheduled end time has passed in Nairobi time
            var instancesToCheck = allIncompleteInstances
                .Where(instance => 
                {
                    var scheduledDateUtc = instance.ScheduledDate;
                    var scheduledTimeUtc = scheduledDateUtc.Date.Add(instance.ScheduledEndTime.TimeOfDay);
                    var scheduledTimeInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledTimeUtc, nairobiTimeZone);
                    
                    return scheduledTimeInNairobi <= nairobiNow;
                })
                .ToList();
            
            if (!instancesToCheck.Any())
            {
                return; // Early return if no instances need completion
            }

            int completedCount = 0;

            foreach (var instance in instancesToCheck)
            {
                try
                {
                    if (instance.Status == ShiftInstanceStatus.InProgress)
                    {
                        instance.Status = ShiftInstanceStatus.Completed;
                        instance.UpdatedAt = now;
                        await shiftInstanceRepository.UpdateAsync(instance);
                        completedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing shift instance {InstanceId}", instance.Id);
                }
            }

            if (completedCount > 0)
            {
                _logger.LogInformation("Marked {Count} shift instances as completed", completedCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CheckForShiftEnds");
        }
    }
}