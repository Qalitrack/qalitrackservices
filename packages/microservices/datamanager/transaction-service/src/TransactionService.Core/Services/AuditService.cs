using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;
    private readonly IMapper _mapper;

    public AuditService(IAuditRepository auditRepository, IMapper mapper)
    {
        _auditRepository = auditRepository;
        _mapper = mapper;
    }

    public async Task<TransactionAuditDto> CreateAuditLogAsync(CreateAuditRequest request, string userId, string? ipAddress = null, string? userAgent = null)
    {
        var auditRecord = new TransactionAudit
        {
            TransactionId = request.TransactionId,
            Action = request.Action,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Reason = request.Reason,
            AuditAction = Enum.Parse<AuditAction>(request.AuditAction)
        };

        await _auditRepository.AddAsync(auditRecord);
        return _mapper.Map<TransactionAuditDto>(auditRecord);
    }

    public async Task<List<TransactionAuditDto>> GetAuditTrailAsync(string transactionId)
    {
        var auditTrail = await _auditRepository.GetTransactionAuditTrailAsync(transactionId);
        return _mapper.Map<List<TransactionAuditDto>>(auditTrail);
    }

    public async Task<List<TransactionAuditDto>> GetUserAuditHistoryAsync(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var auditHistory = await _auditRepository.GetAuditsByUserAsync(userId, fromDate, toDate);
        return _mapper.Map<List<TransactionAuditDto>>(auditHistory);
    }

    public async Task<List<TransactionAuditDto>> GetEntityAuditHistoryAsync(string entityType, string entityId)
    {
        var auditHistory = await _auditRepository.GetAuditsByEntityAsync(entityType, entityId);
        return _mapper.Map<List<TransactionAuditDto>>(auditHistory);
    }

    public async Task LogTransactionCreatedAsync(string transactionId, Dictionary<string, object> transactionData, string userId)
    {
        var request = new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = "Transaction Created",
            EntityType = "WeighingTransaction",
            EntityId = transactionId,
            NewValues = transactionData,
            AuditAction = AuditAction.Create.ToString()
        };

        await CreateAuditLogAsync(request, userId);
    }

    public async Task LogTransactionUpdatedAsync(string transactionId, Dictionary<string, object> oldData, Dictionary<string, object> newData, string userId)
    {
        var request = new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = "Transaction Updated",
            EntityType = "WeighingTransaction",
            EntityId = transactionId,
            OldValues = oldData,
            NewValues = newData,
            AuditAction = AuditAction.Update.ToString()
        };

        await CreateAuditLogAsync(request, userId);
    }

    public async Task LogStateChangeAsync(string transactionId, string fromState, string toState, string trigger, string userId)
    {
        var request = new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = $"State Changed: {fromState} -> {toState}",
            EntityType = "TransactionState",
            EntityId = transactionId,
            OldValues = new Dictionary<string, object> { ["State"] = fromState },
            NewValues = new Dictionary<string, object> { ["State"] = toState, ["Trigger"] = trigger },
            AuditAction = AuditAction.StateChange.ToString()
        };

        await CreateAuditLogAsync(request, userId);
    }

    public async Task LogWorkflowAdvanceAsync(string transactionId, string workflowStep, string userId)
    {
        var request = new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = $"Workflow Advanced: {workflowStep}",
            EntityType = "TransactionWorkflow",
            EntityId = transactionId,
            NewValues = new Dictionary<string, object> { ["WorkflowStep"] = workflowStep },
            AuditAction = AuditAction.WorkflowAdvance.ToString()
        };

        await CreateAuditLogAsync(request, userId);
    }
}