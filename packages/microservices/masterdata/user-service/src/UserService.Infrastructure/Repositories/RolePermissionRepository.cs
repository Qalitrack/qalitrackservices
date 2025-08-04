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
        return await base.DeleteAsync(id);
    }
    

    public async Task<string?> GetByRoleAndPermissionAsync(string roleId, string permissionId)
    {
        return await context.RolePermissions
            .Where(rp => rp.RoleId == roleId && rp.PermissionId == permissionId)
            .Select(rp => rp.Id)
            .FirstOrDefaultAsync(); 
    }

    public async Task AddAsync(RolePermission rolePermission)
    {
        var currentUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;
    
        rolePermission.Id = Guid.NewGuid().ToString();
        rolePermission.CreatedAt = now;
        rolePermission.UpdatedAt = now;
        rolePermission.CreatedBy = currentUserId;
        rolePermission.UpdatedBy = currentUserId;
        rolePermission.IsDeleted = false;

        await context.RolePermissions.AddAsync(rolePermission);
    }
    public new async Task  SaveChangesAsync()
    {
        await context.SaveChangesAsync();
        
    }
}