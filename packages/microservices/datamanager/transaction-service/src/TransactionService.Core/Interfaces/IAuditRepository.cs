using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IAuditRepository : IRepository<TransactionAudit>
{
    Task<List<TransactionAudit>> GetTransactionAuditTrailAsync(string transactionId);
    Task<List<TransactionAudit>> GetAuditsByUserAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<TransactionAudit>> GetAuditsByActionAsync(AuditAction action, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<TransactionAudit>> GetAuditsByEntityAsync(string entityType, string entityId);
}