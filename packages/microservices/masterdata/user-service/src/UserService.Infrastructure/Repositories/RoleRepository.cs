using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Core.Services
{
    public class RoleRepository : IRoleRepository
    {
        private readonly UserServiceDbContext _context;

        public RoleRepository(UserServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // CRUD Operations (inherited from IRepository<Role>)
        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task<Role?> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(id));
                
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }

        public async Task<Role> CreateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role?> UpdateAsync(Role role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            var existingRole = await _context.Roles.FindAsync(role.Id);
            if (existingRole != null)
            {
                existingRole.Name = role.Name;
                existingRole.Description = role.Description;
                existingRole.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existingRole;
            }

            return null;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(id));

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role != null)
            {
                role.IsDeleted = true;
                role.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Role-Specific Methods
        public async Task<Role?> GetByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or whitespace", nameof(roleName));

            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower() && !r.IsDeleted);
        }

        public async Task<bool> DoesRoleExistAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be null or whitespace", nameof(roleName));

            return await _context.Roles
                .AnyAsync(r => r.Name.ToLower() == roleName.ToLower() && !r.IsDeleted);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));

            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));
            if (string.IsNullOrEmpty(permissionId))
                throw new ArgumentException("Permission ID cannot be null or empty", nameof(permissionId));

            // Check if the role exists and is not deleted
            var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId && !r.IsDeleted);
            if (!roleExists)
                return false;

            // Check if the permission exists
            var permissionExists = await _context.Permissions.AnyAsync(p => p.Id == permissionId);
            if (!permissionExists)
                return false;

            // Check if the role already has this permission
            var existingAssignment = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (existingAssignment != null)
                return true; // Already assigned

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId)
        {
            if (string.IsNullOrEmpty(roleId))
                throw new ArgumentException("Role ID cannot be null or empty", nameof(roleId));
            if (string.IsNullOrEmpty(permissionId))
                throw new ArgumentException("Permission ID cannot be null or empty", nameof(permissionId));

            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission != null)
            {
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
