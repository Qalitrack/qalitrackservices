using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using DriverService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Repositories;

public class DriverProfileRepository : Repository<DriverProfile>, IDriverProfileRepository
{
    public DriverProfileRepository(DriverDbContext context) : base(context)
    {
    }

    public async Task<DriverProfile?> GetByDriverIdAsync(string driverId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.DriverId == driverId);
    }

    public async Task<IEnumerable<DriverProfile>> GetTopPerformersAsync(int count = 10)
    {
        return await _dbSet
            .Include(p => p.Driver)
            .OrderByDescending(p => p.PerformanceRating)
            .ThenByDescending(p => p.SafetyRating)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverProfile>> GetDriversByExperienceAsync(int minYears, int maxYears)
    {
        return await _dbSet
            .Include(p => p.Driver)
            .Where(p => p.YearsOfExperience >= minYears && p.YearsOfExperience <= maxYears)
            .OrderByDescending(p => p.YearsOfExperience)
            .ToListAsync();
    }
}