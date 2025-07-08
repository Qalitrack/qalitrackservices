using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<Permission?> GetByNameAsync(string name);
    Task<IEnumerable<Permission>> GetByResourceAsync(string resource);
    Task<IEnumerable<Permission>> GetByActionAsync(string action);
    Task<IEnumerable<Permission>> GetByScopeAsync(PermissionScope scope);
    Task<IEnumerable<Permission>> GetActivePermissionsAsync();
    Task<bool> IsPermissionNameAvailableAsync(string name);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<IEnumerable<Permission>> GetUserPermissionsByOrganizationAsync(string userId, string organizationId);
}