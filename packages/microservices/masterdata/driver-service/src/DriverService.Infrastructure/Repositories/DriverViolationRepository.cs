using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using DriverService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Repositories;

public class DriverViolationRepository : Repository<DriverViolation>, IDriverViolationRepository
{
    public DriverViolationRepository(DriverDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DriverViolation>> GetByDriverIdAsync(string driverId)
    {
        return await _dbSet
            .Where(v => v.DriverId == driverId)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverViolation>> GetBySeverityAsync(ViolationSeverity severity)
    {
        return await _dbSet
            .Include(v => v.Driver)
            .Where(v => v.Severity == severity)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverViolation>> GetUnpaidViolationsAsync()
    {
        return await _dbSet
            .Include(v => v.Driver)
            .Where(v => !v.IsPaid && v.FineAmount > 0)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverViolation>> GetRecentViolationsAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        return await _dbSet
            .Include(v => v.Driver)
            .Where(v => v.ViolationDate >= cutoffDate)
            .OrderByDescending(v => v.ViolationDate)
            .ToListAsync();
    }
}