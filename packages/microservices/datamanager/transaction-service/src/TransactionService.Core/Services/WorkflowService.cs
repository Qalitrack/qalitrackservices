using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class WorkflowService : IWorkflowService
{
    private readonly IWorkflowRepository _workflowRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMapper _mapper;

    private static readonly Dictionary<WorkflowStep, WorkflowStep?> WorkflowTransitions = new()
    {
        { WorkflowStep.VehicleArrival, WorkflowStep.DocumentCheck },
        { WorkflowStep.DocumentCheck, WorkflowStep.EntryWeighing },
        { WorkflowStep.EntryWeighing, WorkflowStep.LoadingUnloading },
        { WorkflowStep.LoadingUnloading, WorkflowStep.ExitWeighing },
        { WorkflowStep.ExitWeighing, WorkflowStep.PaymentProcessing },
        { WorkflowStep.PaymentProcessing, WorkflowStep.Completion },
        { WorkflowStep.Completion, null },
        { WorkflowStep.DisputeResolution, WorkflowStep.Completion }
    };

    public WorkflowService(
        IWorkflowRepository workflowRepository,
        ITransactionRepository transactionRepository,
        IMapper mapper)
    {
        _workflowRepository = workflowRepository;
        _transactionRepository = transactionRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TransactionWorkflowDto>> GetWorkflowStepsAsync(string transactionId)
    {
        var steps = await _workflowRepository.GetByTransactionIdAsync(transactionId);
        return _mapper.Map<IEnumerable<TransactionWorkflowDto>>(steps);
    }

    public async Task<TransactionWorkflowDto?> GetCurrentStepAsync(string transactionId)
    {
        var step = await _workflowRepository.GetCurrentStepAsync(transactionId);
        return step == null ? null : _mapper.Map<TransactionWorkflowDto>(step);
    }

    public async Task<bool> AdvanceWorkflowAsync(string transactionId, string? processedBy = null, string? notes = null)
    {
        var currentStep = await _workflowRepository.GetCurrentStepAsync(transactionId);
        if (currentStep == null)
            return false;

        // Complete current step
        currentStep.Status = StepStatus.Completed;
        currentStep.CompletedAt = DateTime.UtcNow;
        currentStep.ProcessedBy = processedBy;
        if (!string.IsNullOrEmpty(notes))
            currentStep.Notes = notes;

        await _workflowRepository.UpdateAsync(currentStep);

        // Start next step if exists
        if (WorkflowTransitions.TryGetValue(currentStep.WorkflowStep, out var nextStep) && nextStep.HasValue)
        {
            var nextWorkflowStep = await _workflowRepository.GetByTransactionAndStepAsync(transactionId, nextStep.Value);
            if (nextWorkflowStep != null)
            {
                nextWorkflowStep.Status = StepStatus.InProgress;
                nextWorkflowStep.StartedAt = DateTime.UtcNow;
                await _workflowRepository.UpdateAsync(nextWorkflowStep);
            }
        }
        else
        {
            // Update transaction status to completed
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                transaction.Status = TransactionStatus.Completed;
                await _transactionRepository.UpdateAsync(transaction);
            }
        }

        return true;
    }

    public async Task<bool> UpdateStepAsync(string transactionId, WorkflowStep step, StepStatus status, string? processedBy = null, string? notes = null)
    {
        var workflowStep = await _workflowRepository.GetByTransactionAndStepAsync(transactionId, step);
        if (workflowStep == null)
            return false;

        workflowStep.Status = status;
        workflowStep.ProcessedBy = processedBy;
        if (!string.IsNullOrEmpty(notes))
            workflowStep.Notes = notes;

        if (status == StepStatus.InProgress && !workflowStep.StartedAt.HasValue)
            workflowStep.StartedAt = DateTime.UtcNow;
        else if (status == StepStatus.Completed)
            workflowStep.CompletedAt = DateTime.UtcNow;

        await _workflowRepository.UpdateAsync(workflowStep);
        return true;
    }

    public async Task<bool> ResetWorkflowAsync(string transactionId, string? reason = null)
    {
        var steps = await _workflowRepository.GetByTransactionIdAsync(transactionId);
        
        foreach (var step in steps)
        {
            step.Status = StepStatus.NotStarted;
            step.StartedAt = null;
            step.CompletedAt = null;
            step.Notes = reason;
            await _workflowRepository.UpdateAsync(step);
        }

        // Set first step to in progress
        var firstStep = steps.OrderBy(s => s.Order).FirstOrDefault();
        if (firstStep != null)
        {
            firstStep.Status = StepStatus.InProgress;
            firstStep.StartedAt = DateTime.UtcNow;
            await _workflowRepository.UpdateAsync(firstStep);
        }

        return true;
    }

    public async Task<bool> SkipStepAsync(string transactionId, WorkflowStep step, string reason)
    {
        var workflowStep = await _workflowRepository.GetByTransactionAndStepAsync(transactionId, step);
        if (workflowStep == null)
            return false;

        workflowStep.Status = StepStatus.Skipped;
        workflowStep.Notes = reason;
        workflowStep.CompletedAt = DateTime.UtcNow;

        await _workflowRepository.UpdateAsync(workflowStep);
        return true;
    }

    public async Task<IEnumerable<TransactionWorkflowDto>> GetPendingStepsAsync()
    {
        var steps = await _workflowRepository.GetPendingStepsAsync();
        return _mapper.Map<IEnumerable<TransactionWorkflowDto>>(steps);
    }

    public async Task InitializeWorkflowAsync(string transactionId)
    {
        var workflowSteps = new List<TransactionWorkflow>();
        var stepOrder = 1;

        foreach (WorkflowStep step in Enum.GetValues<WorkflowStep>())
        {
            if (step == WorkflowStep.DisputeResolution)
                continue; // Skip dispute resolution in normal workflow

            workflowSteps.Add(new TransactionWorkflow
            {
                TransactionId = transactionId,
                WorkflowStep = step,
                Status = step == WorkflowStep.VehicleArrival ? StepStatus.InProgress : StepStatus.NotStarted,
                Order = stepOrder++,
                StartedAt = step == WorkflowStep.VehicleArrival ? DateTime.UtcNow : null
            });
        }

        await _workflowRepository.AddRangeAsync(workflowSteps);
    }
}