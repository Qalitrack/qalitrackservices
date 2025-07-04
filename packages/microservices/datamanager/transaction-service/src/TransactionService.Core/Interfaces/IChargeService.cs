using TransactionService.Core.DTOs;

namespace TransactionService.Core.Interfaces;

public interface IChargeService
{
    Task<IEnumerable<TransactionChargeDto>> GetChargesAsync(string transactionId);
    Task<TransactionChargeDto> CreateChargeAsync(CreateChargeRequest request);
    Task<TransactionChargeDto> UpdateChargeAsync(string chargeId, CreateChargeRequest request);
    Task<bool> DeleteChargeAsync(string chargeId);
    Task<bool> ApproveChargeAsync(string chargeId, string approvedBy);
    Task<bool> ProcessPaymentAsync(string chargeId, string paymentReference);
    Task<decimal> GetTotalAmountAsync(string transactionId);
    Task<decimal> GetUnpaidAmountAsync(string transactionId);
    Task<IEnumerable<TransactionChargeDto>> GetUnapprovedChargesAsync();
    Task CalculateChargesAsync(string transactionId);
}