using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IWorkflowRepository : IRepository<TransactionWorkflow>
{
    Task<IEnumerable<TransactionWorkflow>> GetByTransactionIdAsync(string transactionId);
    Task<TransactionWorkflow?> GetCurrentStepAsync(string transactionId);
    Task<TransactionWorkflow?> GetByTransactionAndStepAsync(string transactionId, WorkflowStep step);
    Task<IEnumerable<TransactionWorkflow>> GetPendingStepsAsync();
    Task<bool> IsStepCompletedAsync(string transactionId, WorkflowStep step);
}