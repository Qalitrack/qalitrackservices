using Transaction.Core.DTOs;

namespace Transaction.Core.Interfaces;

public interface ITransactionService
{
    // Basic CRUD with pagination
    Task<PagedResult<TransactionReadDto>> GetAllAsync(WeighbridgeTransactionFilter filter);
    Task<TransactionReadDto?> GetByIdAsync(string ticketId);
    Task<TransactionReadDto?> GetByReceiptNoAsync(string receiptNo);
    Task<TransactionReadDto> CreateAsync(CreateTransactionDto dto);
    Task<TransactionReadDto?> UpdateAsync(string ticketId, UpdateTransactionDto dto);
    Task<bool> DeleteAsync(string ticketId);
    
    // Receipt validation
    Task<bool> IsReceiptNoAvailableAsync(string receiptNo);
    
    // Weighing operations
    Task<TransactionReadDto?> AddSecondWeightAsync(AddSecondWeightDto dto);
    Task<TransactionReadDto?> CompleteTransactionAsync(CompleteTransactionDto dto);
    
    // Get incomplete transactions for continuation
    Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleAsync(string noPlate);
    Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId);
    
    // Get transactions by status
    Task<IEnumerable<TransactionReadDto>> GetTransactionsByStatusAsync(string status, int limit = 100);
    
    // Reweigh operations
    Task<bool> RequestReweighAsync(RequestReweighDto dto);
    Task<IEnumerable<ReweighRecordDto>> GetReweighRecordsAsync(string ticketId);
}