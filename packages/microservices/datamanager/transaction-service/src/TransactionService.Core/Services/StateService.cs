using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class StateService : IStateService
{
    private readonly IStateRepository _stateRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAuditService _auditService;
    private readonly IMapper _mapper;

    // Define state machine transitions
    private static readonly Dictionary<string, Dictionary<string, string>> StateTransitions = new()
    {
        [TransactionStates.Pending] = new()
        {
            [TransactionTriggers.Start] = TransactionStates.InProgress,
            [TransactionTriggers.Cancel] = TransactionStates.Cancelled
        },
        [TransactionStates.InProgress] = new()
        {
            [TransactionTriggers.BeginWeighing] = TransactionStates.Weighing,
            [TransactionTriggers.Cancel] = TransactionStates.Cancelled,
            [TransactionTriggers.Fail] = TransactionStates.Failed
        },
        [TransactionStates.Weighing] = new()
        {
            [TransactionTriggers.CompleteWeighing] = TransactionStates.Documentation,
            [TransactionTriggers.Cancel] = TransactionStates.Cancelled,
            [TransactionTriggers.Fail] = TransactionStates.Failed
        },
        [TransactionStates.Documentation] = new()
        {
            [TransactionTriggers.SubmitDocuments] = TransactionStates.Approval,
            [TransactionTriggers.Cancel] = TransactionStates.Cancelled,
            [TransactionTriggers.Fail] = TransactionStates.Failed
        },
        [TransactionStates.Approval] = new()
        {
            [TransactionTriggers.Approve] = TransactionStates.Charging,
            [TransactionTriggers.Reject] = TransactionStates.InProgress,
            [TransactionTriggers.Cancel] = TransactionStates.Cancelled
        },
        [TransactionStates.Charging] = new()
        {
            [TransactionTriggers.ApplyCharges] = TransactionStates.Completed,
            [TransactionTriggers.Complete] = TransactionStates.Completed,
            [TransactionTriggers.Fail] = TransactionStates.Failed
        }
    };

    public StateService(IStateRepository stateRepository, ITransactionRepository transactionRepository, 
                       IAuditService auditService, IMapper mapper)
    {
        _stateRepository = stateRepository;
        _transactionRepository = transactionRepository;
        _auditService = auditService;
        _mapper = mapper;
    }

    public async Task<StateValidationResult> ValidateStateTransitionAsync(string transactionId, string trigger)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            return new StateValidationResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { "Transaction not found" }
            };
        }

        var currentState = transaction.CurrentState;
        
        if (!StateTransitions.ContainsKey(currentState))
        {
            return new StateValidationResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { $"Invalid current state: {currentState}" }
            };
        }

        if (!StateTransitions[currentState].ContainsKey(trigger))
        {
            return new StateValidationResult
            {
                IsValid = false,
                ValidationErrors = new List<string> { $"Invalid trigger '{trigger}' for state '{currentState}'" },
                AvailableTriggers = StateTransitions[currentState].Keys.ToList()
            };
        }

        var nextState = StateTransitions[currentState][trigger];
        
        // Additional business rule validations
        var validationErrors = await ValidateBusinessRulesAsync(transaction, trigger, nextState);

        return new StateValidationResult
        {
            IsValid = !validationErrors.Any(),
            ValidationErrors = validationErrors,
            NextState = nextState,
            AvailableTriggers = StateTransitions[currentState].Keys.ToList()
        };
    }

    public async Task<TransactionStateDto> TransitionStateAsync(StateTransitionRequest request, string userId)
    {
        var validationResult = await ValidateStateTransitionAsync(request.TransactionId, request.Trigger);
        
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException($"Invalid state transition: {string.Join(", ", validationResult.ValidationErrors)}");
        }

        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        var fromState = transaction!.CurrentState;
        var toState = validationResult.NextState!;

        // Create state transition record
        var stateTransition = new TransactionState
        {
            TransactionId = request.TransactionId,
            FromState = fromState,
            ToState = toState,
            Trigger = request.Trigger,
            UserId = userId,
            Reason = request.Reason,
            TransitionData = request.TransitionData,
            IsValid = true
        };

        await _stateRepository.AddAsync(stateTransition);

        // Update transaction current state
        transaction.CurrentState = toState;
        transaction.StateLastChanged = DateTime.UtcNow;
        transaction.StateChangedBy = userId;

        await _transactionRepository.UpdateAsync(transaction);

        // Log audit trail
        await _auditService.LogStateChangeAsync(request.TransactionId, fromState, toState, request.Trigger, userId);

        return _mapper.Map<TransactionStateDto>(stateTransition);
    }

    public async Task<List<TransactionStateDto>> GetStateHistoryAsync(string transactionId)
    {
        var stateHistory = await _stateRepository.GetTransactionStateHistoryAsync(transactionId);
        return _mapper.Map<List<TransactionStateDto>>(stateHistory);
    }

    public async Task<List<string>> GetAvailableTriggersAsync(string transactionId)
    {
        var currentState = await GetCurrentStateAsync(transactionId);
        
        if (StateTransitions.ContainsKey(currentState))
        {
            return StateTransitions[currentState].Keys.ToList();
        }

        return new List<string>();
    }

    public async Task<string> GetCurrentStateAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        return transaction?.CurrentState ?? TransactionStates.Pending;
    }

    public async Task<bool> CanTransitionToStateAsync(string transactionId, string targetState)
    {
        var currentState = await GetCurrentStateAsync(transactionId);
        
        if (!StateTransitions.ContainsKey(currentState))
            return false;

        return StateTransitions[currentState].Values.Contains(targetState);
    }

    private async Task<List<string>> ValidateBusinessRulesAsync(WeighingTransaction transaction, string trigger, string nextState)
    {
        var errors = new List<string>();

        // Business rule: Cannot complete without weights
        if (trigger == TransactionTriggers.CompleteWeighing)
        {
            if (!transaction.GrossWeight.HasValue && !transaction.TareWeight.HasValue)
            {
                errors.Add("Cannot complete weighing without weight measurements");
            }
        }

        // Business rule: Cannot approve without required documents
        if (trigger == TransactionTriggers.Approve)
        {
            if (!transaction.Documents.Any())
            {
                errors.Add("Cannot approve transaction without required documents");
            }
        }

        // Business rule: Cannot charge without approval if required
        if (trigger == TransactionTriggers.ApplyCharges && transaction.RequiresApproval)
        {
            if (string.IsNullOrEmpty(transaction.ApprovedBy))
            {
                errors.Add("Cannot apply charges without approval");
            }
        }

        return await Task.FromResult(errors);
    }
}