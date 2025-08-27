using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Utilities;

namespace UserService.Core.Services
{
    public class ShiftService(
        IShiftRepository shiftRepository,
        IUserShiftRepository userShiftRepository,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        ILogger<ShiftService> logger)
        : IShiftService
    {
        private readonly IShiftRepository _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
        private readonly IUserShiftRepository _userShiftRepository = userShiftRepository ?? throw new ArgumentNullException(nameof(userShiftRepository));
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<ShiftService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<IEnumerable<ShiftDto>> GetAllAsync()
        {
            var shifts = await _shiftRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
        }

        public async Task<ShiftDto?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task<ShiftDto> CreateAsync(DTOs.Shift.CreateShiftDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate duration
            if (dto.DurationMinutes <= 0)
                throw new ValidationException("Duration must be greater than 0 minutes");

            // Always calculate EndTime from StartTime + DurationMinutes
            var calculatedEndTime = dto.StartTime.AddMinutes(dto.DurationMinutes);

            // Validate shift times - compare time parts only since shifts are time-based
            var startTime = dto.StartTime.TimeOfDay;
            var endTime = calculatedEndTime.TimeOfDay;
            
            // Allow overnight shifts (e.g., 22:00 to 06:00), but not same time
            if (startTime == endTime)
                throw new ValidationException("Start time and end time cannot be the same");

            var shift = _mapper.Map<Shift>(dto);
            
            // Set calculated EndTime
            shift.EndTime = calculatedEndTime.TimeOfDay;
            shift.CreatedAt = DateTime.UtcNow;
            shift.UpdatedAt = DateTime.UtcNow;
            shift.CreatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
            shift.UpdatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User); 
            
            
            var createdShift = await _shiftRepository.CreateAsync(shift);
            return _mapper.Map<ShiftDto>(createdShift);
        }

