using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IOrganizationUserRepository : IRepository<OrganizationUser>
{
    Task<OrganizationUser?> GetByUserAndOrganizationAsync(string userId, string organizationId);
    Task<IEnumerable<OrganizationUser>> GetByUserIdAsync(string userId);
    Task<IEnumerable<OrganizationUser>> GetByOrganizationIdAsync(string organizationId);
    Task<IEnumerable<OrganizationUser>> GetActiveByOrganizationAsync(string organizationId);
    Task<IEnumerable<OrganizationUser>> GetOrganizationOwnersAsync(string organizationId);
    Task<IEnumerable<OrganizationUser>> GetOrganizationAdminsAsync(string organizationId);
    Task<bool> IsUserInOrganizationAsync(string userId, string organizationId);
    Task<bool> IsUserOwnerAsync(string userId, string organizationId);
    Task<bool> IsUserAdminAsync(string userId, string organizationId);
    Task<int> GetOrganizationUserCountAsync(string organizationId);
    Task<int> GetActiveOrganizationUserCountAsync(string organizationId);
    Task RemoveUserFromOrganizationAsync(string userId, string organizationId);
    Task<IEnumerable<OrganizationUser>> GetPendingInvitationsAsync(string organizationId);
}