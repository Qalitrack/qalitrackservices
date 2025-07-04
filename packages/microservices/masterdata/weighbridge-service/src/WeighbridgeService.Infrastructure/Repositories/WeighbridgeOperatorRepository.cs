using Microsoft.EntityFrameworkCore;
using WeighbridgeService.Core.Entities;
using WeighbridgeService.Core.Interfaces;
using WeighbridgeService.Infrastructure.Data;

namespace WeighbridgeService.Infrastructure.Repositories;

public class WeighbridgeOperatorRepository : Repository<WeighbridgeOperator>, IWeighbridgeOperatorRepository
{
    public WeighbridgeOperatorRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeighbridgeOperator>> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(o => o.WeighbridgeId == weighbridgeId && !o.IsDeleted)
            .OrderBy(o => o.FirstName)
            .ThenBy(o => o.LastName)
            .ToListAsync();
    }

    public async Task<WeighbridgeOperator?> GetByOperatorIdAsync(string operatorId)
    {
        return await _dbSet
            .Where(o => o.OperatorId == operatorId && !o.IsDeleted)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<WeighbridgeOperator>> GetByStatusAsync(OperatorStatus status)
    {
        return await _dbSet
            .Where(o => o.Status == status && !o.IsDeleted)
            .OrderBy(o => o.FirstName)
            .ThenBy(o => o.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeOperator>> GetActiveOperatorsAsync()
    {
        return await _dbSet
            .Where(o => o.Status == OperatorStatus.Active && !o.IsDeleted)
            .OrderBy(o => o.FirstName)
            .ThenBy(o => o.LastName)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeOperator>> GetOperatorsNeedingTrainingAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(o => o.Status == OperatorStatus.Active && 
                       (o.NextTrainingDate == null || o.NextTrainingDate <= today) && 
                       !o.IsDeleted)
            .OrderBy(o => o.NextTrainingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeOperator>> GetOperatorsWithExpiredCertificationAsync()
    {
        var today = DateTime.Today;
        return await _dbSet
            .Where(o => o.Status == OperatorStatus.Active && 
                       o.CertificationExpiry != null && 
                       o.CertificationExpiry <= today && 
                       !o.IsDeleted)
            .OrderBy(o => o.CertificationExpiry)
            .ToListAsync();
    }
}

public class WeighbridgeScheduleRepository : Repository<WeighbridgeSchedule>, IWeighbridgeScheduleRepository
{
    public WeighbridgeScheduleRepository(WeighbridgeDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeighbridgeSchedule>> GetByWeighbridgeIdAsync(string weighbridgeId)
    {
        return await _dbSet
            .Where(s => s.WeighbridgeId == weighbridgeId && !s.IsDeleted)
            .Include(s => s.Operator)
            .OrderBy(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeSchedule>> GetByOperatorIdAsync(string operatorId)
    {
        return await _dbSet
            .Where(s => s.OperatorId == operatorId && !s.IsDeleted)
            .Include(s => s.Operator)
            .OrderBy(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeSchedule>> GetByDateAsync(DateTime date)
    {
        return await _dbSet
            .Where(s => s.ScheduleDate.Date == date.Date && !s.IsDeleted)
            .Include(s => s.Operator)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeighbridgeSchedule>> GetActiveSchedulesAsync()
    {
        return await _dbSet
            .Where(s => s.IsActive && !s.IsDeleted)
            .Include(s => s.Operator)
            .OrderBy(s => s.ScheduleDate)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }
}