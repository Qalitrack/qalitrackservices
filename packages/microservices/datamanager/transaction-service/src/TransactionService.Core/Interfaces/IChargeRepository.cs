using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IChargeRepository : IRepository<TransactionCharge>
{
    Task<IEnumerable<TransactionCharge>> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<TransactionCharge>> GetUnpaidChargesAsync(string transactionId);
    Task<IEnumerable<TransactionCharge>> GetUnapprovedChargesAsync();
    Task<decimal> GetTotalAmountAsync(string transactionId);
    Task<decimal> GetUnpaidAmountAsync(string transactionId);
}