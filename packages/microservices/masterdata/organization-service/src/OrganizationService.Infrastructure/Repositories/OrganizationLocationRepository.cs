using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;
using OrganizationService.Infrastructure.Data;

namespace OrganizationService.Infrastructure.Repositories;

public class OrganizationLocationRepository : Repository<OrganizationLocation>, IOrganizationLocationRepository
{
    public OrganizationLocationRepository(OrganizationDbContext context) : base(context) { }

    public async Task<List<OrganizationLocation>> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Where(ol => ol.OrganizationId == organizationId)
            .OrderBy(ol => ol.Name)
            .ToListAsync();
    }

    public async Task<OrganizationLocation?> GetByCodeAsync(string organizationId, string code)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ol => ol.OrganizationId == organizationId && ol.Code == code);
    }

    public async Task<OrganizationLocation?> GetHeadquartersAsync(string organizationId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(ol => ol.OrganizationId == organizationId && ol.IsHeadquarters);
    }
}