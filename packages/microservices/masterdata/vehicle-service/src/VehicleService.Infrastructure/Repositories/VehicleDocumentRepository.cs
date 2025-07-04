using Microsoft.EntityFrameworkCore;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Infrastructure.Repositories;

public class VehicleDocumentRepository : Repository<VehicleDocument>, IVehicleDocumentRepository
{
    public VehicleDocumentRepository(VehicleDbContext context) : base(context) { }

    public async Task<List<VehicleDocument>> GetByVehicleIdAsync(string vehicleId)
    {
        return await _dbSet.Where(vd => vd.VehicleId == vehicleId && vd.IsActive).ToListAsync();
    }

    public async Task<List<VehicleDocument>> GetByDocumentTypeAsync(string vehicleId, DocumentType documentType)
    {
        return await _dbSet.Where(vd => vd.VehicleId == vehicleId && 
                                       vd.DocumentType == documentType && 
                                       vd.IsActive).ToListAsync();
    }

    public async Task<List<VehicleDocument>> GetExpiringDocumentsAsync(DateTime beforeDate)
    {
        return await _dbSet.Where(vd => vd.ExpiryDate.HasValue && 
                                       vd.ExpiryDate.Value <= beforeDate && 
                                       vd.IsActive).ToListAsync();
    }

    public async Task<List<VehicleDocument>> GetRequiredDocumentsAsync(string vehicleId)
    {
        return await _dbSet.Where(vd => vd.VehicleId == vehicleId && 
                                       vd.IsRequired && 
                                       vd.IsActive).ToListAsync();
    }
}