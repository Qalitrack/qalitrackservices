using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;
using OrganizationService.Infrastructure.Data;

namespace OrganizationService.Infrastructure.Repositories;

public class OrganizationUserRepository : Repository<OrganizationUser>, IOrganizationUserRepository
{
    public OrganizationUserRepository(OrganizationDbContext context) : base(context) { }

    public async Task<List<OrganizationUser>> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Include(ou => ou.Department)
            .Where(ou => ou.OrganizationId == organizationId)
            .OrderBy(ou => ou.FirstName)
            .ThenBy(ou => ou.LastName)
            .ToListAsync();
    }

    public async Task<OrganizationUser?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(ou => ou.Organization)
            .Include(ou => ou.Department)
            .FirstOrDefaultAsync(ou => ou.Email.ToLower() == email.ToLower());
    }

    public async Task<OrganizationUser?> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Include(ou => ou.Organization)
            .Include(ou => ou.Department)
            .FirstOrDefaultAsync(ou => ou.UserId == userId);
    }

    public async Task<List<OrganizationUser>> GetByRoleAsync(string organizationId, OrganizationRole role)
    {
        return await _dbSet
            .Include(ou => ou.Department)
            .Where(ou => ou.OrganizationId == organizationId && ou.Role == role)
            .OrderBy(ou => ou.FirstName)
            .ThenBy(ou => ou.LastName)
            .ToListAsync();
    }

    public async Task<bool> IsEmailUniqueInOrganizationAsync(string organizationId, string email, string? excludeId = null)
    {
        var query = _dbSet.Where(ou => ou.OrganizationId == organizationId && ou.Email.ToLower() == email.ToLower());
        
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(ou => ou.Id != excludeId);
        }

        return !await query.AnyAsync();
    }
}