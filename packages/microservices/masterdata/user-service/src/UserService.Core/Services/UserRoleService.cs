using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services
{
    public class UserRoleService(
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICacheService cacheService,
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

        public async Task<ServiceResult> AddUserToRoleAsync(string userId, string roleId)
        {
            // Validate that the user exists and is not soft deleted
            var user = await userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                logger.LogWarning("User {UserId} does not exist or is deleted", userId);
                return new ServiceResult { Success = false, Message = "User does not exist or is deleted" };
            }

            // Validate that the role exists and is active
            var role = await roleRepository.GetByIdAsync(roleId, true);
            if (role == null)
            {
                logger.LogWarning("Role {RoleId} does not exist", roleId);
                return new ServiceResult { Success = false, Message = "Role does not exist" };
            }

            if (!role.IsActive)
            {
                logger.LogWarning("Role {RoleId} is not active", roleId);
                return new ServiceResult { Success = false, Message = "Role is not active" };
            }

            var success = await userRoleRepository.AssignRoleToUserAsync(userId, roleId);
            if (success)
            {
                logger.LogInformation("Successfully assigned role {RoleId} to user {UserId}", roleId, userId);
                
                // Invalidate user permissions cache
                await cacheService.RemoveAsync($"user_permissions:{userId}");
                await cacheService.RemovePatternAsync($"user_permission:{userId}:*");
            }
            else
            {
                logger.LogWarning("Failed to assign role {RoleId} to user {UserId}", roleId, userId);
            }
            
            return new ServiceResult { Success = success, Message = success ? "User added to role." : "Failed to add user to role." };
        }

        public async Task<ServiceResult> RemoveUserFromRoleAsync(string userId, string roleId)
        {
            // Validate that the user exists and is not soft deleted
            var user = await userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                logger.LogWarning("User {UserId} does not exist or is deleted", userId);
                return new ServiceResult { Success = false, Message = "User does not exist or is deleted" };
            }

            var success = await userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
            
            if (success)
            {
                // Invalidate user permissions cache
                await cacheService.RemoveAsync($"user_permissions:{userId}");
                await cacheService.RemovePatternAsync($"user_permission:{userId}:*");
                logger.LogInformation("Successfully removed role {RoleId} from user {UserId} and cleared cache", roleId, userId);
                return new ServiceResult { Success = true, Message = "User removed from role." };
            }
            else
            {
                logger.LogWarning("User {UserId} is not assigned to role {RoleId}", userId, roleId);
                return new ServiceResult { Success = false, Message = "User is not assigned to this role." };
            }
        }
    }
}
