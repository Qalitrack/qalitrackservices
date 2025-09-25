using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Utilities;
using UserService.Core.DTOs.Common;
using UserService.Core.Enums;

namespace UserService.Core.Services
{
    public class ShiftService(
        IShiftRepository shiftRepository,
        IUserShiftRepository userShiftRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        ILogger<ShiftService> logger,
        IShiftInstanceService shiftInstanceService,
        IShiftNotificationService shiftNotificationService)
        : IShiftService
    {
        private readonly IShiftRepository _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        private readonly IUserShiftRepository _userShiftRepository = userShiftRepository ?? throw new ArgumentNullException(nameof(userShiftRepository));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<ShiftService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IShiftInstanceService _shiftInstanceService = shiftInstanceService ?? throw new ArgumentNullException(nameof(shiftInstanceService));
        private readonly IShiftNotificationService _shiftNotificationService = shiftNotificationService ?? throw new ArgumentNullException(nameof(shiftNotificationService));

        public async Task<PagedResult<ShiftDto>> GetAllAsync(PaginationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            try
            {
                var shifts = await _shiftRepository.GetAllAsync(parameters.Page, parameters.PageSize);
                
                var totalCount = await _shiftRepository.CountAsync();

                var mappedShifts = _mapper.Map<IEnumerable<ShiftDto>>(shifts);

                return new PagedResult<ShiftDto>
                {
                    Items = mappedShifts,
                    Page = parameters.Page,
                    PageSize = parameters.PageSize,
                    TotalCount = totalCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated shifts");
                throw new ApplicationException("An error occurred while retrieving shifts", ex);
            }
        }

        public async Task<ShiftResponse?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            return _mapper.Map<ShiftResponse>(shift);
        }
        private async Task<int> GetAssignedUserCountAsync(string shiftId)
        {
            var userShifts = await _userShiftRepository.GetUsersAssignedToShiftAsync(shiftId);
            return userShifts.Count();
        }
        

        private async Task ValidateShiftTimeChangesAsync(string shiftId, TimeSpan newStartTime, TimeSpan newEndTime)
        {
            // Get all users assigned to this shift
            var userShifts = await _userShiftRepository.GetUsersAssignedToShiftAsync(shiftId);
            
            foreach (var userShift in userShifts)
            {
                // Get all other shifts for this user
                var otherUserShifts = await _userShiftRepository.GetShiftsForUserAsync(userShift.UserId, null);
                
                foreach (var otherShift in otherUserShifts.Where(us => us.ShiftId != shiftId))
                {
                    // Check for overlaps with the new times
                    if (newStartTime < otherShift.Shift.EndTime && newEndTime > otherShift.Shift.StartTime)
                    {
                        throw new ValidationException($"Updated shift times would create an overlap with another shift for user {userShift.User.Email}");
                    }
                }
            }
        }
        

        private async Task<int> GetShiftInstanceCountAsync(string shiftId)
        {
            var instances = await _shiftInstanceService.GetInstancesByShiftAsync(shiftId);
            return instances.Count();
        }
        

        // New method to notify users about shift modifications
        private async Task NotifyUsersAboutShiftModificationAsync(string shiftId, string changes)
        {
            try
            {
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                if (shift == null) return;

                var userShifts = await _userShiftRepository.GetUsersAssignedToShiftAsync(shiftId);
                if (!userShifts.Any()) return;

                var users = userShifts.Select(us => (
                    Email: us.User.Email,
                    FullName: $"{us.User.FirstName} {us.User.LastName}"
                ));

                // For shift modifications, we'll use the shift's start and end times
                // You might want to get specific instances if needed
                var startDateTime = DateTime.Today.Add(shift.StartTime);
                var endDateTime = DateTime.Today.Add(shift.EndTime);

                await _shiftNotificationService.SendShiftNotificationsAsync(
                    users,
                    shiftId, // Using shiftId as instance ID for now
                    startDateTime,
                    endDateTime,
                    shift.Name,
                    NotificationType.ShiftModification,
                    changes: changes
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shift modification notifications for shift {ShiftId}", shiftId);
                // Don't rethrow - notification failure shouldn't break the main operation
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            var allUserShifts = await _userShiftRepository.GetAllAsync();
            var hasUsers = allUserShifts.Any(us => us.ShiftId == id);
            
            if (hasUsers)
                throw new ValidationException("Cannot delete shift with assigned users");

            try
            {
                // Get all scheduled instances for this shift
                var instances = await _shiftInstanceService.GetInstancesByShiftAsync(id);
                var scheduledInstances = instances.Where(i => i.Status == ShiftInstanceStatus.Scheduled);

                // Cancel all scheduled instances
                foreach (var instance in scheduledInstances)
                {
                    await _shiftInstanceService.CancelInstanceAsync(instance.Id, "Shift has been deleted");
                }

                // Proceed with soft delete of the shift
                return await _shiftRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cancelling shift instances for shift {ShiftId}", id);
                throw new Exception("An error occurred while cancelling shift instances. The shift was not deleted.");
            }
        }

      
        public async Task<bool> AssignUserToShiftAsync(string userId, string shiftId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            // Check if the user exists and is not soft deleted
            var user = await _userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                throw new Exception("User does not exist or is deleted");
            }

            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
                throw new Exception("Shift not found");

            var existingAssignment = (await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId))
                .FirstOrDefault(us => us.ShiftId == shiftId);

            if (existingAssignment != null)
                throw new ValidationException("User is already assigned to this shift");

            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId: null); // Get all shifts for the user
            foreach (var existingShift in userShifts)
            {
                if (shift.StartTime < existingShift.Shift.EndTime && shift.EndTime > existingShift.Shift.StartTime)
                {
                    throw new ValidationException("User already has an overlapping shift.");
                }
            }

            var userShift = new UserShift
            {
                UserId = userId,
                ShiftId = shiftId,
                AssignedAt = DateTime.UtcNow
            };

            var result = await _userShiftRepository.CreateAsync(userShift);
            if (result == null)
                throw new Exception("Failed to assign user to shift");

            // Send notification about shift assignment
            await NotifyUserAboutShiftAssignmentAsync(userId, shiftId);

            return true;
        }

