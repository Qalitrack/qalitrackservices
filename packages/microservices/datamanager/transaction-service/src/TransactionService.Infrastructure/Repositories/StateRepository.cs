using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class StateRepository : Repository<TransactionState>, IStateRepository
{
    public StateRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<List<TransactionState>> GetTransactionStateHistoryAsync(string transactionId)
    {
        return await _dbSet
            .Where(s => s.TransactionId == transactionId && !s.IsDeleted)
            .OrderBy(s => s.TransitionDate)
            .ToListAsync();
    }

    public async Task<TransactionState?> GetLastStateTransitionAsync(string transactionId)
    {
        return await _dbSet
            .Where(s => s.TransactionId == transactionId && !s.IsDeleted)
            .OrderByDescending(s => s.TransitionDate)
            .FirstOrDefaultAsync();
    }

    public async Task<List<TransactionState>> GetStateTransitionsByTriggerAsync(string trigger)
    {
        return await _dbSet
            .Where(s => s.Trigger == trigger && !s.IsDeleted)
            .OrderByDescending(s => s.TransitionDate)
            .ToListAsync();
    }

    public async Task<bool> IsValidStateTransitionAsync(string fromState, string toState, string trigger)
    {
        // This could be enhanced to check against a state transition rules table
        // For now, we'll use the business logic from StateService
        return await Task.FromResult(true);
    }

    public async Task<List<TransactionState>> GetTransitionStatisticsAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _dbSet.Where(s => !s.IsDeleted);
        
        if (fromDate.HasValue)
        {
            query = query.Where(s => s.TransitionDate >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            query = query.Where(s => s.TransitionDate <= toDate.Value);
        }
        
        return await query.OrderBy(s => s.TransitionDate).ToListAsync();
    }
}