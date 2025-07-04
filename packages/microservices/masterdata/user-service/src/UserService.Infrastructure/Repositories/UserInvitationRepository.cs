using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class UserInvitationRepository : Repository<UserInvitation>, IUserInvitationRepository
{
    public UserInvitationRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<UserInvitation?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(i => i.Token == token);
    }

    public async Task<UserInvitation?> GetByEmailAndOrganizationAsync(string email, string organizationId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(i => i.Email == email && 
                                i.OrganizationId == organizationId && 
                                i.Status == InvitationStatus.Pending);
    }

    public async Task<IEnumerable<UserInvitation>> GetByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Where(i => i.OrganizationId == organizationId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetPendingInvitationsAsync(string organizationId)
    {
        return await _dbSet
            .Where(i => i.OrganizationId == organizationId && 
                       i.Status == InvitationStatus.Pending && 
                       i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserInvitation>> GetExpiredInvitationsAsync()
    {
        return await _dbSet
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<bool> HasPendingInvitationAsync(string email, string organizationId)
    {
        return await _dbSet
            .AnyAsync(i => i.Email == email && 
                          i.OrganizationId == organizationId && 
                          i.Status == InvitationStatus.Pending && 
                          i.ExpiresAt > DateTime.UtcNow);
    }

    public async Task MarkAsAcceptedAsync(string invitationId, string acceptedBy)
    {
        var invitation = await _dbSet.FindAsync(invitationId);
        if (invitation != null)
        {
            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedBy = acceptedBy;
            invitation.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(invitation);
        }
    }

    public async Task MarkAsExpiredAsync(string invitationId)
    {
        var invitation = await _dbSet.FindAsync(invitationId);
        if (invitation != null)
        {
            invitation.Status = InvitationStatus.Expired;
            invitation.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(invitation);
        }
    }

    public async Task CancelInvitationAsync(string invitationId)
    {
        var invitation = await _dbSet.FindAsync(invitationId);
        if (invitation != null)
        {
            invitation.Status = InvitationStatus.Cancelled;
            invitation.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(invitation);
        }
    }

    public async Task CleanupExpiredInvitationsAsync()
    {
        var expiredInvitations = await _dbSet
            .Where(i => i.Status == InvitationStatus.Pending && i.ExpiresAt <= DateTime.UtcNow)
            .ToListAsync();

        foreach (var invitation in expiredInvitations)
        {
            invitation.Status = InvitationStatus.Expired;
            invitation.UpdatedAt = DateTime.UtcNow;
        }

        _dbSet.UpdateRange(expiredInvitations);
    }
}