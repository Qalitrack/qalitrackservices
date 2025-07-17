using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;

namespace TransactionService.Core.Interfaces;

public interface IStateService
{
    Task<StateValidationResult> ValidateStateTransitionAsync(string transactionId, string trigger);
    Task<TransactionStateDto> TransitionStateAsync(StateTransitionRequest request, string userId);
    Task<List<TransactionStateDto>> GetStateHistoryAsync(string transactionId);
    Task<List<string>> GetAvailableTriggersAsync(string transactionId);
    Task<string> GetCurrentStateAsync(string transactionId);
    Task<bool> CanTransitionToStateAsync(string transactionId, string targetState);
}