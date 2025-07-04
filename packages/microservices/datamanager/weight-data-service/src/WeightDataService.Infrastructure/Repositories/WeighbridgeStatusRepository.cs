using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class WeighbridgeStatusRepository : Repository<WeighbridgeStatus>, IWeighbridgeStatusRepository
{
    public WeighbridgeStatusRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<WeighbridgeStatus?> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.WeighbridgeId == weighbridgeId);
    }

    public async Task<IEnumerable<WeighbridgeStatus>> GetByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeStatus>> GetActiveWeighbridgesAsync(string organizationId)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && 
                       w.Status == MaintenanceStatus.Active && 
                       w.IsOnline)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeStatus>> GetByStatusAsync(MaintenanceStatus status, string organizationId)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && w.Status == status)
            .OrderBy(w => w.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeStatus>> GetRequiringCalibrationAsync(string organizationId)
    {
        var today = DateTime.UtcNow.Date;
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && 
                       w.NextCalibrationDate <= today.AddDays(30))
            .OrderBy(w => w.NextCalibrationDate)
            .ToListAsync();
    }
}