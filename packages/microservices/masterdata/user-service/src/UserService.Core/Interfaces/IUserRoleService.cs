using System.Collections.Generic;
using System.Threading.Tasks;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Roles;

namespace UserService.Core.Interfaces
{
    public interface IUserRoleService
    {
        // User-Role Management

        Task<object> AddUserToRoleAsync(string userId, string roleId);
        Task<object> RemoveUserFromRoleAsync(string userId, string roleId);
    }
}