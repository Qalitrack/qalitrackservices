using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeCapacityRepository : Repository<WeighbridgeCapacity>, IWeighbridgeCapacityRepository
{
    public WeighbridgeCapacityRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<WeighbridgeCapacity?> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(c => c.WeighbridgeId == weighbridgeId && !c.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeighbridgeCapacity>> GetAvailableCapacitiesAsync()
    {
        return await _dbSet
            .Where(c => c.IsAvailable && !c.IsDeleted)
            .OrderBy(c => c.UsagePercentage)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeCapacity>> GetOverCapacityAsync()
    {
        return await _dbSet
            .Where(c => c.IsOverCapacity && !c.IsDeleted)
            .OrderByDescending(c => c.UsagePercentage)
            .ToListAsync();
    }
}