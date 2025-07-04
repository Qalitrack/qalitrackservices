using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeRepository : Repository<Weighbridge>, IWeighbridgeRepository
{
    public WeighbridgeRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Weighbridge>> GetActiveWeighbridgesAsync()
    {
        return await _dbSet
            .Where(w => w.Status == WeighbridgeStatus.Active && !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Weighbridge>> GetWeighbridgesByStatusAsync(WeighbridgeStatus status)
    {
        return await _dbSet
            .Where(w => w.Status == status && !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .ToListAsync();
    }

    public async Task<Weighbridge?> GetByCodeAsync(string code)
    {
        return await _dbSet
            .Where(w => w.Code == code && !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .Include(w => w.CalibrationHistory)
            .Include(w => w.MaintenanceSchedules)
            .Include(w => w.Operators)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Weighbridge>> GetWeighbridgesByLocationAsync(string location)
    {
        return await _dbSet
            .Where(w => w.Location.Contains(location) && !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .ToListAsync();
    }

    public async Task<IEnumerable<Weighbridge>> GetWeighbridgesNeedingCalibrationAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(w => w.NextCalibrationDate <= today && 
                       w.Status == WeighbridgeStatus.Active && 
                       !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .ToListAsync();
    }

    public async Task<IEnumerable<Weighbridge>> GetWeighbridgesNeedingMaintenanceAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(w => w.MaintenanceSchedules.Any(m => 
                           m.ScheduledDate <= today && 
                           m.Status == MaintenanceStatus.Scheduled) && 
                       !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.MaintenanceSchedules)
            .ToListAsync();
    }

    public async Task<IEnumerable<Weighbridge>> GetAvailableWeighbridgesAsync(DateTime requestedTime)
    {
        return await _dbSet
            .Where(w => w.Status == WeighbridgeStatus.Active && 
                       !w.IsDeleted &&
                       !w.MaintenanceSchedules.Any(m => 
                           m.ScheduledDate <= requestedTime && 
                           m.ScheduledDate.AddHours((double)m.EstimatedDuration) > requestedTime &&
                           m.Status != MaintenanceStatus.Completed &&
                           m.Status != MaintenanceStatus.Cancelled))
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .ToListAsync();
    }

    public override async Task<Weighbridge?> GetByIdAsync(string id)
    {
        return await _dbSet
            .Where(w => w.Id == id && !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .Include(w => w.CalibrationHistory)
            .Include(w => w.MaintenanceSchedules)
            .Include(w => w.Operators)
            .FirstOrDefaultAsync();
    }

    public override async Task<IEnumerable<Weighbridge>> GetAllAsync()
    {
        return await _dbSet
            .Where(w => !w.IsDeleted)
            .Include(w => w.WeighbridgeLocation)
            .Include(w => w.Configuration)
            .Include(w => w.CurrentCapacity)
            .ToListAsync();
    }
}