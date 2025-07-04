using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterDriverRepository : Repository<TransporterDriver>, ITransporterDriverRepository
{
    public TransporterDriverRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterDriver>> GetDriversByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(d => d.TransporterId == transporterId && !d.IsDeleted)
            .OrderBy(d => d.FirstName)
            .ThenBy(d => d.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterDriver>> GetActiveDriversAsync(string transporterId)
    {
        return await _dbSet
            .Where(d => d.TransporterId == transporterId && d.Status == DriverStatus.Active && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterDriver>> GetDriversByStatusAsync(string transporterId, DriverStatus status)
    {
        return await _dbSet
            .Where(d => d.TransporterId == transporterId && d.Status == status && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<TransporterDriver?> GetByDriverIdAsync(string driverId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.DriverId == driverId && !d.IsDeleted);
    }

    public async Task<TransporterDriver?> GetByLicenseNumberAsync(string licenseNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.LicenseNumber == licenseNumber && !d.IsDeleted);
    }

    public async Task<IEnumerable<TransporterDriver>> GetDriversDueForMedicalCheckAsync(string transporterId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(d => d.TransporterId == transporterId && 
                       d.NextMedicalCheckDate.HasValue && 
                       d.NextMedicalCheckDate.Value <= today.AddDays(30) && 
                       !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsDriverAssignedAsync(string driverId)
    {
        return await _dbSet
            .AnyAsync(d => d.DriverId == driverId && d.Status == DriverStatus.Active && !d.IsDeleted);
    }
}