using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleInsuranceRepository : Repository<VehicleInsurance>, IVehicleInsuranceRepository
{
    public VehicleInsuranceRepository(VehicleDbContext context) : base(context) { }

    public async Task<List<VehicleInsurance>> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet.Where(vi => vi.VehicleId == vehicleId && vi.IsActive)
                          .OrderByDescending(vi => vi.StartDate)
                          .ToListAsync();
    }

    public async Task<VehicleInsurance?> GetActiveInsuranceAsync(string vehicleId)
    {
        var currentDate = DateTime.UtcNow;
        return await _dbSet.Where(vi => vi.VehicleId == vehicleId && 
                                       vi.IsActive && 
                                       vi.StartDate <= currentDate && 
                                       vi.EndDate >= currentDate)
                          .OrderByDescending(vi => vi.StartDate)
                          .FirstOrDefaultAsync();
    }

    public async Task<List<VehicleInsurance>> GetByInsuranceCompanyAsync(string company)
    {
        return await _dbSet.Where(vi => vi.InsuranceCompany == company && vi.IsActive).ToListAsync();
    }

    public async Task<List<VehicleInsurance>> GetExpiringInsuranceAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(vi => vi.EndDate <= beforeDate && vi.IsActive).ToListAsync();
    }

    public async Task<List<VehicleInsurance>> GetByPolicyNumberAsync(string policyNumber)
    {
        return await _dbSet.Where(vi => vi.PolicyNumber == policyNumber).ToListAsync();
    }
}