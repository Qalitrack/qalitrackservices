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
    Task<bool> DeleteAsync(string ticketId);
}