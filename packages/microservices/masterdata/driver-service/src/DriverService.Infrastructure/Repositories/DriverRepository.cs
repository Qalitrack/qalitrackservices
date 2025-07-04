using DriverService.Core.Entities;
using DriverService.Core.Interfaces;
using DriverService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DriverService.Infrastructure.Repositories;

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(DriverDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Driver>> GetActiveDriversAsync()
    {
        return await _dbSet
            .Where(d => d.Status == DriverStatus.Active)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Driver>> GetDriversByStatusAsync(DriverStatus status)
    {
        return await _dbSet
            .Where(d => d.Status == status)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Driver>> GetDriversWithExpiringLicensesAsync(int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        
        return await _dbSet
            .Include(d => d.License)
            .Where(d => d.License != null && 
                       d.License.ExpiryDate <= cutoffDate &&
                       d.License.Status == LicenseStatus.Active)
            .OrderBy(d => d.License!.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Driver>> SearchDriversAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        var term = searchTerm.ToLower();
        
        return await _dbSet
            .Where(d => d.FirstName.ToLower().Contains(term) ||
                       d.LastName.ToLower().Contains(term) ||
                       d.MiddleName.ToLower().Contains(term) ||
                       d.Email.ToLower().Contains(term) ||
                       d.EmployeeId.ToLower().Contains(term) ||
                       d.PhoneNumber.Contains(term))
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();
    }

    public async Task<Driver?> GetDriverByEmployeeIdAsync(string employeeId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.EmployeeId == employeeId);
    }

    public async Task<Driver?> GetDriverByEmailAsync(string email)
    {
        return await _dbSet
            .FirstOrDefaultAsync(d => d.Email.ToLower() == email.ToLower());
    }

    public async Task<Driver?> GetDriverWithLicenseAsync(string driverId)
    {
        return await _dbSet
            .Include(d => d.License)
            .FirstOrDefaultAsync(d => d.Id == driverId);
    }

    public async Task<Driver?> GetDriverWithProfileAsync(string driverId)
    {
        return await _dbSet
            .Include(d => d.Profile)
            .FirstOrDefaultAsync(d => d.Id == driverId);
    }

    public async Task<Driver?> GetDriverWithAllDetailsAsync(string driverId)
    {
        return await _dbSet
            .Include(d => d.License)
            .Include(d => d.Profile)
            .Include(d => d.Documents)
            .Include(d => d.Trainings)
            .Include(d => d.MedicalRecords)
            .Include(d => d.Violations)
            .Include(d => d.PerformanceRecords)
            .FirstOrDefaultAsync(d => d.Id == driverId);
    }
}