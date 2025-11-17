using Transaction.Core.DTOs;

namespace Transaction.Core.Interfaces;

public interface ITransactionService
{
    // Basic CRUD with pagination
    Task<PagedResult<TransactionReadDto>> GetAllAsync(WeighbridgeTransactionFilter filter);
    Task<TransactionReadDto?> GetByIdAsync(string id);
    Task<TransactionReadDto?> GetByReceiptNoAsync(string receiptNo);
    Task<TransactionReadDto> CreateAsync(CreateTransactionDto dto);
    Task<TransactionReadDto?> UpdateAsync(string id, UpdateTransactionDto dto);
    Task<bool> DeleteAsync(string id);
    
    // Receipt validation
    Task<bool> IsReceiptNoAvailableAsync(string receiptNo);
    
    // Weighing operations
    Task<TransactionReadDto?> AddWeighingAsync(AddWeighingDto dto);
    Task<TransactionReadDto?> CompleteTransactionAsync(CompleteTransactionDto dto);
    
    // Get incomplete transactions for continuation
    Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleAsync(string noPlate);
    Task<IEnumerable<TransactionReadDto>> GetIncompleteTransactionsByVehicleIdAsync(string vehicleId);
    
    // Get transactions by status
    Task<IEnumerable<TransactionReadDto>> GetTransactionsByStatusAsync(string status, int limit = 100);
    
    // Get with related data
    Task<TransactionReadDto?> GetWithWeighingRecordsAsync(string id);
    Task<TransactionReadDto?> GetWithAuditLogsAsync(string id);
    
    // Reweigh operations
    Task<bool> RequestReweighAsync(RequestReweighDto dto);
    Task<TransactionReadDto?> StartReweighAsync(string transactionId, string startedBy);
    Task<TransactionReadDto> AddReweighWeightAsync(AddReweighWeightDto dto);
    Task<TransactionReadDto> CompleteReweighAsync(CompleteReweighDto dto);
    Task<IEnumerable<ReweighRecordDto>> GetReweighRecordsAsync(string transactionId);
}