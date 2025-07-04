using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
{
    public VehicleRepository(VehicleDbContext context) : base(context) { }

    public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.RegistrationNumber == registrationNumber);
    }

    public async Task<Vehicle?> GetByVINAsync(string vin)
    {
        return await _dbSet.FirstOrDefaultAsync(v => v.VIN == vin);
    }

    public async Task<List<Vehicle>> GetByOwnerAsync(string ownerName)
    {
        return await _dbSet.Where(v => v.OwnerName.Contains(ownerName)).ToListAsync();
    }

    public async Task<List<Vehicle>> GetByStatusAsync(VehicleStatus status)
    {
        return await _dbSet.Where(v => v.Status == status).ToListAsync();
    }

    public async Task<List<Vehicle>> GetByVehicleTypeAsync(string vehicleTypeId)
    {
        return await _dbSet.Where(v => v.VehicleTypeId == vehicleTypeId).ToListAsync();
    }

    public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null)
    {
        var query = _dbSet.Where(v => v.RegistrationNumber == registrationNumber);
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(v => v.Id != excludeId);
        }
        return !await query.AnyAsync();
    }

    public async Task<bool> IsVINUniqueAsync(string vin, string? excludeId = null)
    {
        var query = _dbSet.Where(v => v.VIN == vin);
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(v => v.Id != excludeId);
        }
        return !await query.AnyAsync();
    }

    public async Task<List<Vehicle>> GetExpiringInsuranceAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(v => v.InsuranceExpiryDate.HasValue && 
                                      v.InsuranceExpiryDate.Value <= beforeDate).ToListAsync();
    }

    public async Task<List<Vehicle>> GetExpiringRegistrationAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(v => v.RegistrationExpiryDate.HasValue && 
                                      v.RegistrationExpiryDate.Value <= beforeDate).ToListAsync();
    }

    public async Task<List<Vehicle>> GetDueForInspectionAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(v => v.NextInspectionDue.HasValue && 
                                      v.NextInspectionDue.Value <= beforeDate).ToListAsync();
    }

    public async Task<Vehicle?> GetWithDetailsAsync(string id)
    {
        return await _dbSet
            .Include(v => v.VehicleType)
            .Include(v => v.Registration)
            .Include(v => v.Specification)
            .Include(v => v.Documents)
            .Include(v => v.Inspections)
            .Include(v => v.InsurancePolicies)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}