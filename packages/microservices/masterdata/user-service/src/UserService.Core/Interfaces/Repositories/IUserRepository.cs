using UserService.Core.DTOs.Common;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        // Methods related to User's Shift
        Task<IEnumerable<UserShift>> GetUserShiftsAsync(string userId);  // Get all shifts assigned to a specific user
        Task<UserShift?> GetUserShiftByShiftIdAsync(string userId, string shiftId); // Get specific shift for a user by ShiftId
        Task<bool> AssignShiftToUserAsync(string userId, string shiftId);  // Assign a shift to a user
        Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId); // Remove a shift assignment for a user
        Task<bool> HasPermissionAsync(string userId, string permissionName);
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleId);
        // In IUserRepository.cs
        Task<User?> GetByEmailAsync(string toLower);
        Task<bool> RestoreAsync(string id);
        Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive);
        Task<PagedResult<User>> GetPagedAsync(PaginationParameters parameters);
        Task<PagedResult<User>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}