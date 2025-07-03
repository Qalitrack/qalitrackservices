using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterLicenseRepository : Repository<TransporterLicense>, ITransporterLicenseRepository
{
    public TransporterLicenseRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterLicense>> GetLicensesByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(l => l.TransporterId == transporterId && !l.IsDeleted)
            .OrderBy(l => l.LicenseType)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterLicense>> GetLicensesByTypeAsync(string transporterId, LicenseType type)
    {
        return await _dbSet
            .Where(l => l.TransporterId == transporterId && l.LicenseType == type && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterLicense>> GetActiveLicensesAsync(string transporterId)
    {
        return await _dbSet
            .Where(l => l.TransporterId == transporterId && l.Status == LicenseStatus.Active && !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterLicense>> GetExpiringLicensesAsync(string transporterId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(daysAhead);
        return await _dbSet
            .Where(l => l.TransporterId == transporterId && 
                       l.ExpiryDate <= cutoffDate && 
                       l.Status == LicenseStatus.Active && 
                       !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterLicense>> GetExpiredLicensesAsync(string transporterId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(l => l.TransporterId == transporterId && 
                       l.ExpiryDate < today && 
                       !l.IsDeleted)
            .ToListAsync();
    }

    public async Task<TransporterLicense?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(l => l.LicenseNumber == licenseNumber && !l.IsDeleted);
    }
}