using Microsoft.EntityFrameworkCore;
using UserModule.Data;
using UserModule.Dtos;
using UserModule.Dtos.Users;

namespace UserModule.Services;

public class PermissionService(AppDbContext context) : IPermissionService
{
    public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionName)
    {
        return await context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .AnyAsync(ur => ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permissionName));
    }

    public async Task<bool> UserHasRoleAsync(Guid userId, string roleName)
    {
        return await context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == roleName && ur.Role.IsActive);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        return await context.UserRoles
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && ur.Role.IsActive)
            .Select(ur => ur.Role.Name)
            .ToListAsync();
    }

    public async Task<UserPermissionsDto> GetUserPermissionsDetailAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found");

        var roles = await GetUserRolesAsync(userId);
        var permissions = await GetUserPermissionsAsync(userId);

        return new UserPermissionsDto
        {
            UserId = userId,
            UserName = user.Username,
            Roles = roles,
            Permissions = permissions
        };
    }
}