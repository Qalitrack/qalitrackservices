using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Roles;

namespace UserService.Core.Interfaces
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