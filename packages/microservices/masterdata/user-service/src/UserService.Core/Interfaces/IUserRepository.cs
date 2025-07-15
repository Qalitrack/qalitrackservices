using System.Collections;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        // Methods related to User's Shift
        Task<IEnumerable<UserShift>> GetUserShiftsAsync(string userId);  // Get all shifts assigned to a specific user
        Task<UserShift?> GetUserShiftByShiftIdAsync(string userId, string shiftId); // Get specific shift for a user by ShiftId
        Task<bool> AssignShiftToUserAsync(string userId, string shiftId);  // Assign a shift to a user
        Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId); // Remove a shift assignment for a user
        Task<bool> HasPermissionAsync(string userId, string permissionName);
        Task<IEnumerable> GetUserPermissionsAsync(string? toString);
        Task<IEnumerable> GetUsersByRoleAsync(string roleId);
        Task<IEnumerable<UserRole>> GetUserRolesAsync(string userId);
        Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> roleIds);
        Task<User> GetByEmailAsync(string toLower);
        Task<IEnumerable<User>> GetDeletedAsync();
        Task<bool> RestoreAsync(string id);
    }
}