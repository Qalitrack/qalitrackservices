using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleTypeRepository : Repository<VehicleType>, IVehicleTypeRepository
{
    public VehicleTypeRepository(VehicleDbContext context) : base(context) { }

    public async Task<VehicleType?> GetByNameAsync(string name)
    {
        return await _dbSet.FirstOrDefaultAsync(vt => vt.Name == name);
    }

    public async Task<List<VehicleType>> GetByCategoryAsync(string category)
    {
        return await _dbSet.Where(vt => vt.Category == category).ToListAsync();
    }

    public async Task<List<VehicleType>> GetActiveTypesAsync()
    {
        return await _dbSet.Where(vt => vt.IsActive).ToListAsync();
    }

    public async Task<bool> IsNameUniqueAsync(string name, string? excludeId = null)
    {
        var query = _dbSet.Where(vt => vt.Name == name);
        if (!string.IsNullOrEmpty(excludeId))
        {
            query = query.Where(vt => vt.Id != excludeId);
        }
        return !await query.AnyAsync();
    }
}