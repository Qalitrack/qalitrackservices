using Microsoft.EntityFrameworkCore;
using OrganizationService.Core.Entities;
using OrganizationService.Core.Interfaces;
using OrganizationService.Infrastructure.Data;

namespace OrganizationService.Infrastructure.Repositories;

public class OrganizationSettingsRepository : Repository<OrganizationSettings>, IOrganizationSettingsRepository
{
    public OrganizationSettingsRepository(OrganizationDbContext context) : base(context) { }

    public async Task<OrganizationSettings?> GetByOrganizationIdAsync(string organizationId)
    {
        return await _dbSet
            .Include(os => os.Organization)
            .FirstOrDefaultAsync(os => os.OrganizationId == organizationId);
    }
}