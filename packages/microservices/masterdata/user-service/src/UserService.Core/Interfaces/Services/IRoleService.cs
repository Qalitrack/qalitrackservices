using UserService.Core.DTOs.Roles;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.RolePermission;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Services
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllAsync();
        Task<RoleDto?> GetByIdAsync(string id);
        Task<RoleDto> CreateAsync(DTOs.Roles.CreateRoleDto dto);
        Task<RoleDto?> UpdateAsync(string id, DTOs.Roles.UpdateRoleDto dto);
        Task<bool> DeleteAsync(string id);
        Task<bool> AssignPermissionToRoleAsync(string roleId, string permissionId);
        Task<bool> RemovePermissionFromRoleAsync(string roleId, string permissionId);
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(string roleId);
        Task<PagedResult<RoleDto>> GetDeletedPagedAsync(PaginationParameters parameters);
        Task<PagedResult<RolePermissionDto>> GetDeletedRolePermissionsPagedAsync(PaginationParameters parameters);    }
}