using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserInvitationRepository : IRepository<UserInvitation>
{
    Task<UserInvitation?> GetByTokenAsync(string token);
    Task<UserInvitation?> GetByEmailAndOrganizationAsync(string email, string organizationId);
    Task<IEnumerable<UserInvitation>> GetByOrganizationAsync(string organizationId);
    Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(string organizationId);
    Task<IEnumerable<UserInvitation>> GetExpiredInvitationsAsync();
    Task<bool> HasPendingInvitationAsync(string email, string organizationId);
    Task MarkAsAcceptedAsync(string invitationId, string acceptedBy);
    Task MarkAsExpiredAsync(string invitationId);
    Task CancelInvitationAsync(string invitationId);
    Task CleanupExpiredInvitationsAsync();
}