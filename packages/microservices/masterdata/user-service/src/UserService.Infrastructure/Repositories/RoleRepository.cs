using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories
{
    public class RoleRepository : IRepository<Role>, IRoleRepository
    {
        private readonly UserServiceDbContext _context;

        public RoleRepository(UserServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Where(r => !r.IsDeleted)
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(string id, bool b)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Role> CreateAsync(Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.Roles.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Role?> UpdateAsync(Role entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var existingRole = await _context.Roles
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == entity.Id && !r.IsDeleted);

            if (existingRole == null)
                return null;

            // Only update specific fields to avoid constraint issues
            existingRole.Name = entity.Name;
            existingRole.Description = entity.Description; 
            existingRole.IsActive = entity.IsActive;
            existingRole.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return existingRole;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var role = await _context.Roles
                .Include(r => r.UserRoles)
                .AsTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (role == null)
                return false;

            if (role.UserRoles != null && role.UserRoles.Any())
            {
                throw new InvalidOperationException("Cannot delete role that is assigned to users.");
            } // public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        // {
        //     return await _context.RolePermissions
        //         .Where(rp => rp.RoleId == roleId)
        //         .Select(rp => rp.Permission)
        //         .ToListAsync();
        // }

            role.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.Trim().ToLower() && !r.IsDeleted);
        }

        public async Task<bool> DoesRoleExistAsync(string roleName)
        {
            return await _context.Roles
                .AnyAsync(r => r.Name == roleName && !r.IsDeleted);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        // public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
        // {
        //     var exists = await _context.RolePermissions
        //         .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        //
        //     if (exists)
        //         return true;
        //
        //     var rolePermission = new RolePermission
        //     {
        //         RoleId = roleId,
        //         PermissionId = permissionId,
        //         AssignedAt = DateTime.UtcNow
        //     };
        //
        //     await _context.RolePermissions.AddAsync(rolePermission);
        //     await _context.SaveChangesAsync();
        //     return true;
        // }

        // public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
        // {
        //     var rolePermission = await _context.RolePermissions
        //         .AsTracking()
        //         .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        //
        //     if (rolePermission == null)
        //         return false;
        //
        //     _context.RolePermissions.Remove(rolePermission);
        //     await _context.SaveChangesAsync();
        //     return true;
        // }

        public async Task<object> AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // public async Task GetByIdWithPermissionsAsync(string roleId)
        // {
        //     await _context.Roles
        //         .Include(r => r.RolePermissions)
        //             .ThenInclude(rp => rp.Permission)
        //         .FirstOrDefaultAsync(r => r.Id == roleId);
        // }

        public async Task<IEnumerable<Role>> GetByIdsAsync(IEnumerable<string> select)
        {
            return await _context.Roles
                .Where(r => select.Contains(r.Id))
                .ToListAsync(); 
        }

        public async Task<Role?> GetRoleWithPermissionsAsync(string roleName)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == roleName);
        }
    }
}