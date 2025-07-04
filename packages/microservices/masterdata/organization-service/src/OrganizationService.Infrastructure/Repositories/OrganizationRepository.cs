using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;
using OrganizationService.Infrastructure.Data;

namespace OrganizationService.Infrastructure.Repositories;

public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(OrganizationDbContext context) : base(context) { }

    public async Task<Organization?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Include(o => o.ChildOrganizations)
            .Include(o => o.Settings)
            .Include(o => o.OrganizationUsers)
            .FirstOrDefaultAsync(o => o.Code == code);
    }

    public async Task<List<Organization>> GetChildOrganizationsAsync(string parentId)
    {
        return await _dbSet
            .Where(o => o.ParentOrganizationId == parentId)
            .Include(o => o.ChildOrganizations)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<List<Organization>> GetOrganizationHierarchyAsync(string organizationId)
    {
        var organization = await _dbSet
            .Include(o => o.ChildOrganizations)
            .ThenInclude(c => c.ChildOrganizations)
            .FirstOrDefaultAsync(o => o.Id == organizationId);

        var result = new List<Organization>();
        if (organization != null)
        {
            result.Add(organization);
            await GetChildrenRecursively(organization, result);
        }

        return result;
    }

    private async Task GetChildrenRecursively(Organization parent, List<Organization> result)
    {
        var children = await GetChildOrganizationsAsync(parent.Id);
        result.AddRange(children);

        foreach (var child in children)
        {
            await GetChildrenRecursively(child, result);
        }
    }

    public async Task<bool> IsCodeUniqueAsync(string code, string? excludeId = null)
    {
        var query = _dbSet.Where(o => o.Code == code);
        
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(o => o.Id != excludeId);
        }

        return !await query.AnyAsync();
    }

    public async Task<List<Organization>> GetByStatusAsync(OrganizationStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<int> GetUserCountAsync(string organizationId)
    {
        return await _context.OrganizationUsers
            .CountAsync(ou => ou.OrganizationId == organizationId && ou.Status == UserStatus.Active);
    }

    public override async Task<Organization?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Include(o => o.ParentOrganization)
            .Include(o => o.ChildOrganizations)
            .Include(o => o.Settings)
            .Include(o => o.OrganizationUsers)
            .Include(o => o.Departments)
            .Include(o => o.Locations)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public override async Task<List<Organization>> GetAllAsync()
    {
        return await _dbSet
            .Include(o => o.ParentOrganization)
            .Include(o => o.ChildOrganizations)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }
}