using Microsoft.EntityFrameworkCore;
using WeightDataService.Core.Entities;
using WeightDataService.Core.Interfaces;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Infrastructure.Repositories;

public class WeightMeasurementRepository : Repository<WeightMeasurement>, IWeightMeasurementRepository
{
    public WeightMeasurementRepository(WeightDataContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeightMeasurement>> GetByOrganizationAsync(string organizationId, int skip = 0, int take = 50)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && !w.IsDeleted)
            .Include(w => w.Corrections)
            .OrderByDescending(w => w.MeasurementDateTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightMeasurement>> GetByWeighbridgeAsync(string weighbridgeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbSet
            .Where(w => w.WeighbridgeId == weighbridgeId && !w.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(w => w.MeasurementDateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.MeasurementDateTime <= toDate.Value);

        return await query
            .Include(w => w.Corrections)
            .OrderByDescending(w => w.MeasurementDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightMeasurement>> GetByVehicleRegistrationAsync(string vehicleRegistration, string organizationId)
    {
        return await _dbSet
            .Where(w => w.VehicleRegistration == vehicleRegistration && w.OrganizationId == organizationId && !w.IsDeleted)
            .Include(w => w.Corrections)
            .OrderByDescending(w => w.MeasurementDateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeightMeasurement>> GetPendingMeasurementsAsync(string organizationId)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && w.Status == MeasurementStatus.Pending && !w.IsDeleted)
            .Include(w => w.Corrections)
            .OrderBy(w => w.MeasurementDateTime)
            .ToListAsync();
    }

    public async Task<WeightMeasurement?> GetByTicketReferenceAsync(string ticketReference, string organizationId)
    {
        return await _dbSet
            .Where(w => w.TicketReference == ticketReference && w.OrganizationId == organizationId && !w.IsDeleted)
            .Include(w => w.Corrections)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeightMeasurement>> SearchAsync(string organizationId, string? searchTerm = null, 
        MeasurementStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null, 
        int skip = 0, int take = 50)
    {
        var query = _dbSet
            .Where(w => w.OrganizationId == organizationId && !w.IsDeleted);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(w => 
                w.VehicleRegistration.Contains(searchTerm) ||
                (w.TicketReference != null && w.TicketReference.Contains(searchTerm)) ||
                (w.CustomerReference != null && w.CustomerReference.Contains(searchTerm)));
        }

        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(w => w.MeasurementDateTime >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(w => w.MeasurementDateTime <= toDate.Value);

        return await query
            .Include(w => w.Corrections)
            .OrderByDescending(w => w.MeasurementDateTime)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetCountByOrganizationAsync(string organizationId)
    {
        return await _dbSet
            .Where(w => w.OrganizationId == organizationId && !w.IsDeleted)
            .CountAsync();
    }
}