using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IWorkflowService
{
    Task<IEnumerable<TransactionWorkflowDto>> GetWorkflowStepsAsync(string transactionId);
    Task<TransactionWorkflowDto?> GetCurrentStepAsync(string transactionId);
    Task<bool> AdvanceWorkflowAsync(string transactionId, string? processedBy = null, string? notes = null);
    Task<bool> UpdateStepAsync(string transactionId, WorkflowStep step, StepStatus status, string? processedBy = null, string? notes = null);
    Task<bool> ResetWorkflowAsync(string transactionId, string? reason = null);
    Task<bool> SkipStepAsync(string transactionId, WorkflowStep step, string reason);
    Task<IEnumerable<TransactionWorkflowDto>> GetPendingStepsAsync();
    Task InitializeWorkflowAsync(string transactionId);
}