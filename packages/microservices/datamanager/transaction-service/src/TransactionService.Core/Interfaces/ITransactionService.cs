using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request);
    Task<TransactionDto?> GetTransactionAsync(string id);
    Task<TransactionDto?> GetCompleteTransactionAsync(string id);
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<TransactionDto>> GetTransactionsByVehicleAsync(string vehicleId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByOrganizationAsync(string organizationId);
    Task<IEnumerable<TransactionDto>> GetTransactionsByStatusAsync(TransactionStatus status);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<TransactionDto> UpdateTransactionAsync(string id, UpdateTransactionRequest request);
    Task<bool> DeleteTransactionAsync(string id);
    Task<bool> CancelTransactionAsync(string id, string reason);
}