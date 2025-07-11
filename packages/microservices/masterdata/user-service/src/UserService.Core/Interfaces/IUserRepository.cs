using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByFirstNameAsync(string firstName);  // Get user by first name
        Task<User?> GetByLastNameAsync(string lastName);    // Get user by last name
        Task<User?> GetByEmailAsync(string email);          // Get user by email
        Task<User?> GetByMobileNumberAsync(string mobileNumber); // Get user by mobile number

        // Methods related to User's Shift
        Task<IEnumerable<UserShift>> GetUserShiftsAsync(string userId);  // Get all shifts assigned to a specific user
        Task<UserShift?> GetUserShiftByShiftIdAsync(string userId, string shiftId); // Get specific shift for a user by ShiftId
        Task<bool> AssignShiftToUserAsync(string userId, string shiftId);  // Assign a shift to a user
        Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId); // Remove a shift assignment for a user
    }
}