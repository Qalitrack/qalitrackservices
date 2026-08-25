using Transaction.Core.DTOs;
using Transaction.Core.Entities;

namespace Transaction.Core.Interfaces;

public interface ITransactionRepository : IRepository<WeighbridgeTransaction>
{
    Task<bool> IsReceiptNoAvailableAsync(string receiptNo);
    Task<WeighbridgeTransaction?> GetByReceiptNoAsync(string receiptNo);
    Task<WeighbridgeTransaction?> GetByIdAsync(string ticketId);
    Task<string?> GetLatestReceiptNumberAsync(string datePrefix);
    
    // Paginated queries with filters
    Task<PagedResult<WeighbridgeTransaction>> GetPagedAsync(WeighbridgeTransactionFilter filter);

    // Aggregated dashboard stats — computed server-side instead of shipping raw rows to the client
    Task<TransactionStatsDto> GetStatsAsync();
    
    // Get incomplete transactions for a vehicle (to allow continuation)
    Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleAsync(string noPlate);
    Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId);
    
    // Get transactions by status
    Task<List<WeighbridgeTransaction>> GetTransactionsByStatusAsync(string status, int limit = 100);
    
    // Get reweigh records for a transaction
    Task<List<ReweighRecord>> GetReweighRecordsAsync(string ticketId);

    // Create reweigh record
    Task<ReweighRecord> CreateReweighRecordAsync(ReweighRecord record);

    // Delete by ticket ID
    Task<bool> DeleteAsync(string ticketId, string? changedBy = null);

    // Get generic before/after audit trail for a transaction
    Task<List<TransactionAuditLog>> GetAuditLogsAsync(string ticketId);

    // Create a generic audit-log row directly (used for the Create action,
    // which has no "before" state to diff)
    Task<TransactionAuditLog> CreateAuditLogAsync(TransactionAuditLog log);

    // Creates a transaction, regenerating and retrying on a ReceiptNo unique
    // constraint conflict — guards against two concurrent creations both
    // reading the same "latest receipt number" and computing the same next
    // value (see ReceiptNumberService).
    Task<WeighbridgeTransaction> CreateWithUniqueReceiptNoAsync(WeighbridgeTransaction entity, Func<Task<string>> generateReceiptNo);
}