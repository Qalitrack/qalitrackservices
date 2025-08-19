using UserService.Core.DTOs.Common;

namespace UserService.Core.Interfaces.Services
{
    public interface IUserRoleService
    {
        // User-Role Management
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
      //  Task<IEnumerable<string>> GetUsersInRoleAsync(string roleId);
     //   Task<bool> IsUserInRoleAsync(string userId, string roleId);
        Task<ServiceResult> AddUserToRoleAsync(string userId, string roleId);
        Task<ServiceResult> RemoveUserFromRoleAsync(string userId, string roleId);
    }
}