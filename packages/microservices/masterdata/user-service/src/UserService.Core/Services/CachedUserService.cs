using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace UserService.Core.Services;

public class CachedUserService(IUserService userService, ICacheService cacheService, ILogger<CachedUserService> logger) : IUserService
{
    private const int CacheExpirationMinutes = 15;

    // Helper method to invalidate all user-related caches
    private async Task InvalidateUserCachesAsync(string userId)
    {
        logger.LogInformation("Starting comprehensive cache invalidation for user {UserId}", userId);
        
        var cacheKeys = new[]
        {
            $"user:{userId}",
            $"user_roles:{userId}",
            $"user_permissions:{userId}"
        };

        var patternKeys = new[]
        {
            $"user_permission:{userId}:*",
            "users:*",
            "users:paged:*",
            "users:deleted:paged:*"
        };

        foreach (var key in cacheKeys)
        {
            logger.LogDebug("Removing cache key: {CacheKey}", key);
            await cacheService.RemoveAsync(key);
            var testCache = await cacheService.GetAsync<object>(key);
            if (testCache != null)
            {
                logger.LogWarning("Cache key {CacheKey} still exists after removal", key);
            }
        }

        foreach (var pattern in patternKeys)
        {
            logger.LogDebug("Removing cache pattern: {CachePattern}", pattern);
            await cacheService.RemovePatternAsync(pattern);
            logger.LogDebug("Cache pattern {Pattern} removal completed", pattern);
        }
        
    }

    public async Task<UserReadDto?> GetByIdAsync(string id)
    {
        var cacheKey = $"user:{id}";
        
        var cachedUser = await cacheService.GetAsync<UserReadDto>(cacheKey);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await userService.GetByIdAsync(id);
        if (user != null)
        {
            await cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }
        
        return user;
    }

    public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
    {
        
        // Use a global lock for user creation to prevent conflicts in user lists
        var lockKey = "lock:users:create";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for user creation");
            throw new InvalidOperationException("Concurrent user creation in progress");
        }
        
        try
        {
            // Pre-invalidate list caches
            await cacheService.RemovePatternAsync("users:*");
            await cacheService.RemovePatternAsync("users:paged:*");
            
            var user = await userService.CreateAsync(dto);
            
            logger.LogInformation("User {UserId} created successfully", user.Id);
            
            // Cache the new user
            var cacheKey = $"user:{user.Id}";
            await cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(CacheExpirationMinutes));
            
            // Thorough invalidation of lists to ensure new user appears
            await cacheService.RemovePatternAsync("users:*");
            await cacheService.RemovePatternAsync("users:paged:*");
            
            // Verify list caches are invalidated
            await VerifyCacheInvalidation("users:paged:*");
            
            // Indicate recent change for cache invalidation logic
            await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
            
            // Ensure all paged users caches are explicitly removed
            var pattern = "users:paged:*";
            await cacheService.RemovePatternAsync(pattern);
            
