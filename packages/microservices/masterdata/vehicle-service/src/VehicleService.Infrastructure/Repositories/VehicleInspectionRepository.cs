using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleInspectionRepository : Repository<VehicleInspection>, IVehicleInspectionRepository
{
    public VehicleInspectionRepository(VehicleDbContext context) : base(context) { }

    public async Task<List<VehicleInspection>> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet.Where(vi => vi.VehicleId == vehicleId && vi.IsValid)
                          .OrderByDescending(vi => vi.InspectionDate)
                          .ToListAsync();
    }

    public async Task<VehicleInspection?> GetLatestInspectionAsync(string vehicleId)
    {
        return await _dbSet.Where(vi => vi.VehicleId == vehicleId && vi.IsValid)
                          .OrderByDescending(vi => vi.InspectionDate)
                          .FirstOrDefaultAsync();
    }

    public async Task<List<VehicleInspection>> GetByInspectionTypeAsync(string vehicleId, InspectionType inspectionType)
    {
        return await _dbSet.Where(vi => vi.VehicleId == vehicleId && 
                                       vi.InspectionType == inspectionType && 
                                       vi.IsValid)
                          .OrderByDescending(vi => vi.InspectionDate)
                          .ToListAsync();
    }

    public async Task<List<VehicleInspection>> GetByResultAsync(InspectionResult result)
    {
        return await _dbSet.Where(vi => vi.Result == result && vi.IsValid).ToListAsync();
    }

    public async Task<List<VehicleInspection>> GetDueInspectionsAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(vi => vi.NextInspectionDue.HasValue && 
                                       vi.NextInspectionDue.Value <= beforeDate && 
                                       vi.IsValid).ToListAsync();
    }
}