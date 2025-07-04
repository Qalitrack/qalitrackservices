using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleRegistrationRepository : Repository<VehicleRegistration>, IVehicleRegistrationRepository
{
    public VehicleRegistrationRepository(VehicleDbContext context) : base(context) { }

    public async Task<VehicleRegistration?> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet.FirstOrDefaultAsync(vr => vr.VehicleId == vehicleId);
    }

    public async Task<VehicleRegistration?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet.FirstOrDefaultAsync(vr => vr.RegistrationNumber == registrationNumber);
    }

    public async Task<List<VehicleRegistration>> GetExpiringRegistrationsAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(vr => vr.ExpiryDate <= beforeDate && vr.IsActive).ToListAsync();
    }

    public async Task<List<VehicleRegistration>> GetByIssuingAuthorityAsync(string authority)
    {
        return await _dbSet.Where(vr => vr.IssuingAuthority == authority).ToListAsync();
    }
}