            // Also ensure timestamps are invalidated
            await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            
            return user;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto)
    {
        var cacheKey = $"user:{id}";
        var lockKey = $"lock:user:{id}";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for user {UserId}", id);
            throw new InvalidOperationException("Concurrent update in progress");
        }
        
        try
        {
            // Pre-emptively invalidate cache
            await InvalidateUserCachesAsync(id);
            
            var user = await userService.UpdateAsync(id, dto);
            
            if (user != null)
            {
                await cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("User {UserId} update returned null", id);
            }
            
            return user;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var lockKey = $"lock:user:{id}";
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for deleting user {UserId}", id);
            throw new InvalidOperationException("Concurrent delete operation in progress");
        }
        
        try
        {
            await InvalidateUserCachesAsync(id);
            var result = await userService.DeleteAsync(id);
            if (result)
            {
                await cacheService.RemovePatternAsync("users:*");
                await cacheService.RemovePatternAsync("users:paged:*");
                await cacheService.RemovePatternAsync("users:deleted:paged:*");
                await VerifyCacheInvalidation("users:paged:*");
                await VerifyCacheInvalidation("users:deleted:paged:*");
                var specificKeys = new[] 
                {
                    $"user:{id}",
                    $"user_roles:{id}",
                    $"user_permissions:{id}"
                };
                
                foreach (var key in specificKeys)
                {
                    await cacheService.RemoveAsync(key);
                    logger.LogDebug("Removed cache key: {Key}", key);
                }
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                var deletedPattern = "users:deleted:paged:*";
                await cacheService.RemovePatternAsync(deletedPattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
                await cacheService.RemovePatternAsync("users:deleted:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("Delete operation for user {UserId} returned false", id);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }
    
    // Helper method to verify cache invalidation
    private async Task VerifyCacheInvalidation(string pattern)
    {
        try
        {
            // Force invalidation again to be sure
            await cacheService.RemovePatternAsync(pattern);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cache invalidation verification for pattern: {Pattern}", pattern);
        }
    }

    public async Task<User?> ValidateUserCredentials(string email, string password)
    {
        // Don't cache authentication attempts for security reasons
        return await userService.ValidateUserCredentials(email, password);
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionName)
    {
        var cacheKey = $"user_permission:{userId}:{permissionName}";
        logger.LogDebug("Checking permission cache for key: {CacheKey}", cacheKey);
        
        var cachedResult = await cacheService.GetAsync<string>(cacheKey);
        if (cachedResult != null && bool.TryParse(cachedResult, out var cachedValue))
        {
            logger.LogDebug("Permission cache HIT for {CacheKey}", cacheKey);
            return cachedValue;
        }

        logger.LogDebug("Permission cache MISS for {CacheKey}", cacheKey);
        var result = await userService.HasPermissionAsync(userId, permissionName);
        await cacheService.SetAsync(cacheKey, result.ToString(), TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"user_permissions:{userId}";
        logger.LogDebug("Getting permissions for user {UserId} - cache key: {CacheKey}", userId, cacheKey);
        
        var cachedPermissions = await cacheService.GetAsync<IEnumerable<Permission>>(cacheKey);
        if (cachedPermissions != null)
        {
            logger.LogDebug("Permissions cache HIT for user {UserId}", userId);
            return cachedPermissions;
        }

        logger.LogDebug("Permissions cache MISS for user {UserId}", userId);
        var permissions = await userService.GetUserPermissionsAsync(userId);
        var userPermissionsAsync = permissions as Permission[] ?? permissions.ToArray();
        await cacheService.SetAsync(cacheKey, userPermissionsAsync, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return userPermissionsAsync;
    }

    public async Task<bool> RestoreAsync(string id)
    {
        var lockKey = $"lock:user:{id}";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            throw new InvalidOperationException("Concurrent restore operation in progress");
        }
        
        try
        {
            // Pre-emptively invalidate cache for this specific user
            await InvalidateUserCachesAsync(id);
            
            var result = await userService.RestoreAsync(id);
            
            if (result)
            {
                await cacheService.RemovePatternAsync("users:*");
                await cacheService.RemovePatternAsync("users:paged:*");
                await cacheService.RemovePatternAsync("users:deleted:paged:*");
                
                await VerifyCacheInvalidation("users:paged:*");
                await VerifyCacheInvalidation("users:deleted:paged:*");
                
                var specificKeys = new[] 
                {
                    $"user:{id}",
                    $"user_roles:{id}",
                    $"user_permissions:{id}"
                };
                
                foreach (var key in specificKeys)
                {
                    await cacheService.RemoveAsync(key);
                    logger.LogDebug("Removed cache key: {Key}", key);
                }
                
                var user = await userService.GetByIdAsync(id);
                if (user != null)
                {
                    await cacheService.SetAsync($"user:{id}", user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
                var deletedPattern = "users:deleted:paged:*";
                await cacheService.RemovePatternAsync(deletedPattern);
                await cacheService.RemovePatternAsync("users:deleted:paged:*:timestamp");
                
            }
            else
            {
                logger.LogWarning("Restore operation for user {UserId} returned false", id);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto)
    {
        var lockKey = $"lock:user:{userId}";
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for password update for user {UserId}", userId);
            throw new InvalidOperationException("Concurrent password update in progress");
        }
        
        try
        {
            await InvalidateUserCachesAsync(userId);
            var user = await userService.UpdatePassword(userId, dto);
            var cacheKey = $"user:{userId}";
            await cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(CacheExpirationMinutes));
            await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return user;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<PagedResult<UserReadDto>> GetPagedAsync(PaginationParameters parameters)
    {
        var cacheKey = $"users:paged:{parameters.Page}:{parameters.PageSize}:{parameters.Search ?? "null"}:{parameters.SortBy ?? "null"}:{parameters.SortDescending}";
        var recentChangeKey = "users:recent_change";
        var recentChange = await cacheService.GetAsync<DateTime?>(recentChangeKey);
        var cacheAge = TimeSpan.Zero;
        
        var cachedResult = await cacheService.GetAsync<PagedResult<UserReadDto>>(cacheKey);
        if (cachedResult != null)
        {
            if (recentChange.HasValue)
            {
                var cacheTimestampKey = $"{cacheKey}:timestamp";
                var cacheTimestamp = await cacheService.GetAsync<DateTime?>(cacheTimestampKey);
                if (cacheTimestamp.HasValue)
                {
                    if (cacheTimestamp.Value < recentChange.Value)
                    {
                        cachedResult = null; // Force refresh
                    }
                    else
                    {
                        cacheAge = DateTime.UtcNow - cacheTimestamp.Value;
                    }
                }
            }
            
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        var result = await userService.GetPagedAsync(parameters);
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        // Store cache timestamp for invalidation checks
        await cacheService.SetAsync($"{cacheKey}:timestamp", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<PagedResult<UserReadDto>> GetDeletedPagedAsync(PaginationParameters parameters)
    {
        var cacheKey = $"users:deleted:paged:{parameters.Page}:{parameters.PageSize}:{parameters.Search ?? "null"}:{parameters.SortBy ?? "null"}:{parameters.SortDescending}";
        // Check if we had a recent delete or restore operation
        var recentChangeKey = "users:recent_change";
        var recentChange = await cacheService.GetAsync<DateTime?>(recentChangeKey);
        var cacheAge = TimeSpan.Zero;
        
        var cachedResult = await cacheService.GetAsync<PagedResult<UserReadDto>>(cacheKey);
        if (cachedResult != null)
        {
            // If we have both a recent change and a cached result
            if (recentChange.HasValue)
            {
                // Get cache metadata if available
                var cacheTimestampKey = $"{cacheKey}:timestamp";
                var cacheTimestamp = await cacheService.GetAsync<DateTime?>(cacheTimestampKey);
                
                if (cacheTimestamp.HasValue)
                {
                    // If the cache was created before the most recent change, consider it stale
                    if (cacheTimestamp.Value < recentChange.Value)
                    {
                        cachedResult = null; // Force refresh
                    }
                    else
                    {
                        cacheAge = DateTime.UtcNow - cacheTimestamp.Value;
                    }
                }
            }
            
            if (cachedResult != null)
            {
                return cachedResult;
            }
        }

        var result = await userService.GetDeletedPagedAsync(parameters);
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        // Store cache timestamp for invalidation checks
        await cacheService.SetAsync($"{cacheKey}:timestamp", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    // Shift-related methods - delegate without caching for now
    public async Task<IEnumerable<UserShiftDto>> GetUserShiftsAsync(string userId)
    {
        return await userService.GetUserShiftsAsync(userId);
    }

    public async Task<UserShiftDto?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
    {
        return await userService.GetUserShiftByShiftIdAsync(userId, shiftId);
    }

    public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
    {
        var lockKey = $"lock:user:{userId}";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for shift assignment for user {UserId}", userId);
            throw new InvalidOperationException("Concurrent operation in progress");
        }
        
        try
        {
            await InvalidateUserCachesAsync(userId);
            
            var result = await userService.AssignShiftToUserAsync(userId, shiftId);
            
            if (result)
            {
                var user = await userService.GetByIdAsync(userId);
                if (user != null)
                {
                    await cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("Failed to assign shift {ShiftId} to user {UserId}", shiftId, userId);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
            logger.LogInformation("Lock released for user {UserId} after shift assignment", userId);
        }
    }

    public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
    {
        logger.LogInformation("Starting shift removal for user {UserId}", userId);
        
        var lockKey = $"lock:user:{userId}";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for shift removal for user {UserId}", userId);
            throw new InvalidOperationException("Concurrent operation in progress");
        }
        
        try
        {
            await InvalidateUserCachesAsync(userId);
            
            var result = await userService.RemoveShiftFromUserAsync(userId, shiftId);
            
            if (result)
            {
                logger.LogInformation("Shift {ShiftId} removed successfully from user {UserId}", shiftId, userId);
                
                var user = await userService.GetByIdAsync(userId);
                if (user != null)
                {
                    await cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("Failed to remove shift {ShiftId} from user {UserId}", shiftId, userId);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
            logger.LogInformation("Lock released for user {UserId} after shift removal", userId);
        }
    }

    public async Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive)
    {
        
        var lockKey = $"lock:user:{userId}";
        
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for updating active status for user {UserId}", userId);
            throw new InvalidOperationException("Concurrent operation in progress");
        }
        
        try
        {
            // Pre-emptively invalidate cache
            await InvalidateUserCachesAsync(userId);
            
            var result = await userService.UpdateUserActiveStatusAsync(userId, isActive);
            
            if (result)
            {
                await InvalidateUserCachesAsync(userId);
                var user = await userService.GetByIdAsync(userId);
                if (user != null)
                {
                    await cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("Failed to update active status for user {UserId}", userId);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleName)
    {
        // This runs once per role on every policy-protected request
        // (PermissionAuthorizationHandler loops over the caller's roles) —
        // uncached, it was the one method on this class that didn't actually
        // cache anything despite the class's whole purpose being caching.
        // Role-permission mappings change rarely, so the same TTL as the
        // other permission caches is fine.
        var cacheKey = $"role_permissions:{roleName}";

        var cached = await cacheService.GetAsync<IEnumerable<string>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var permissions = await userService.GetPermissionsForRoleAsync(roleName);
        var permissionsArray = permissions as string[] ?? permissions.ToArray();
        await cacheService.SetAsync(cacheKey, permissionsArray, TimeSpan.FromMinutes(CacheExpirationMinutes));

        return permissionsArray;
    }

    public async Task<IEnumerable<Role>> GetUserRolesByUserIdAsync(string userId)
    {
        var cacheKey = $"user_roles:{userId}";
        var cachedRoles = await cacheService.GetAsync<IEnumerable<Role>>(cacheKey);
        if (cachedRoles != null)
        {
            return cachedRoles;
        }

        var roles = await userService.GetUserRolesByUserIdAsync(userId);
        await cacheService.SetAsync(cacheKey, roles, TimeSpan.FromMinutes(CacheExpirationMinutes));
    
        return roles;
    }

    public async Task<string?> ResetUserPasswordAsync(string userId)
    {
        var lockKey = $"lock:user:{userId}";
        bool acquired = await cacheService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(10));
        if (!acquired)
        {
            logger.LogWarning("Could not acquire lock for password reset for user {UserId}", userId);
            throw new InvalidOperationException("Concurrent password reset in progress");
        }
        
        try
        {
            // Pre-emptively invalidate user cache
            await InvalidateUserCachesAsync(userId);
            
            var result = await userService.ResetUserPasswordAsync(userId);

            if (result != null)
            {
                
                await InvalidateUserCachesAsync(userId);
                var user = await userService.GetByIdAsync(userId);
                if (user != null)
                {
                    await cacheService.SetAsync($"user:{userId}", user, TimeSpan.FromMinutes(CacheExpirationMinutes));
                }
                
                await cacheService.SetAsync("users:recent_change", DateTime.UtcNow, TimeSpan.FromMinutes(CacheExpirationMinutes));
                var pattern = "users:paged:*";
                await cacheService.RemovePatternAsync(pattern);
                await cacheService.RemovePatternAsync("users:paged:*:timestamp");
            }
            else
            {
                logger.LogWarning("Password reset failed for user {UserId}", userId);
            }
            
            return result;
        }
        finally
        {
            await cacheService.ReleaseLockAsync(lockKey);
        }
    }
}