using UserService.Core.DTOs.Permissions;
using UserService.Core.DTOs.Roles;
using UserService.Core.DTOs.Common; 

namespace UserService.Core.Interfaces.Services
{
    public interface IPermissionsService
    {
      
        Task<IEnumerable<PermissionDto>> GetAllAsync();  
        Task<PermissionDto> CreateAsync(DTOs.Permissions.CreatePermissionDto dto);  
        Task<PermissionDto?> UpdateAsync(string id, DTOs.Permissions.UpdatePermissionDto dto);  
        Task<bool> DeleteAsync(string id);  
        Task<IEnumerable<RoleDto>> GetRolesForPermissionAsync(string permissionId); 
        Task<PermissionDto?> GetByIdAsync(string id);

        Task<PagedResult<PermissionDto>> GetDeletedPagedAsync(PaginationParameters parameters);
    }
}