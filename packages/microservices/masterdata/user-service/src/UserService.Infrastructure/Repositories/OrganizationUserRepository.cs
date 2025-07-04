using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public class OrganizationUserRepository : Repository<OrganizationUser>, IOrganizationUserRepository
{
    public OrganizationUserRepository(UserDbContext context) : base(context)
    {
    }

    public async Task<OrganizationUser?> GetByUserAndOrganizationAsync(string userId, string organizationId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<OrganizationUser>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(ou => ou.UserId == userId)
            .OrderBy(ou => ou.JoinedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrganizationUser>> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.User)
            .Where(ou => ou.OrganizationId == organizationId)
            .OrderBy(ou => ou.User.FirstName)
            .ThenBy(ou => ou.User.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrganizationUser>> GetActiveByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.User)
            .Where(ou => ou.OrganizationId == organizationId && ou.Status == OrganizationUserStatus.Active)
            .OrderBy(ou => ou.User.FirstName)
            .ThenBy(ou => ou.User.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrganizationUser>> GetOrganizationOwnersAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.User)
            .Where(ou => ou.OrganizationId == organizationId && ou.IsOwner && ou.Status == OrganizationUserStatus.Active)
            .OrderBy(ou => ou.User.FirstName)
            .ThenBy(ou => ou.User.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrganizationUser>> GetOrganizationAdminsAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.User)
            .Where(ou => ou.OrganizationId == organizationId && 
                        (ou.IsAdmin || ou.IsOwner) && 
                        ou.Status == OrganizationUserStatus.Active)
            .OrderBy(ou => ou.User.FirstName)
            .ThenBy(ou => ou.User.LastName)
            .ToListAsync();
    }

    public async Task<bool> IsUserInOrganizationAsync(string userId, string organizationId)
    {
        return await _dbSet
            .AnyAsync(ou => ou.UserId == userId && 
                           ou.OrganizationId == organizationId && 
                           ou.Status == OrganizationUserStatus.Active);
    }

    public async Task<bool> IsUserOwnerAsync(string userId, string organizationId)
    {
        return await _dbSet
            .AnyAsync(ou => ou.UserId == userId && 
                           ou.OrganizationId == organizationId && 
                           ou.IsOwner && 
                           ou.Status == OrganizationUserStatus.Active);
    }

    public async Task<bool> IsUserAdminAsync(string userId, string organizationId)
    {
        return await _dbSet
            .AnyAsync(ou => ou.UserId == userId && 
                           ou.OrganizationId == organizationId && 
                           (ou.IsAdmin || ou.IsOwner) && 
                           ou.Status == OrganizationUserStatus.Active);
    }

    public async Task<int> GetOrganizationUserCountAsync(string organizationId)
    {
        return await _dbSet
            .CountAsync(ou => ou.OrganizationId == organizationId);
    }

    public async Task<int> GetActiveOrganizationUserCountAsync(string organizationId)
    {
        return await _dbSet
            .CountAsync(ou => ou.OrganizationId == organizationId && ou.Status == OrganizationUserStatus.Active);
    }

    public async Task RemoveUserFromOrganizationAsync(string userId, string organizationId)
    {
        var organizationUser = await _dbSet
            .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId);

        if (organizationUser != null)
        {
            organizationUser.Status = OrganizationUserStatus.Left;
            organizationUser.LeftAt = DateTime.UtcNow;
            organizationUser.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(organizationUser);
        }
    }

    public async Task<IEnumerable<OrganizationUser>> GetPendingInvitationsAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.User)
            .Where(ou => ou.OrganizationId == organizationId && ou.Status == OrganizationUserStatus.Invited)
            .OrderByDescending(ou => ou.InvitedAt)
            .ToListAsync();
    }
}