        public async Task<ShiftDto?> UpdateAsync(string id, DTOs.Shift.UpdateShiftDto dto)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));
                
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var existingShift = await _shiftRepository.GetByIdAsync(id);
            if (existingShift == null)
                throw new Exception("Shift not found");

            // Update properties only if they are provided in the DTO
            if (!string.IsNullOrEmpty(dto.Name))
                existingShift.Name = dto.Name;
                
            if (!string.IsNullOrEmpty(dto.Description))
                existingShift.Description = dto.Description;
                
            // Handle StartTime and DurationMinutes updates
            var startTime = dto.StartTime != default ? dto.StartTime : DateTime.Today.Add(existingShift.StartTime);
            var durationMinutes = dto.DurationMinutes ?? existingShift.DurationMinutes ?? 480; // Default 8 hours if null

            // Validate duration
            if (durationMinutes <= 0)
                throw new ValidationException("Duration must be greater than 0 minutes");

            // Always recalculate EndTime when either StartTime or DurationMinutes changes
            var calculatedEndTime = startTime.AddMinutes(durationMinutes);

            // Validate shift times - compare time parts only since shifts are time-based
            var startTimeOfDay = startTime.TimeOfDay;
            var endTimeOfDay = calculatedEndTime.TimeOfDay;
            
            // Allow overnight shifts (e.g., 22:00 to 06:00), but not same time
            if (startTimeOfDay == endTimeOfDay)
                throw new ValidationException("Start time and end time cannot be the same");

            // Update the shift properties
            if (dto.StartTime != default)
                existingShift.StartTime = startTime.TimeOfDay;
                
            if (dto.DurationMinutes.HasValue)
                existingShift.DurationMinutes = durationMinutes;

            // Always update EndTime based on current StartTime and DurationMinutes
            existingShift.EndTime = calculatedEndTime.TimeOfDay;
                
            if (dto.Mode.HasValue)
                existingShift.Mode = dto.Mode.Value;
                
            if (dto.AutoRepeatDaily.HasValue)
                existingShift.AutoRepeatDaily = dto.AutoRepeatDaily.Value;
                
            existingShift.UpdatedAt = DateTime.UtcNow;
            existingShift.UpdatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
            var updatedShift = await _shiftRepository.UpdateAsync(existingShift);
            return _mapper.Map<ShiftDto>(updatedShift);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            // Check if shift exists
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            // Check if any users are assigned to this shift by getting all user shifts
            // and filtering by the shift ID
            var allUserShifts = await _userShiftRepository.GetAllAsync();
            var hasUsers = allUserShifts.Any(us => us.ShiftId == id);
            
            if (hasUsers)
                throw new ValidationException("Cannot delete shift with assigned users");

            return await _shiftRepository.DeleteAsync(id);
        }

        public async Task<bool> IsShiftActive(string shiftId)
        {
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
                throw new Exception("Shift not found");

            return await _shiftRepository.IsShiftActiveAsync(shiftId);
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

            // Check if the shift exists
            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
                throw new Exception("Shift not found");

            // Check if the user is already assigned to this shift
            var existingAssignment = (await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId))
                .FirstOrDefault(us => us.ShiftId == shiftId);

            if (existingAssignment != null)
                throw new ValidationException("User is already assigned to this shift");

            // Check if the user has overlapping shifts
            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId: null); // Get all shifts for the user
            foreach (var existingShift in userShifts)
            {
                // Check if the shift times overlap
                if (shift.StartTime < existingShift.Shift.EndTime && shift.EndTime > existingShift.Shift.StartTime)
                {
                    throw new ValidationException("User already has an overlapping shift.");
                }
            }

            // Create new user-shift assignment
            var userShift = new UserShift
            {
                UserId = userId,
                ShiftId = shiftId,
                AssignedAt = DateTime.UtcNow
            };

            var result = await _userShiftRepository.CreateAsync(userShift);
            if (result == null)
                throw new Exception("Failed to assign user to shift");
            return true;
        }

        public async Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId)
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

            // Check if the assignment exists
            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId, shiftId);
            var userShift = userShifts.FirstOrDefault(us => us.ShiftId == shiftId);
            
            if (userShift == null)
                throw new Exception("User is not assigned to this shift");

            return await _userShiftRepository.DeleteAsync(userId, shiftId);
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
            // Get all users with the specified role
            var users = (await _userRepository.GetUsersByRoleAsync(roleId)).ToList();
            result.TotalUsersProcessed = users.Count;

            foreach (var user in users)
            {
                try
                {
                    // Get the shift details
                    var shift = await _shiftRepository.GetByIdAsync(shiftId);
                    if (shift == null)
                    {
                        throw new Exception($"Shift with ID {shiftId} not found");
                    }

                    // Check if user already has this shift assigned
                    var isAssigned = await _userShiftRepository.IsUserAssignedToShiftAsync(user.Id, shiftId);
                    if (isAssigned)
                    {
                        continue;  // Skip user if already assigned to this shift
                    }

                    // Check if the user has overlapping shifts
                    var userShifts = await _userShiftRepository.GetShiftsForUserAsync(user.Id, shiftId: null); // Get all shifts for the user
                    foreach (var existingShift in userShifts)
                    {
                        // Check if the shift times overlap
                        if (existingShift.Shift.StartTime < shift.StartTime && existingShift.Shift.EndTime > shift.StartTime ||
                            existingShift.Shift.StartTime < shift.EndTime && existingShift.Shift.EndTime > shift.EndTime ||
                            shift.StartTime < existingShift.Shift.EndTime && shift.EndTime > existingShift.Shift.StartTime)
                        {
                            // Skip this user if there is an overlap
                            result.UsersFailed++;
                            result.FailedUserIds.Add(user.Id);
                            result.FailedUserMessages[user.Id] = $"User has overlapping shift with shift {shiftId}";
                            break;  // No need to assign this user to the shift
                        }
                    }

                    // If no overlapping shift found, proceed with assignment
                    var success = await AssignUserToShiftAsync(user.Id, shiftId);
                    if (success)
                    {
                        result.UsersAssigned++;
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

            // Update final success status based on results
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
                // Get all users with the specified role
                var users = (await _userRepository.GetUsersByRoleAsync(roleId)).ToList();
                result.TotalUsersProcessed = users.Count;

                foreach (var user in users)
                {
                    try
                    {
                        // Check if user has this shift assigned
                        var isAssigned = await _userShiftRepository.IsUserAssignedToShiftAsync(user.Id, shiftId);
                        if (!isAssigned)
                        {
                            continue;
                        }

                        // Try to remove the shift
                        var success = await RemoveUserFromShiftAsync(user.Id, shiftId);
                        if (success)
                        {
                            result.UsersAssigned++;
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

                // Update final success status based on results
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

        public async Task<UsersAssignedToShiftDto> GetUsersAssignedToShiftAsync(string? shiftId)
        {
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            try
            {
                // Get all users assigned to this shift
                var userShifts = await _userShiftRepository.GetUsersAssignedToShiftAsync(shiftId);
                
                // Get the shift details
                var shift = await _shiftRepository.GetByIdAsync(shiftId);
                if (shift == null)
                {
                    throw new Exception($"Shift with ID {shiftId} not found");
                }

                // Extract user details from user shifts
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
    }
}