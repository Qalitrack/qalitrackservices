using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using DriverService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Repositories;

public class DriverLicenseRepository : Repository<DriverLicense>, IDriverLicenseRepository
{
    public DriverLicenseRepository(DriverDbContext context) : base(context)
    {
    }

    public async Task<DriverLicense?> GetByDriverIdAsync(string driverId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.DriverId == driverId);
    }

    public async Task<DriverLicense?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.LicenseNumber == licenseNumber);
    }

    public async Task<IEnumerable<DriverLicense>> GetExpiringLicensesAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        return await _dbSet
            .Include(l => l.Driver)
            .Where(l => l.ExpiryDate <= cutoffDate && l.Status == LicenseStatus.Active)
            .OrderBy(l => l.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverLicense>> GetLicensesByStatusAsync(LicenseStatus status)
    {
        return await _dbSet
            .Include(l => l.Driver)
            .Where(l => l.Status == status)
            .OrderBy(l => l.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DriverLicense>> GetExpiredLicensesAsync()
    {
        return await _dbSet
            .Include(l => l.Driver)
            .Where(l => l.ExpiryDate < DateTime.UtcNow)
            .OrderBy(l => l.ExpiryDate)
            .ToListAsync();
    }
}