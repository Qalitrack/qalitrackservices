using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IAuditService
{
    Task<TransactionAuditDto> CreateAuditLogAsync(CreateAuditRequest request, string userId, string? ipAddress = null, string? userAgent = null);
    Task<List<TransactionAuditDto>> GetAuditTrailAsync(string transactionId);
    Task<List<TransactionAuditDto>> GetUserAuditHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<TransactionAuditDto>> GetEntityAuditHistoryAsync(string entityType, string entityId);
    Task LogTransactionCreatedAsync(string transactionId, Dictionary<string, object> transactionData, string userId);
    Task LogTransactionUpdatedAsync(string transactionId, Dictionary<string, object> oldData, Dictionary<string, object> newData, string userId);
    Task LogStateChangeAsync(string transactionId, string fromState, string toState, string trigger, string userId);
    Task LogWorkflowAdvanceAsync(string transactionId, string workflowStep, string userId);
}