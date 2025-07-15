using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services
{
    public class UserRoleService(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        ILogger<UserRoleService> logger)
        : IUserRoleService
    {
        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var allUserRoles = await userRoleRepository.GetAllAsync();
            return allUserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUsersInRoleAsync(string roleId)
        {
            var allUserRoles = await userRoleRepository.GetAllAsync();
            return allUserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToList();
        }

        public async Task<bool> IsUserInRoleAsync(string userId, string roleId)
        {
            var allUserRoles = await userRoleRepository.GetAllAsync();
            return allUserRoles.Any(ur => ur.UserId == userId && ur.RoleId == roleId);
        }

        public async Task<object> AddUserToRoleAsync(string userId, string roleId)
        {
            // Validate that the role exists and is active
            var role = await roleRepository.GetByIdAsync(roleId, true);
            if (role == null)
            {
                logger.LogWarning("Role {RoleId} does not exist", roleId);
                return new { Success = false, Message = "Role does not exist" };
            }

            if (!role.IsActive)
            {
                logger.LogWarning("Role {RoleId} is not active", roleId);
                return new { Success = false, Message = "Role is not active" };
            }

            var success = await userRoleRepository.AssignRoleToUserAsync(userId, roleId);
            if (success)
            {
                logger.LogInformation("Successfully assigned role {RoleId} to user {UserId}", roleId, userId);
            }
            else
            {
                logger.LogWarning("Failed to assign role {RoleId} to user {UserId}", roleId, userId);
            }
            
            return new { Success = success, Message = success ? "User added to role." : "Failed to add user to role." };
        }

        public async Task<object> RemoveUserFromRoleAsync(string userId, string roleId)
        {
            var success = await userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
            return new { Success = success, Message = success ? "User removed from role." : "Failed to remove user from role." };
        }
    }
}
