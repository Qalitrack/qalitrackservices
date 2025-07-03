using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IDocumentRepository : IRepository<TransactionDocument>
{
    Task<IEnumerable<TransactionDocument>> GetByTransactionIdAsync(string transactionId);
    Task<IEnumerable<TransactionDocument>> GetActiveByTransactionIdAsync(string transactionId);
    Task<TransactionDocument?> GetLatestVersionAsync(string transactionId, string documentType);
    Task<IEnumerable<TransactionDocument>> GetByDocumentTypeAsync(string documentType);
    Task<bool> DocumentExistsAsync(string transactionId, string fileName);
}