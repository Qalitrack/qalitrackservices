using UserService.Core.DTOs.Roles;

namespace UserService.Core.Interfaces.Services
{
    public interface IRoleService
    {
        // CRUD Operations for Role
        Task<IEnumerable<RoleDto>> GetAllAsync();  // Get all roles
        Task<RoleDto?> GetByIdAsync(string id);   // Get role by ID
        Task<RoleDto> CreateAsync(DTOs.Roles.CreateRoleDto dto);  // Create a new role
        Task<RoleDto?> UpdateAsync(string id, DTOs.Roles.UpdateRoleDto dto);  // Update an existing role
        Task<bool> DeleteAsync(string id);  // Soft delete a role (set IsDeleted = true)

        // Role-Specific Operations
       // Task<bool> DoesRoleExistAsync(string roleName);  // Check if a role with a specific name exists
     //   Task<IEnumerable<PermissionDto>> GetPermissionsForRoleAsync(string roleId);  // Get all permissions assigned to a role
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);  // Assign a permission to a role
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);  // Remove a permission from a role
       // Task<IEnumerable<Role>> GetRolesByUserIdAsync(string userDtoId);
    }
}