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
                   var shiftRepository = scope.ServiceProvider.GetRequiredService<IShiftRepository>();
                   
                   // Monitor strict shift instances for logout
                   await MonitorStrictShiftsAsync(shiftService);
                   
                   // Monitor and update expired shift statuses
                   await MonitorShiftStatusAsync(shiftRepository);

                   // Wait for a set period before checking again (e.g., 1 minute)
                   await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error occurred while monitoring shifts.");
               }
           }
       }

      private async Task MonitorStrictShiftsAsync(IShiftService shiftService)
        {
            var currentTimeUtc = DateTime.UtcNow;
            
            // Create a scope to resolve services
            using var scope = _serviceScopeFactory.CreateScope();
            var shiftInstanceRepository = scope.ServiceProvider.GetRequiredService<IShiftInstanceRepository>();
            
            try
            {
                // Get Nairobi timezone for proper comparison
                var nairobiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
                var nairobiNow = TimeZoneInfo.ConvertTimeFromUtc(currentTimeUtc, nairobiTimeZone);
                
                _logger.LogInformation("=== CHECKING FOR ENDED SHIFT INSTANCES ===");
                _logger.LogInformation("Current UTC time: {UtcNow:yyyy-MM-dd HH:mm:ss}", currentTimeUtc);
                _logger.LogInformation("Current Nairobi time: {NairobiNow:yyyy-MM-dd HH:mm:ss}", nairobiNow);
                
                // Get all in-progress instances
                var inProgressInstances = (await shiftInstanceRepository.GetInstancesByStatusAsync(ShiftInstanceStatus.InProgress)).ToList();
                
                // Filter for instances that should have ended by now in Nairobi time
                var endedInstances = inProgressInstances
                    .Where(instance => 
                    {
                        var scheduledTimeUtc = instance.ScheduledDate.Date.Add(instance.ScheduledEndTime.TimeOfDay);
                        var scheduledTimeInNairobi = TimeZoneInfo.ConvertTimeFromUtc(scheduledTimeUtc, nairobiTimeZone);
                        return scheduledTimeInNairobi <= nairobiNow;
                    })
                    .ToList();

                _logger.LogInformation("Found {Count} shift instances that have ended", endedInstances.Count);

                foreach (var instance in endedInstances)
                {
                    var scheduledEndTimeNairobi = TimeZoneInfo.ConvertTimeFromUtc(
                        instance.ScheduledDate.Date.Add(instance.ScheduledEndTime.TimeOfDay), 
                        nairobiTimeZone);
                        
                    _logger.LogInformation("Processing ended shift instance {InstanceId} for shift {ShiftId} - Ended at: {EndTime} (Nairobi)", 
                        instance.Id, instance.ShiftId, scheduledEndTimeNairobi);

                    try
                    {
                        // Get the shift details
                        var shift = await shiftService.GetByIdAsync(instance.ShiftId);
                        if (shift == null)
                        {
                            _logger.LogWarning("Skipping instance {InstanceId} - shift not found", instance.Id);
                            continue;
                        }

                        // Process all shifts regardless of mode
                        using var logoutScope = _serviceScopeFactory.CreateScope();
                        var userShiftRepository = logoutScope.ServiceProvider.GetRequiredService<IUserShiftRepository>();
                        var tokenService = logoutScope.ServiceProvider.GetRequiredService<ITokenService>();
                        
                        // Log out users associated with this shift instance using the instance's end time
                        await LogoutUsersForShift(instance, shift, userShiftRepository, tokenService, _logger);
                        
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
            }
        }

       private async Task MonitorShiftStatusAsync(IShiftRepository shiftRepository)
       {
           var currentDate = DateTime.Today;
           
           try
           {
               _logger.LogInformation("Checking for expired shifts on {CurrentDate}", currentDate);
               
               // Get all active shifts that might need status updates
               var activeShifts = await shiftRepository.GetShiftsByStatusAsync(ShiftStatus.Active);
               
               var expiredShifts = activeShifts.Where(shift => 
                   IsShiftExpired(shift, currentDate)).ToList();

               _logger.LogInformation("Found {Count} expired shifts to update", expiredShifts.Count);

               foreach (var shift in expiredShifts)
               {
                   try
                   {
                       _logger.LogInformation("Updating expired shift {ShiftId} ({ShiftName}) - EndDate: {EndDate}", 
                           shift.Id, shift.Name, shift.EndDate?.ToString("yyyy-MM-dd") ?? "None");

                       // Update shift status to Expired
                       shift.Status = ShiftStatus.Completed;
                       shift.UpdatedAt = DateTime.UtcNow;
                       shift.UpdatedBy = "System"; // System update
                       
                       var updatedShift = await shiftRepository.UpdateAsync(shift);
                       
                       if (updatedShift != null)
                       {
                           _logger.LogInformation("Successfully updated shift {ShiftId} status to Expired", shift.Id);
                           
                           // Optionally, send notifications to users about shift expiration
                           await NotifyUsersAboutShiftExpiration(shift);
                       }
                       else
                       {
                           _logger.LogWarning("Failed to update shift {ShiftId} status", shift.Id);
                       }
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError(ex, "Error updating status for shift {ShiftId}", shift.Id);
                   }
               }

               await CheckForShiftsToActivate(shiftRepository, currentDate);
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error in MonitorShiftStatusAsync");
           }
       }

       private async Task CheckForShiftsToActivate(IShiftRepository shiftRepository, DateTime currentDate)
       {
           try
           {
               // Get published shifts that should now be active
               var publishedShifts = await shiftRepository.GetShiftsByStatusAsync(ShiftStatus.Published);
               
               var shiftsToActivate = publishedShifts.Where(shift => 
                   shift.StartDate <= currentDate && 
                   (shift.EndDate == null || shift.EndDate >= currentDate)).ToList();

               _logger.LogInformation("Found {Count} published shifts to activate", shiftsToActivate.Count);

               foreach (var shift in shiftsToActivate)
               {
                   try
                   {
                       _logger.LogInformation("Activating shift {ShiftId} ({ShiftName}) - StartDate: {StartDate}", 
                           shift.Id, shift.Name, shift.StartDate.ToString("yyyy-MM-dd"));

                       shift.Status = ShiftStatus.Active;
                       shift.UpdatedAt = DateTime.UtcNow;
                       shift.UpdatedBy = "System";
                       
                       var updatedShift = await shiftRepository.UpdateAsync(shift);
                       
                       if (updatedShift != null)
                       {
                           _logger.LogInformation("Successfully activated shift {ShiftId}", shift.Id);
                       }
                   }
                   catch (Exception ex)
                   {
                       _logger.LogError(ex, "Error activating shift {ShiftId}", shift.Id);
                   }
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error checking shifts to activate");
           }
       }

       private static bool IsShiftExpired(Shift shift, DateTime currentDate)
       {
           // Handle one-time shifts
           if (shift.Type == ShiftType.OneTime)
           {
               // Check if the shift's end date has passed
               return shift.EndDate.HasValue && shift.EndDate.Value.Date < currentDate.Date;
           }
           
           // Handle recurring shifts
           if (shift.Type == ShiftType.Recurring)
           {
               // If shift has an end date and it's passed, the shift is expired
               if (shift.EndDate.HasValue && shift.EndDate.Value.Date < currentDate.Date)
               {
                   return true;
               }
               
               // If no end date, recurring shifts don't expire automatically
               return false;
           }
           
           return false;
       }

       private async Task NotifyUsersAboutShiftExpiration(Shift shift)
       {
           try
           {
               using var scope = _serviceScopeFactory.CreateScope();
               var userShiftRepository = scope.ServiceProvider.GetRequiredService<IUserShiftRepository>();
               var shiftNotificationService = scope.ServiceProvider.GetRequiredService<IShiftNotificationService>();
               
               // Get users assigned to this shift
               var userShifts = await userShiftRepository.GetUsersAssignedToShiftAsync(shift.Id);
               
               if (userShifts.Any())
               {
                   var users = userShifts.Select(us => (
                       Email: us.User.Email,
                       FullName: $"{us.User.FirstName} {us.User.LastName}"
                   ));

                   var startDateTime = DateTime.Today.Add(shift.StartTime);
                   var endDateTime = DateTime.Today.Add(shift.EndTime);

                   await shiftNotificationService.SendShiftNotificationsAsync(
                       users,
                       shift.Id,
                       startDateTime,
                       endDateTime,
                       shift.Name,
                       NotificationType.ShiftCancellation,
                       reason: "This shift has expired and is no longer active"
                   );
                   
                   _logger.LogInformation("Sent expiration notifications for shift {ShiftId} to {UserCount} users", 
                       shift.Id, userShifts.Count());
               }
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error sending shift expiration notifications for shift {ShiftId}", shift.Id);
           }
       }

       private async Task LogoutUsersForShift(
           ShiftInstance instance,
           ShiftResponse shiftResponse,
           IUserShiftRepository userShiftRepository,
           ITokenService tokenService,
           ILogger<ShiftMonitorService> logger)
       {
           try
           {
               // Get the actual Shift entity (not the response DTO) for attendance handling
               using var shiftScope = _serviceScopeFactory.CreateScope();
               var shiftRepository = shiftScope.ServiceProvider.GetRequiredService<IShiftRepository>();
               var actualShift = await shiftRepository.GetByIdAsync(shiftResponse.Id);
               
               if (actualShift == null)
               {
                   logger.LogError("Could not find shift entity {ShiftId} for attendance handling", shiftResponse.Id);
                   return;
               }

               // Get all users assigned to this shift
               var usersAssignedToShift = await userShiftRepository.GetUsersAssignedToShiftAsync(shiftResponse.Id);

               logger.LogInformation("Processing logout for {UserCount} users assigned to expired shift {ShiftId} ({ShiftName})", 
                   usersAssignedToShift.Count(), shiftResponse.Id, shiftResponse.Name);

               foreach (var userShift in usersAssignedToShift)
               {
                   var userId = userShift.UserId;
                   var userName = $"{userShift.User.FirstName} {userShift.User.LastName}";
                   var currentDateTime = DateTime.UtcNow;

                   logger.LogInformation("Processing logout for user {UserId} ({UserName}) from expired shift {ShiftId}", 
                       userId, userName, shiftResponse.Id);

                   try
                   {
                       // Create a scope for attendance handling
                       using var attendanceScope = _serviceScopeFactory.CreateScope();
                       var shiftAttendanceHandlerService = attendanceScope.ServiceProvider.GetRequiredService<IShiftAttendanceHandlerService>();
                       
                       // Use the instance's scheduled end time for accurate attendance tracking
                       var scheduledEndTime = instance.ScheduledDate.Date.Add(instance.ScheduledEndTime.TimeOfDay);
                       
                       // Handle individual user attendance logout with the instance's scheduled end time
                       var attendanceHandled = await shiftAttendanceHandlerService.HandleLogoutAttendanceAsync(userId, actualShift, scheduledEndTime);
                       
                       logger.LogInformation("Using scheduled end time {ScheduledEndTime} for user {UserId} shift {ShiftId}", 
                           scheduledEndTime, userId, shiftResponse.Id);
                       
                       if (attendanceHandled)
                       {
                           logger.LogInformation("Successfully clocked out user {UserId} ({UserName}) from expired strict shift {ShiftId}", 
                               userId, userName, shiftResponse.Id);
                       }
                       else
                       {
                           logger.LogWarning("Failed to clock out user {UserId} ({UserName}) from expired strict shift {ShiftId}", 
                               userId, userName, shiftResponse.Id);
                       }
                       
                       // Then revoke the user's token to force logout
                       var success = await tokenService.RevokeTokenAsync1(new Guid(userId));
                       
                       if (success)
                       {
                           logger.LogInformation("Successfully revoked token for user {UserId} ({UserName}) from expired strict shift {ShiftId}", 
                               userId, userName, shiftResponse.Id);
                       }
                       else
                       {
                           logger.LogWarning("Failed to revoke token for user {UserId} ({UserName}) from expired strict shift {ShiftId}", 
                               userId, userName, shiftResponse.Id);
                       }
                   }
                   catch (Exception ex)
                   {
                       logger.LogError(ex, "Error during logout process for user {UserId} ({UserName}) from shift {ShiftId}", 
                           userId, userName, shiftResponse.Id);
                       
                       // Still try to revoke token even if attendance fails
                       try
                       {
                           await tokenService.RevokeTokenAsync(userId);
                           logger.LogInformation("Token revoked for user {UserId} ({UserName}) despite attendance error", userId, userName);
                       }
                       catch (Exception tokenEx)
                       {
                           logger.LogError(tokenEx, "Failed to revoke token for user {UserId} ({UserName}) after attendance error", userId, userName);
                       }
                   }
               }
           }
           catch (Exception ex)
           {
               logger.LogError(ex, "Error in LogoutUsersForShift for shift {ShiftId}", shiftResponse.Id);
           }
       }
   }
}