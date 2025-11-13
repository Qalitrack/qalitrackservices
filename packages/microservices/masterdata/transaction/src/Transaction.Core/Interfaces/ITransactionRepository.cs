using Transaction.Core.DTOs;
using Transaction.Core.Entities;

namespace Transaction.Core.Interfaces;

public interface ITransactionRepository : IRepository<WeighbridgeTransaction>
{
    Task<bool> IsReceiptNoAvailableAsync(string receiptNo);
    Task<WeighbridgeTransaction?> GetByReceiptNoAsync(string receiptNo);
    
    // Paginated queries with filters
    Task<PagedResult<WeighbridgeTransaction>> GetPagedAsync(WeighbridgeTransactionFilter filter);
    
    // Get incomplete transactions for a vehicle (to allow continuation)
    Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleAsync(string noPlate);
    Task<List<WeighbridgeTransaction>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId);
    
    // Get transactions by status
    Task<List<WeighbridgeTransaction>> GetTransactionsByStatusAsync(WeighbridgeTransactionStatus status, int limit = 100);
    
    // Get with related entities
    Task<WeighbridgeTransaction?> GetWithWeighingRecordsAsync(string id);
    Task<WeighbridgeTransaction?> GetWithAuditLogsAsync(string id);
    
    // Mark entity as modified for change tracking
    void MarkAsModified(WeighbridgeTransaction entity);
}