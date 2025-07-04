using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeConfigurationRepository : Repository<WeighbridgeConfiguration>, IWeighbridgeConfigurationRepository
{
    public WeighbridgeConfigurationRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<WeighbridgeConfiguration?> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(c => c.WeighbridgeId == weighbridgeId && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeighbridgeConfiguration>> GetByConfigurationNameAsync(string configurationName)
    {
        return await _dbSet
            .Where(c => c.ConfigurationName.Contains(configurationName) && !c.IsDeleted)
            .ToListAsync();
    }
}