using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface IPermissionsRepository : IRepository<Permission>  // Inheriting from IRepository<Permission>
    {
        Task<bool> DoesPermissionExistAsync(string name);
        
        Task<IEnumerable<Role>> GetRolesForPermissionAsync(string permissionId);
    }
}