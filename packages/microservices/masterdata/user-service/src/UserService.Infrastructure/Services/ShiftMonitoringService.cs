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
            _logger.LogInformation("Monitoring strict shifts for logouts.");

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
        // Use the existing method to get all active shifts
        var activeShifts = await shiftService.GetAllAsync();

        var currentTime = DateTime.UtcNow.TimeOfDay;
        _logger.LogDebug("Current time: {CurrentTime}", currentTime);

        // Filter for shifts in strict mode and whose end time has passed
        var strictShifts = activeShifts
            .Where(s => s.Mode == ShiftMode.Strict)
            .Where(s => 
            {
                var shiftEndTime = s.EndTime.TimeOfDay;
                
                // For overnight shifts (EndTime < StartTime), check if current time is past midnight and before end time
                // OR if current time is after start time (yesterday)
                if (shiftEndTime < s.StartTime.TimeOfDay)
                {
                    // Overnight shift: ends next day
                    return currentTime <= shiftEndTime; // We're in the "next day" portion and past end time
                }
                else
                {
                    // Regular shift: ends same day
                    return currentTime >= shiftEndTime; // Current time is past the end time
                }
            })
            .ToList();

        _logger.LogInformation("Found {Count} strict shifts that have ended", strictShifts.Count());

        foreach (ShiftDto shift in strictShifts)
        {
            _logger.LogInformation("Processing ended strict shift: {ShiftName} (End: {EndTime})", 
                shift.Name, shift.EndTime.TimeOfDay);

            // Create a new scope for logging out users
            using var logoutScope = _serviceScopeFactory.CreateScope();
            var userShiftRepository = logoutScope.ServiceProvider.GetRequiredService<IUserShiftRepository>();
            var tokenService = logoutScope.ServiceProvider.GetRequiredService<ITokenService>();
        
            // Log out users associated with this shift
            await LogoutUsersForShift(shift, userShiftRepository, tokenService, _logger);
        }
    }

    private static async Task LogoutUsersForShift(
        ShiftDto shift,
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

            if (success)
            {
                logger.LogInformation("Successfully logged out user {UserId} from shift {ShiftName}", userId, shift.Name);
            }
            else
            {
                logger.LogWarning("Failed to log out user {UserId} from shift {ShiftName}", userId, shift.Name);
            }
        }
    }
}
}
