using UserService.Core.DTOs;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities; // Assuming you have relevant DTOs for Shift

namespace UserService.Core.Interfaces
{
    public interface IShiftService
    {
        // CRUD Operations
        Task<IEnumerable<ShiftDto>> GetAllAsync();  // Get all shifts
        Task<ShiftDto?> GetByIdAsync(string id);   // Get shift by ID
        Task<ShiftDto> CreateAsync(DTOs.Shift.CreateShiftDto dto);  // Create a new shift
        Task<ShiftDto?> UpdateAsync(string id, DTOs.Shift.UpdateShiftDto dto);  // Update an existing shift
        Task<bool> DeleteAsync(string id);  // Delete a shift by ID
        
        // Additional Service Methods
        Task<bool> IsShiftActive(string shiftId);  // Check if a shift is active (currently happening)
        Task<bool> AssignUserToShiftAsync(string userId, string shiftId);  // Assign a user to a shift
        Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId);  // Remove a user from a shift
        Task<bool> HasActiveStrictShiftAsync();
        Task<bool> HasActiveStrictShiftForUserAsync(string userId);
        Task<User?> ValidateUserCredentials(string email, string password);
        
        // Optional: Add any other business logic related to shifts (e.g., conflict checking)
        Task<object?> IsUserAssignedToShiftAsync(string userId, string shiftId);
        Task<MassAssignShiftResultDto> MassAssignShiftToRoleAsync(string roleId, string shiftId);
        Task<MassAssignShiftResultDto> MassRemoveUsersFromShiftByRoleAsync(string roleId, string shiftId);
        Task<UsersAssignedToShiftDto> GetUsersAssignedToShiftAsync(string? shiftId);
    }
}