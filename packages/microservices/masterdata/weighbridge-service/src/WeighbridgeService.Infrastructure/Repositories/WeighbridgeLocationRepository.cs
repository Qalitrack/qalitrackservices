using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeLocationRepository : Repository<WeighbridgeLocation>, IWeighbridgeLocationRepository
{
    public WeighbridgeLocationRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<WeighbridgeLocation?> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(l => l.WeighbridgeId == weighbridgeId && !l.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeighbridgeLocation>> GetByCityAsync(string city)
    {
        return await _dbSet
            .Where(l => l.City.Contains(city) && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeLocation>> GetByStateAsync(string state)
    {
        return await _dbSet
            .Where(l => l.State.Contains(state) && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeLocation>> GetByCountryAsync(string country)
    {
        return await _dbSet
            .Where(l => l.Country.Contains(country) && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeLocation>> GetActiveLocationsAsync()
    {
        return await _dbSet
            .Where(l => l.IsActive && !l.IsDeleted)
            .ToListAsync();
    }
}