using Microsoft.EntityFrameworkCore;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;
using TransactionService.Infrastructure.Data;

namespace TransactionService.Infrastructure.Repositories;

public class WorkflowRepository : Repository<TransactionWorkflow>, IWorkflowRepository
{
    public WorkflowRepository(TransactionDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TransactionWorkflow>> GetByTransactionIdAsync(string transactionId)
    {
        return await _dbSet
            .Where(w => w.TransactionId == transactionId && !w.IsDeleted)
            .OrderBy(w => w.Order)
            .ToListAsync();
    }

    public async Task<TransactionWorkflow?> GetCurrentStepAsync(string transactionId)
    {
        return await _dbSet
            .Where(w => w.TransactionId == transactionId && !w.IsDeleted)
            .Where(w => w.Status == StepStatus.InProgress || w.Status == StepStatus.NotStarted)
            .OrderBy(w => w.Order)
            .FirstOrDefaultAsync();
    }

    public async Task<TransactionWorkflow?> GetByTransactionAndStepAsync(string transactionId, WorkflowStep step)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.TransactionId == transactionId && w.WorkflowStep == step && !w.IsDeleted);
    }

    public async Task<IEnumerable<TransactionWorkflow>> GetPendingStepsAsync()
    {
        return await _dbSet
            .Include(w => w.Transaction)
            .Where(w => w.Status == StepStatus.InProgress || w.Status == StepStatus.RequiresAttention)
            .Where(w => !w.IsDeleted)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> IsStepCompletedAsync(string transactionId, WorkflowStep step)
    {
        return await _dbSet
            .AnyAsync(w => w.TransactionId == transactionId && 
                          w.WorkflowStep == step && 
                          w.Status == StepStatus.Completed && 
                          !w.IsDeleted);
    }
}