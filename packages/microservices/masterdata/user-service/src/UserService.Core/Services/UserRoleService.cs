using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;

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

     
        public async Task<ServiceResult> AddUserToRoleAsync(string userId, string roleId)
        {
            // Validate that the user exists and is not soft deleted
            var user = await userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                return new ServiceResult { Success = false, Message = "User does not exist or is deleted" };
            }

            // Validate that the role exists and is active
            var role = await roleRepository.GetByIdAsync(roleId, true);
            if (role == null)
            {
                return new ServiceResult { Success = false, Message = "Role does not exist" };
            }

            if (!role.IsActive)
            {
                return new ServiceResult { Success = false, Message = "Role is not active" };
            }

            var success = await userRoleRepository.AssignRoleToUserAsync(userId, roleId);
            if (success)
            {
                
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
                return new ServiceResult { Success = false, Message = "User does not exist or is deleted" };
            }

            var success = await userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
            
            if (success)
            {
                // Invalidate user permissions cache
                await cacheService.RemoveAsync($"user_permissions:{userId}");
                await cacheService.RemovePatternAsync($"user_permission:{userId}:*");
                return new ServiceResult { Success = true, Message = "User removed from role." };
            }
            else
            {
                return new ServiceResult { Success = false, Message = "User is not assigned to this role." };
            }
        }
    }
}
