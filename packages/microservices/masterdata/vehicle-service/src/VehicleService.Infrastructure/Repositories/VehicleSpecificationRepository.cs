using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleSpecificationRepository : Repository<VehicleSpecification>, IVehicleSpecificationRepository
{
    public VehicleSpecificationRepository(VehicleDbContext context) : base(context) { }

    public async Task<VehicleSpecification?> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet.FirstOrDefaultAsync(vs => vs.VehicleId == vehicleId);
    }
}