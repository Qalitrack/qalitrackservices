using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UserService.Core.Interfaces;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

namespace UserService.Infrastructure.Services
{
   public class ShiftMonitorService : BackgroundService
   {
       private readonly ILogger<ShiftMonitorService> _logger;
       private readonly IServiceScopeFactory _serviceScopeFactory;
       
       public ShiftMonitorService(
           ILogger<ShiftMonitorService> logger,
           IServiceScopeFactory serviceScopeFactory)
       {
           _logger = logger;
           _serviceScopeFactory = serviceScopeFactory;
       }
       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a new scope for this operation
                using var scope = _serviceScopeFactory.CreateScope();
                var shiftService = scope.ServiceProvider.GetRequiredService<IShiftService>();
                
                // Use the existing method to check for active strict shifts that have ended
                await MonitorStrictShiftsAsync(shiftService);

                // Wait for a set period before checking again (e.g., 1 minute)
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while monitoring strict shifts.");
            }
        }
    }

    private async Task MonitorStrictShiftsAsync(IShiftService shiftService)
{
    var currentTime = DateTime.UtcNow;
    
    // Create a scope to resolve services
    using var scope = _serviceScopeFactory.CreateScope();
    var shiftInstanceService = scope.ServiceProvider.GetRequiredService<IShiftInstanceService>();
    
    try
    {
        // Get all instances that were supposed to end in the last hour
        // This handles any instances we might have missed in previous runs
        var recentEndTime = currentTime;
        var startTime = currentTime.AddHours(-1);
        
        var recentInstances = await shiftInstanceService.GetInstancesByDateRangeAsync(startTime, recentEndTime);
        
        // Filter for active, strict shift instances that have passed their end time
        var endedInstances = recentInstances
            .Where(instance => 
                instance.Status == ShiftInstanceStatus.InProgress &&
                instance.ScheduledEndTime <= currentTime)
            .ToList();

        _logger.LogInformation("Found {Count} strict shift instances that have ended", endedInstances.Count);

        foreach (var instance in endedInstances)
        {
            _logger.LogInformation("Processing ended strict shift instance {InstanceId} for shift {ShiftId}", 
                instance.Id, instance.ShiftId);

            try
            {
                // Get the shift details
                var shift = await shiftService.GetByIdAsync(instance.ShiftId);
                if (shift == null || shift.Mode != ShiftMode.Strict)
                {
                    _logger.LogWarning("Skipping instance {InstanceId} - shift not found or not in strict mode", instance.Id);
                    continue;
                }

                // Create a new scope for logging out users
                using var logoutScope = _serviceScopeFactory.CreateScope();
                var userShiftRepository = logoutScope.ServiceProvider.GetRequiredService<IUserShiftRepository>();
                var tokenService = logoutScope.ServiceProvider.GetRequiredService<ITokenService>();
                
                // Log out users associated with this shift instance
                await LogoutUsersForShift(shift, userShiftRepository, tokenService, _logger);
                
                _logger.LogInformation("Successfully processed shift instance {InstanceId}", instance.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing shift instance {InstanceId}", instance.Id);
            }
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in MonitorStrictShiftsAsync");
    }    }

    private static async Task LogoutUsersForShift(
        ShiftResponse shift,
        IUserShiftRepository userShiftRepository,
        ITokenService tokenService,
        ILogger<ShiftMonitorService> logger)
    {
        // Get all users assigned to this shift
        var usersAssignedToShift = await userShiftRepository.GetUsersAssignedToShiftAsync(shift.Id);

        foreach (var userShift in usersAssignedToShift)
        {
            var userId = userShift.UserId;

            // Revoke the user's token (ensure the token service works with user id)
            var success = await tokenService.RevokeTokenAsync(userId);
        }
    }
}
}
