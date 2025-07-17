using TransactionService.Core.DTOs;

namespace TransactionService.Core.Interfaces;

public interface IOrchestrationService
{
    Task<TransactionOrchestrationDto> GetOrchestrationStatusAsync(string transactionId);
    Task<bool> CoordinateServicesAsync(ServiceCoordinationRequest request);
    Task<bool> ValidateWithMasterDataAsync(string transactionId);
    Task<bool> ProcessWithWeightDataAsync(string transactionId);
    Task<bool> AdvanceWorkflowAsync(string transactionId, string workflowStep, string userId);
    Task<bool> RequiresApprovalAsync(string transactionId);
    Task<bool> ApproveTransactionAsync(string transactionId, string approvedBy, string? reason = null);
    Task<bool> RejectTransactionAsync(string transactionId, string rejectedBy, string reason);
}