using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class WeightCorrectionRepository : Repository<WeightCorrection>, IWeightCorrectionRepository
{
    public WeightCorrectionRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeightCorrection>> GetByMeasurementIdAsync(Guid measurementId)
    {
        return await _dbSet
            .Where(c => c.WeightMeasurementId == measurementId)
            .Include(c => c.WeightMeasurement)
            .OrderByDescending(c => c.CorrectionDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightCorrection>> GetPendingApprovalsAsync(string organizationId)
    {
        return await _dbSet
            .Where(c => !c.IsApproved)
            .Include(c => c.WeightMeasurement.Where(w => w.OrganizationId == organizationId))
            .Where(c => c.WeightMeasurement.OrganizationId == organizationId)
            .OrderBy(c => c.CorrectionDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightCorrection>> GetByAuthorizedByAsync(string authorizedBy)
    {
        return await _dbSet
            .Where(c => c.AuthorizedBy == authorizedBy)
            .Include(c => c.WeightMeasurement)
            .OrderByDescending(c => c.CorrectionDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightCorrection>> GetRecentCorrectionsAsync(string organizationId, int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _dbSet
            .Where(c => c.CorrectionDateTime >= cutoffDate)
            .Include(c => c.WeightMeasurement.Where(w => w.OrganizationId == organizationId))
            .Where(c => c.WeightMeasurement.OrganizationId == organizationId)
            .OrderByDescending(c => c.CorrectionDateTime)
            .ToListAsync();
    }
}