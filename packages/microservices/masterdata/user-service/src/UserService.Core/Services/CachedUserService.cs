using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class CachedUserService : IUserService
{
    private readonly IUserService _userService;
    private readonly ICacheService _cacheService;
    private const int CacheExpirationMinutes = 15;

    public CachedUserService(IUserService userService, ICacheService cacheService)
    {
        _userService = userService;
        _cacheService = cacheService;
    }


    public async Task<UserReadDto?> GetByIdAsync(string id)
    {
        var cacheKey = $"user:{id}";
        
        var cachedUser = await _cacheService.GetAsync<UserReadDto>(cacheKey);
        if (cachedUser != null)
        {
            return cachedUser;
        }

        var user = await _userService.GetByIdAsync(id);
        if (user != null)
        {
            await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(CacheExpirationMinutes));
        }
        
        return user;
    }

    public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
    {
        var user = await _userService.CreateAsync(dto);
        
        // Invalidate cache
        await _cacheService.RemovePatternAsync("users:");
        
        return user;
    }

    public async Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto)
    {
        var user = await _userService.UpdateAsync(id, dto);
        
        if (user != null)
        {
            // Invalidate specific user cache and list caches
            await _cacheService.RemoveAsync($"user:{id}");
            await _cacheService.RemovePatternAsync("users:");
        }
        
        return user;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _userService.DeleteAsync(id);
        
        if (result)
        {
            // Invalidate specific user cache and list caches
            await _cacheService.RemoveAsync($"user:{id}");
            await _cacheService.RemovePatternAsync("users:");
        }
        
        return result;
    }

    public async Task<User?> ValidateUserCredentials(string email, string password)
    {
        // Don't cache authentication attempts for security reasons
        return await _userService.ValidateUserCredentials(email, password);
    }

    public async Task<bool> HasPermissionAsync(string userId, string permissionName)
    {
        var cacheKey = $"user_permission:{userId}:{permissionName}";
        
        var cachedResult = await _cacheService.GetAsync<string>(cacheKey);
        if (cachedResult != null && bool.TryParse(cachedResult, out var cachedValue))
        {
            return cachedValue;
        }

        var result = await _userService.HasPermissionAsync(userId, permissionName);
        await _cacheService.SetAsync(cacheKey, result.ToString(), TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
    {
        var cacheKey = $"user_permissions:{userId}";
        
        var cachedPermissions = await _cacheService.GetAsync<IEnumerable<Permission>>(cacheKey);
        if (cachedPermissions != null)
        {
            return cachedPermissions;
        }

        var permissions = await _userService.GetUserPermissionsAsync(userId);
        await _cacheService.SetAsync(cacheKey, permissions, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return permissions;
    }

    public async Task<bool> RestoreAsync(string id)
    {
        var result = await _userService.RestoreAsync(id);
        
        if (result)
        {
            // Invalidate caches
            await _cacheService.RemovePatternAsync("users:");
        }
        
        return result;
    }


    public async Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto)
    {
        var user = await _userService.UpdatePassword(userId, dto);
        
        // Invalidate user cache
        await _cacheService.RemoveAsync($"user:{userId}");
        
        return user;
    }

    public async Task<PagedResult<UserReadDto>> GetPagedAsync(PaginationParameters parameters)
    {
        // Create cache key based on parameters
        var cacheKey = $"users:paged:{parameters.Page}:{parameters.PageSize}:{parameters.Search}:{parameters.SortBy}:{parameters.SortDescending}";
        
        var cachedResult = await _cacheService.GetAsync<PagedResult<UserReadDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var result = await _userService.GetPagedAsync(parameters);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    public async Task<PagedResult<UserReadDto>> GetDeletedPagedAsync(PaginationParameters parameters)
    {
        // Create cache key based on parameters
        var cacheKey = $"users:deleted:paged:{parameters.Page}:{parameters.PageSize}:{parameters.Search}:{parameters.SortBy}:{parameters.SortDescending}";
        
        var cachedResult = await _cacheService.GetAsync<PagedResult<UserReadDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var result = await _userService.GetDeletedPagedAsync(parameters);
        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
        
        return result;
    }

    // Shift-related methods - delegate without caching for now
    public async Task<IEnumerable<UserShiftDto>> GetUserShiftsAsync(string userId)
    {
        return await _userService.GetUserShiftsAsync(userId);
    }

    public async Task<UserShiftDto?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
    {
        return await _userService.GetUserShiftByShiftIdAsync(userId, shiftId);
    }

    public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
    {
        var result = await _userService.AssignShiftToUserAsync(userId, shiftId);
        
        if (result)
        {
            // Invalidate user cache
            await _cacheService.RemoveAsync($"user:{userId}");
        }
        
        return result;
    }

    public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
    {
        var result = await _userService.RemoveShiftFromUserAsync(userId, shiftId);
        
        if (result)
        {
            // Invalidate user cache
            await _cacheService.RemoveAsync($"user:{userId}");
        }
        
        return result;
    }

    public async Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive)
    {
        var result = await _userService.UpdateUserActiveStatusAsync(userId, isActive);
        
        if (result)
        {
            // Invalidate user cache
            await _cacheService.RemoveAsync($"user:{userId}");
        }
        
        return result;
    }

    public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleName)
    {
        var result = await _userService.GetPermissionsForRoleAsync(roleName);
        return result;
    }
}