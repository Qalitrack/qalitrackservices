using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserShiftRepository:IRepository<UserShift>
{
    Task<IEnumerable<UserShift>> GetAllAsync();  // Get all user shifts
    Task<UserShift?> GetByIdAsync(string id);    // Get a specific user shift by ID
    Task<UserShift> CreateAsync(UserShift userShift);  // Assign a user to a shift
    Task<UserShift?> UpdateAsync(UserShift userShift); // Update a user shift assignment
    Task<bool> DeleteAsync(string id);  // Remove a user from a shift
    Task<bool> AssignUserToShiftAsync(string userId, string shiftId);  // Assign user to shift
    Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId);  // Remove user from shift
    Task<IEnumerable<UserShift>> GetShiftsForUserAsync(string userId);  
    
}