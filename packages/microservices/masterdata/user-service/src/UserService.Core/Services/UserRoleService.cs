using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.UserRole;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using AutoMapper;

namespace UserService.Core.Services
{
    public class UserRoleService(
        IUserRoleRepository userRoleRepository,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICacheService cacheService,
        IMapper mapper,
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
                // Comprehensive cache invalidation
                logger.LogInformation("Role {RoleId} assigned to user {UserId}, invalidating cache", roleId, userId);
                
                // Invalidate user permissions cache
                await cacheService.RemoveAsync($"user_permissions:{userId}");
                await cacheService.RemovePatternAsync($"user_permission:{userId}:*");
                
                // Invalidate user roles cache
                await cacheService.RemoveAsync($"user_roles:{userId}");
                
                // Invalidate the specific user cache
                await cacheService.RemoveAsync($"user:{userId}");
                
                // Invalidate any paged results that might include this user
                await cacheService.RemovePatternAsync("users:paged:*");
                
                // Flag that recent changes have been made
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(15));
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
                // Comprehensive cache invalidation
                logger.LogInformation("Role {RoleId} removed from user {UserId}, invalidating cache", roleId, userId);
                
                // Invalidate user permissions cache
                await cacheService.RemoveAsync($"user_permissions:{userId}");
                await cacheService.RemovePatternAsync($"user_permission:{userId}:*");
                
                // Invalidate user roles cache
                await cacheService.RemoveAsync($"user_roles:{userId}");
                
                // Invalidate the specific user cache
                await cacheService.RemoveAsync($"user:{userId}");
                
                // Invalidate any paged results that might include this user
                await cacheService.RemovePatternAsync("users:paged:*");
                
                // Flag that recent changes have been made
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(15));
                
                return new ServiceResult { Success = true, Message = "User removed from role." };
            }
            else
            {
                return new ServiceResult { Success = false, Message = "User is not assigned to this role." };
            }
        }

        public async Task<PagedResult<UserRoleDto>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            try
            {
                var pagedResult = await userRoleRepository.GetDeletedPagedAsync(parameters);
                
                var userRoleDtos = mapper.Map<List<UserRoleDto>>(pagedResult.Items);

                return new PagedResult<UserRoleDto>
                {
                    Items = userRoleDtos,
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    TotalCount = pagedResult.TotalCount
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving deleted user-role relationships");
                throw;
            }
        }
    }
}