        // New method to notify user about shift assignment
        private async Task NotifyUserAboutShiftAssignmentAsync(string userId, string shiftId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, true);
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                
                if (user == null || shift == null) return;

                var users = new[] { (Email: user.Email, FullName: $"{user.FirstName} {user.LastName}") };
                
                // For new assignments, we'll use the shift's start and end times
                var startDateTime = DateTime.Today.Add(shift.StartTime);
                var endDateTime = DateTime.Today.Add(shift.EndTime);

                await _shiftNotificationService.SendShiftNotificationsAsync(
                    users,
                    shiftId,
                    startDateTime,
                    endDateTime,
                    shift.Name,
                    NotificationType.ShiftModification,
                    changes: "You have been assigned to this shift"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shift assignment notification to user {UserId} for shift {ShiftId}", userId, shiftId);
                // Don't rethrow - notification failure shouldn't break the main operation
            }
        }

        public async Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
                
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            var user = await _userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                throw new Exception("User does not exist or is deleted");
            }

            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId);
            var userShift = userShifts.FirstOrDefault(us => us.ShiftId == shiftId);
            
            if (userShift == null)
                throw new Exception("User is not assigned to this shift");

            var result = await _userShiftRepository.DeleteAsync(userId, shiftId);

            // Send notification about shift removal
            if (result)
            {
                await NotifyUserAboutShiftRemovalAsync(userId, shiftId);
            }

            return result;
        }

        // New method to notify user about shift removal
        private async Task NotifyUserAboutShiftRemovalAsync(string userId, string shiftId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId, true);
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                
                if (user == null || shift == null) return;

                var users = new[] { (Email: user.Email, FullName: $"{user.FirstName} {user.LastName}") };
                
                // For removals, we'll use the shift's start and end times
                var startDateTime = DateTime.Today.Add(shift.StartTime);
                var endDateTime = DateTime.Today.Add(shift.EndTime);

                await _shiftNotificationService.SendShiftNotificationsAsync(
                    users,
                    shiftId,
                    startDateTime,
                    endDateTime,
                    shift.Name,
                    NotificationType.ShiftCancellation,
                    reason: "You have been removed from this shift"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending shift removal notification to user {UserId} for shift {ShiftId}", userId, shiftId);
                // Don't rethrow - notification failure shouldn't break the main operation
            }
        }

        public async Task<object?> IsUserAssignedToShiftAsync(string userId, string shiftId)
        {
            return await _userShiftRepository.IsUserAssignedToShiftAsync(userId, shiftId);
        }

        public async Task<MassAssignShiftResultDto> MassAssignShiftToRoleAsync(string roleId, string shiftId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role ID is required", nameof(roleId));
                        
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            var result = new MassAssignShiftResultDto
            {
                Success = false,
                Message = "Mass assignment started",
                TotalUsersProcessed = 0,
                UsersAssigned = 0,
                UsersFailed = 0,
                FailedUserIds = new List<string>(),
                FailedUserMessages = new Dictionary<string, string>()
            };

            try
            {
                var users = (await _userRepository.GetUsersByRoleAsync(roleId)).ToList();
                result.TotalUsersProcessed = users.Count;
                var successfullyAssignedUsers = new List<string>();

                foreach (var user in users)
                {
                    try
                    {
                        var shift = await _shiftRepository.GetByIdAsync(shiftId);
                        if (shift == null)
                        {
                            throw new Exception($"Shift with ID {shiftId} not found");
                        }

                        var isAssigned = await _userShiftRepository.IsUserAssignedToShiftAsync(user.Id, shiftId);
                        if (isAssigned)
                        {
                            continue;  
                        }

                        var userShifts = await _userShiftRepository.GetShiftsForUserAsync(user.Id, shiftId: null); // Get all shifts for the user
                        foreach (var existingShift in userShifts)
                        {
                            if (existingShift.Shift.StartTime < shift.StartTime && existingShift.Shift.EndTime > shift.StartTime ||
                                existingShift.Shift.StartTime < shift.EndTime && existingShift.Shift.EndTime > shift.EndTime ||
                                shift.StartTime < existingShift.Shift.EndTime && shift.EndTime > existingShift.Shift.StartTime)
                            {
                                result.UsersFailed++;
                                result.FailedUserIds.Add(user.Id);
                                result.FailedUserMessages[user.Id] = $"User has overlapping shift with shift {shiftId}";
                                break; 
                            }
                        }

                        var success = await AssignUserToShiftAsync(user.Id, shiftId);
                        if (success)
                        {
                            result.UsersAssigned++;
                            successfullyAssignedUsers.Add(user.Id);
                        }
                        else
                        {
                            result.UsersFailed++;
                            result.FailedUserIds.Add(user.Id);
                            result.FailedUserMessages[user.Id] = "Failed to assign shift";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.UsersFailed++;
                        result.FailedUserIds.Add(user.Id);
                        result.FailedUserMessages[user.Id] = ex.Message;
                    }
                }

                // Send mass assignment notification to successfully assigned users
                if (successfullyAssignedUsers.Any())
                {
                    await NotifyUsersAboutMassAssignmentAsync(successfullyAssignedUsers, shiftId);
                }

                result.Success = result.UsersFailed == 0;
                result.Message = result.UsersFailed == 0 
                    ? $"Successfully assigned shift to {result.UsersAssigned} users"
                    : $"Partially successful - Assigned to {result.UsersAssigned} users, failed for {result.UsersFailed} users";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass assigning shift {ShiftId} to role {RoleId}", shiftId, roleId);
                throw;
            }
        }

        // New method to notify users about mass assignment
        private async Task NotifyUsersAboutMassAssignmentAsync(List<string> userIds, string shiftId)
        {
            try
            {
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                if (shift == null) return;

                var users = new List<(string Email, string FullName)>();
                foreach (var userId in userIds)
                {
                    var user = await _userRepository.GetByIdAsync(userId, true);
                    if (user != null)
                    {
                        users.Add((user.Email, $"{user.FirstName} {user.LastName}"));
                    }
                }

                if (!users.Any()) return;

                var startDateTime = DateTime.Today.Add(shift.StartTime);
                var endDateTime = DateTime.Today.Add(shift.EndTime);

                await _shiftNotificationService.SendShiftNotificationsAsync(
                    users,
                    shiftId,
                    startDateTime,
                    endDateTime,
                    shift.Name,
                    NotificationType.ShiftModification,
                    changes: "You have been assigned to this shift as part of a role-based assignment"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending mass assignment notifications for shift {ShiftId}", shiftId);
                // Don't rethrow - notification failure shouldn't break the main operation
            }
        }

        public async Task<MassAssignShiftResultDto> MassRemoveUsersFromShiftByRoleAsync(string roleId, string shiftId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role ID is required", nameof(roleId));
                
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            var result = new MassAssignShiftResultDto
            {
                Success = false,
                Message = "Mass removal started",
                TotalUsersProcessed = 0,
                UsersAssigned = 0,
                UsersFailed = 0
            };

            try
            {
                var users = (await _userRepository.GetUsersByRoleAsync(roleId)).ToList();
                result.TotalUsersProcessed = users.Count;
                var successfullyRemovedUsers = new List<string>();

                foreach (var user in users)
                {
                    try
                    {
                        var isAssigned = await _userShiftRepository.IsUserAssignedToShiftAsync(user.Id, shiftId);
                        if (!isAssigned)
                        {
                            continue;
                        }

                        var success = await RemoveUserFromShiftAsync(user.Id, shiftId);
                        if (success)
                        {
                            result.UsersAssigned++;
                            successfullyRemovedUsers.Add(user.Id);
                        }
                        else
                        {
                            result.UsersFailed++;
                            result.FailedUserIds.Add(user.Id);
                            result.FailedUserMessages[user.Id] = "Failed to remove shift";
                        }
                    }
                    catch (Exception ex)
                    {
                        result.UsersFailed++;
                        result.FailedUserIds.Add(user.Id);
                        result.FailedUserMessages[user.Id] = ex.Message;
                    }
                }

                // Send mass removal notification to successfully removed users
                if (successfullyRemovedUsers.Any())
                {
                    await NotifyUsersAboutMassRemovalAsync(successfullyRemovedUsers, shiftId);
                }

                result.Success = result.UsersFailed == 0;
                result.Message = result.UsersFailed == 0 
                    ? $"Successfully removed shift from {result.UsersAssigned} users"
                    : $"Partially successful - Removed from {result.UsersAssigned} users, failed for {result.UsersFailed} users";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in mass removing shift {ShiftId} from role {RoleId}", shiftId, roleId);
                throw;
            }
        }

        // New method to notify users about mass removal
        private async Task NotifyUsersAboutMassRemovalAsync(List<string> userIds, string shiftId)
        {
            try
            {
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                if (shift == null) return;

                var users = new List<(string Email, string FullName)>();
                foreach (var userId in userIds)
                {
                    var user = await _userRepository.GetByIdAsync(userId, true);
                    if (user != null)
                    {
                        users.Add((user.Email, $"{user.FirstName} {user.LastName}"));
                    }
                }

                if (!users.Any()) return;

                var startDateTime = DateTime.Today.Add(shift.StartTime);
                var endDateTime = DateTime.Today.Add(shift.EndTime);

                await _shiftNotificationService.SendShiftNotificationsAsync(
                    users,
                    shiftId,
                    startDateTime,
                    endDateTime,
                    shift.Name,
                    NotificationType.ShiftCancellation,
                    reason: "You have been removed from this shift as part of a role-based removal"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending mass removal notifications for shift {ShiftId}", shiftId);
                // Don't rethrow - notification failure shouldn't break the main operation
            }
        }

        public async Task<UsersAssignedToShiftDto> GetUsersAssignedToShiftAsync(string? shiftId)
        {
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            try
            {
                var userShifts = await _userShiftRepository.GetUsersAssignedToShiftAsync(shiftId);
                
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                if (shift == null)
                {
                    throw new Exception($"Shift with ID {shiftId} not found");
                }

                var users = userShifts.Select(us => new UserDetailsDto
                {
                    Id = us.User.Id,
                    Email = us.User.Email,
                    FirstName = us.User.FirstName,
                    LastName = us.User.LastName
                }).ToList();

                return new UsersAssignedToShiftDto
                {
                    ShiftId = shiftId,
                    ShiftName = shift.Name,
                    Users = users,
                    TotalUsers = users.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users assigned to shift {ShiftId}", shiftId);
                throw;
            }
        }
        
        public async Task<PagedResult<ShiftDto>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var pagedShifts = await _shiftRepository.GetDeletedPagedAsync(parameters);
            var mappedShifts = _mapper.Map<IEnumerable<ShiftDto>>(pagedShifts.Items);

            return new PagedResult<ShiftDto>
            {
                Items = mappedShifts,
                Page = pagedShifts.Page,
                PageSize = pagedShifts.PageSize,
                TotalCount = pagedShifts.TotalCount
            };
        }

        public async Task<PagedResult<UserShiftDto>> GetDeletedUserShiftsPagedAsync(PaginationParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var pagedUserShifts = await _userShiftRepository.GetDeletedPagedAsync(parameters);
            var mappedUserShifts = _mapper.Map<IEnumerable<UserShiftDto>>(pagedUserShifts.Items);

            return new PagedResult<UserShiftDto>
            {
                Items = mappedUserShifts,
                Page = pagedUserShifts.Page,
                PageSize = pagedUserShifts.PageSize,
                TotalCount = pagedUserShifts.TotalCount
            };
        }

   

        public async Task<ShiftResponse> CreateEnhancedAsync(CreateShiftRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Validate the request
            if (request.StartTime >= request.EndTime)
                throw new ValidationException("End time must be after start time");

            if (request.EndDate.HasValue && request.EndDate < request.StartDate)
                throw new ValidationException("End date cannot be before start date");

            // Create the shift entity
            var shift = new Shift
            {
                Name = request.Name,
                Description = request.Description,
                StartTime = request.StartTime,
                EndTime = request.EndTime,  // Directly use the provided EndTime
                Mode = request.Mode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Type = request.Type,
                RequiredStaffCount = request.RequiredStaffCount,
                RecurrenceType = request.RecurrenceType,
                RecurrenceInterval = request.RecurrenceInterval,
                Status = ShiftStatus.Active
            };

            // Handle custom days and exception dates
            if (request.CustomDays != null && request.CustomDays.Any())
            {
                shift.CustomDays = request.CustomDays;
            }

            if (request.ExceptionDates != null && request.ExceptionDates.Any())
            {
                shift.ExceptionDates = request.ExceptionDates;
            }

            // Add audit fields
            var currentUser = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User); 
            shift.CreatedBy = currentUser;
            shift.UpdatedBy = currentUser;

            try
            {
                // Save the shift
                var createdShift = await _shiftRepository.CreateAsync(shift);
                
                // Generate shift instances
                var generateRequest = new ShiftInstanceGenerateRequest
                {
                    ShiftId = createdShift.Id,
                    StartDate = createdShift.StartDate,
                    EndDate = createdShift.Type == ShiftType.OneTime ? createdShift.StartDate : createdShift.EndDate,
                    StartTime = createdShift.StartTime,
                    EndTime = createdShift.EndTime,
                    RecurrenceType = createdShift.Type == ShiftType.OneTime ? RecurrenceType.None : createdShift.RecurrenceType,
                    RecurrenceInterval = createdShift.Type == ShiftType.OneTime ? 0 : createdShift.RecurrenceInterval,
                    CustomDays = createdShift.CustomDays,
                    ExceptionDates = createdShift.ExceptionDates,
                    CreatedBy = createdShift.CreatedBy,
                    UpdatedBy = createdShift.UpdatedBy
                };

             
                var instances = await _shiftInstanceService.GenerateInstancesAsync(generateRequest);

                // Map to response
                var response = _mapper.Map<ShiftResponse>(createdShift);
                
                // Set additional response properties
                response.IsActive = shift.IsActive;
                response.TotalInstances = 0; // Will be updated by the repository
                response.AssignedUsers = 0;   // Will be updated by the repository

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating enhanced shift");
                throw new ApplicationException("An error occurred while creating the shift", ex);
            }
        }

        // Shift instance generation has been moved to ShiftInstanceService

      public async Task<ShiftResponse?> UpdateEnhancedAsync(string id, UpdateShiftRequest request)
{
    if (string.IsNullOrEmpty(id))
        throw new ArgumentException("Shift ID is required", nameof(id));

    if (request == null)
        throw new ArgumentNullException(nameof(request));

    var existingShift = await _shiftRepository.GetByIdAsync(id);
    if (existingShift == null)
        throw new Exception("Shift not found");

    // Track changes for notifications
    var changes = new List<string>();

    // Check for time changes and validate
    if (existingShift.StartTime != request.StartTime || existingShift.EndTime != request.EndTime)
    {
        await ValidateShiftTimeChangesAsync(id, request.StartTime, request.EndTime);
        changes.Add($"Time changed from {existingShift.StartTime:hh\\:mm} - {existingShift.EndTime:hh\\:mm} to {request.StartTime:hh\\:mm} - {request.EndTime:hh\\:mm}");
    }

    if (existingShift.Name != request.Name)
        changes.Add($"Name changed from '{existingShift.Name}' to '{request.Name}'");

    if (existingShift.Description != request.Description)
        changes.Add("Description updated");

    if (existingShift.StartDate != request.StartDate)
        changes.Add($"Start date changed from {existingShift.StartDate:yyyy-MM-dd} to {request.StartDate:yyyy-MM-dd}");

    if (existingShift.EndDate != request.EndDate)
        changes.Add($"End date changed from {existingShift.EndDate?.ToString("yyyy-MM-dd") ?? "None"} to {request.EndDate?.ToString("yyyy-MM-dd") ?? "None"}");

    // Update shift properties
    existingShift.Name = request.Name;
    existingShift.Description = request.Description;
    existingShift.StartTime = request.StartTime;
    existingShift.EndTime = request.EndTime;
    existingShift.StartDate = request.StartDate;
    existingShift.EndDate = request.EndDate;
    existingShift.Mode = request.Mode;
    existingShift.RecurrenceType = request.RecurrenceType;
    existingShift.RecurrenceInterval = request.RecurrenceInterval;
    existingShift.CustomDays = request.CustomDays;
    existingShift.ExceptionDates = request.ExceptionDates;
    existingShift.RequiredStaffCount = request.RequiredStaffCount;
    existingShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
    existingShift.UpdatedAt = DateTime.UtcNow;

    try
    {
        // Always regenerate instances for the shift
        var generateRequest = new ShiftInstanceGenerateRequest
        {
            ShiftId = existingShift.Id,
            StartDate = existingShift.StartDate,
            EndDate = existingShift.EndDate,
            StartTime = existingShift.StartTime,
            EndTime = existingShift.EndTime,
            RecurrenceType = existingShift.Type == ShiftType.OneTime ? RecurrenceType.None : existingShift.RecurrenceType,
            RecurrenceInterval = existingShift.Type == ShiftType.OneTime ? 0 : existingShift.RecurrenceInterval,
            CustomDays = existingShift.CustomDays,
            ExceptionDates = existingShift.ExceptionDates,
            CreatedBy = existingShift.CreatedBy,
            UpdatedBy = existingShift.UpdatedBy
        };

        // This will handle deleting future instances and regenerating them
        await _shiftInstanceService.UpdateShiftInstancesAsync(id, generateRequest);
        changes.Add("Shift schedule has been updated with new instances");

        // Update the shift
        var updatedShift = await _shiftRepository.UpdateAsync(existingShift);
        if (updatedShift == null)
            throw new Exception("Failed to update shift");

        // Send notifications if there were changes
        if (changes.Any())
        {
            var changesText = string.Join("; ", changes);
            await NotifyUsersAboutShiftModificationAsync(id, changesText);
        }

        // Map to response
        var response = _mapper.Map<ShiftResponse>(updatedShift);
        response.IsActive = updatedShift.IsActive;
        response.TotalInstances = await GetShiftInstanceCountAsync(id);
        response.AssignedUsers = await GetAssignedUserCountAsync(id);

        return response;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating enhanced shift {ShiftId}", id);
        throw new ApplicationException("An error occurred while updating the shift", ex);
    }
}

        public async Task<IEnumerable<ShiftDto>> GetShiftsForUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            try
            {
                var user = await _userRepository.GetByIdAsync(userId, true);
                if (user == null)
                {
                    throw new Exception("User does not exist or is deleted");
                }

                var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId, null);
                var shifts = userShifts.Select(us => _mapper.Map<ShiftDto>(us.Shift)).ToList();
                
                _logger.LogInformation("Retrieved {ShiftCount} shifts for user {UserId}", shifts.Count, userId);
                
                return shifts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shifts for user {UserId}", userId);
                throw;
            }
        }
    }

}