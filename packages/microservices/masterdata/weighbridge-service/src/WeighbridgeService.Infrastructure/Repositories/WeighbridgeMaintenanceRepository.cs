using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeMaintenanceRepository : Repository<WeighbridgeMaintenance>, IWeighbridgeMaintenanceRepository
{
    public WeighbridgeMaintenanceRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(m => m.WeighbridgeId == weighbridgeId && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetByStatusAsync(MaintenanceStatus status)
    {
        return await _dbSet
            .Where(m => m.Status == status && !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetByTypeAsync(MaintenanceType type)
    {
        return await _dbSet
            .Where(m => m.Type == type && !m.IsDeleted)
            .OrderByDescending(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetByPriorityAsync(MaintenancePriority priority)
    {
        return await _dbSet
            .Where(m => m.Priority == priority && !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetScheduledMaintenanceAsync()
    {
        return await _dbSet
            .Where(m => m.Status == MaintenanceStatus.Scheduled && !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetOverdueMaintenanceAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(m => m.Status == MaintenanceStatus.Scheduled && 
                       m.ScheduledDate < today && 
                       !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeMaintenance>> GetUpcomingMaintenanceAsync(int days)
    {
        var endDate = DateTime.Today.AddDays(days);
        return await _dbSet
            .Where(m => m.Status == MaintenanceStatus.Scheduled && 
                       m.ScheduledDate >= DateTime.Today && 
                       m.ScheduledDate <= endDate && 
                       !m.IsDeleted)
            .OrderBy(m => m.ScheduledDate)
            .ToListAsync();
    }
}