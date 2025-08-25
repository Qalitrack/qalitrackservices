using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IStateRepository : IRepository<TransactionState>
{
    Task<List<TransactionState>> GetTransactionStateHistoryAsync(string transactionId);
    Task<TransactionState?> GetLastStateTransitionAsync(string transactionId);
    Task<List<TransactionState>> GetStateTransitionsByTriggerAsync(string trigger);
    Task<List<TransactionState>> GetTransitionStatisticsAsync(DateTime? fromDate, DateTime? toDate);
    Task<bool> IsValidStateTransitionAsync(string fromState, string toState, string trigger);
}