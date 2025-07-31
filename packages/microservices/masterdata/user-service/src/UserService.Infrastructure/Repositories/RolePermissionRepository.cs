using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class RolePermissionRepository(UserServiceDbContext dbContext, UserServiceDbContext context)
    : Repository<RolePermission>(dbContext), IRolePermissionRepository
{
    public async Task<IEnumerable<RolePermission>> GetAllAsync()
    {
        return await context.RolePermissions.ToListAsync();
    }

    public async Task<RolePermission?> GetByIdAsync(string id, bool b)
    {
        return await context.RolePermissions.FirstOrDefaultAsync(rp => rp.Id == id);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await context.RolePermissions
            .Where(rp => rp.Id == id)
            .ExecuteDeleteAsync() > 0;  
    }

    /*public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }*/

    /*public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
    {
        return await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);    
    }*/

    /*public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
    {
        return await context.RolePermissions
            .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);       
    }
    */

    public async Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.PermissionId == permissionId)
            .Select(rp => rp.Id)
            .FirstOrDefaultAsync(); 
    }

    public async Task AddAsync(RolePermission rolePermission)
    {
        await context.RolePermissions.AddAsync(rolePermission);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
        
    }
}