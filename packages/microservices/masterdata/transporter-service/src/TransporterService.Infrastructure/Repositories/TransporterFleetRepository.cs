using Microsoft.EntityFrameworkCore;
using TransporterService.Core.Entities;
using TransporterService.Core.Interfaces;
using TransporterService.Infrastructure.Data;

namespace TransporterService.Infrastructure.Repositories;

public class TransporterFleetRepository : Repository<TransporterFleet>, ITransporterFleetRepository
{
    public TransporterFleetRepository(TransporterDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransporterFleet>> GetFleetByTransporterIdAsync(string transporterId)
    {
        return await _dbSet
            .Where(f => f.TransporterId == transporterId && !f.IsDeleted)
            .OrderBy(f => f.VehicleNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterFleet>> GetAvailableVehiclesAsync(string transporterId)
    {
        return await _dbSet
            .Where(f => f.TransporterId == transporterId && f.Status == VehicleStatus.Available && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterFleet>> GetVehiclesByStatusAsync(string transporterId, VehicleStatus status)
    {
        return await _dbSet
            .Where(f => f.TransporterId == transporterId && f.Status == status && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransporterFleet>> GetVehiclesByTypeAsync(string transporterId, VehicleType type)
    {
        return await _dbSet
            .Where(f => f.TransporterId == transporterId && f.VehicleType == type && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<TransporterFleet?> GetByRegistrationNumberAsync(string registrationNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(f => f.RegistrationNumber == registrationNumber && !f.IsDeleted);
    }

    public async Task<IEnumerable<TransporterFleet>> GetVehiclesDueForMaintenanceAsync(string transporterId)
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(f => f.TransporterId == transporterId && 
                       f.NextMaintenanceDate.HasValue && 
                       f.NextMaintenanceDate.Value <= today.AddDays(7) && 
                       !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, string? excludeId = null)
    {
        var query = _dbSet.Where(f => f.RegistrationNumber == registrationNumber && !f.IsDeleted);
        
        if (excludeId != null)
        {
            query = query.Where(f => f.Id != excludeId);
        }
        
        return !await query.AnyAsync();
    }
}