using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeCalibrationRepository : Repository<WeighbridgeCalibration>, IWeighbridgeCalibrationRepository
{
    public WeighbridgeCalibrationRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeighbridgeCalibration>> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(c => c.WeighbridgeId == weighbridgeId && !c.IsDeleted)
            .OrderByDescending(c => c.ScheduledDate)
            .ToListAsync();
    }

    public async Task<WeighbridgeCalibration?> GetLatestCalibrationAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(c => c.WeighbridgeId == weighbridgeId && 
                       c.Status == CalibrationStatus.Completed && 
                       !c.IsDeleted)
            .OrderByDescending(c => c.ActualDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeighbridgeCalibration>> GetByStatusAsync(CalibrationStatus status)
    {
        return await _dbSet
            .Where(c => c.Status == status && !c.IsDeleted)
            .OrderBy(c => c.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeCalibration>> GetByTypeAsync(CalibrationType type)
    {
        return await _dbSet
            .Where(c => c.Type == type && !c.IsDeleted)
            .OrderByDescending(c => c.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeCalibration>> GetScheduledCalibrationsAsync()
    {
        return await _dbSet
            .Where(c => c.Status == CalibrationStatus.Scheduled && !c.IsDeleted)
            .OrderBy(c => c.ScheduledDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeCalibration>> GetOverdueCalibrationsAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(c => c.Status == CalibrationStatus.Scheduled && 
                       c.ScheduledDate < today && 
                       !c.IsDeleted)
            .OrderBy(c => c.ScheduledDate)
            .ToListAsync();
    }
}