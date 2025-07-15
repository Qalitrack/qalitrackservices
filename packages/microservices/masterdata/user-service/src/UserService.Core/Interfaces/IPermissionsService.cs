using UserService.Core.DTOs;
using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;

namespace UserService.Core.Interfaces
{
    public interface IPermissionsService
    {
        // CRUD Operations for Permission
        Task<IEnumerable<PermissionDto>> GetAllAsync();  // Get all permissions
        Task<PermissionDto?> GetByIdAsync(string id);   // Get permission by ID
        Task<PermissionDto> CreateAsync(CreatePermissionDto dto);  // Create a new permission
        Task<PermissionDto?> UpdateAsync(string id, UpdatePermissionDto dto);  // Update an existing permission
        Task<bool> DeleteAsync(string id);  // Soft delete a permission (set IsDeleted = true)

        // Permission-Specific Operations
        Task<bool> DoesPermissionExistAsync(string name);  // Check if a permission with a specific name exists
        Task<IEnumerable<RoleDto>> GetRolesForPermissionAsync(string permissionId);  // Get all roles that have a specific permission
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);  // Assign a permission to a role
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);  // Remove a permission from a role
    }
}