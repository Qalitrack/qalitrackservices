using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class OrchestrationService : IOrchestrationService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWorkflowRepository _workflowRepository;
    private readonly IStateService _stateService;
    private readonly IAuditService _auditService;
    private readonly IMasterDataIntegrationService _masterDataService;
    private readonly IMapper _mapper;

    public OrchestrationService(
        ITransactionRepository transactionRepository,
        IWorkflowRepository workflowRepository,
        IStateService stateService,
        IAuditService auditService,
        IMasterDataIntegrationService masterDataService,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _workflowRepository = workflowRepository;
        _stateService = stateService;
        _auditService = auditService;
        _masterDataService = masterDataService;
        _mapper = mapper;
    }

    public async Task<TransactionOrchestrationDto> GetOrchestrationStatusAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
            throw new ArgumentException("Transaction not found", nameof(transactionId));

        var workflowSteps = await _workflowRepository.GetByTransactionIdAsync(transactionId);
        var stateHistory = await _stateService.GetStateHistoryAsync(transactionId);

        return new TransactionOrchestrationDto
        {
            TransactionId = transactionId,
            CurrentState = transaction.CurrentState,
            WorkflowSteps = _mapper.Map<List<WorkflowStepDto>>(workflowSteps),
            StateHistory = stateHistory,
            ServiceIntegrations = await GetServiceIntegrationsAsync(transactionId),
            RequiresApproval = transaction.RequiresApproval,
            ApprovedBy = transaction.ApprovedBy,
            ApprovedAt = transaction.ApprovedAt,
            Priority = transaction.Priority,
            ExternalReferenceId = transaction.ExternalReferenceId
        };
    }

    public async Task<bool> CoordinateServicesAsync(ServiceCoordinationRequest request)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.TransactionId);
        if (transaction == null) return false;

        var results = new List<bool>();

        foreach (var serviceName in request.ServiceNames)
        {
            try
            {
                bool serviceResult = serviceName.ToLower() switch
                {
                    "masterdata" => await ValidateWithMasterDataAsync(request.TransactionId),
                    "weightdata" => await ProcessWithWeightDataAsync(request.TransactionId),
                    _ => await CallExternalServiceAsync(serviceName, request)
                };

                results.Add(serviceResult);

                // Log service coordination
                await _auditService.CreateAuditLogAsync(new CreateAuditRequest
                {
                    TransactionId = request.TransactionId,
                    Action = $"Service Coordination: {serviceName}",
                    EntityType = "ServiceIntegration",
                    NewValues = new Dictionary<string, object>
                    {
                        ["ServiceName"] = serviceName,
                        ["Result"] = serviceResult,
                        ["Timestamp"] = DateTime.UtcNow
                    },
                    AuditAction = AuditAction.WorkflowAdvance.ToString()
                }, "SYSTEM");
            }
            catch (Exception ex)
            {
                results.Add(false);
                
                // Log service failure
                await _auditService.CreateAuditLogAsync(new CreateAuditRequest
                {
                    TransactionId = request.TransactionId,
                    Action = $"Service Coordination Failed: {serviceName}",
                    EntityType = "ServiceIntegration",
                    NewValues = new Dictionary<string, object>
                    {
                        ["ServiceName"] = serviceName,
                        ["Error"] = ex.Message,
                        ["Timestamp"] = DateTime.UtcNow
                    },
                    AuditAction = AuditAction.WorkflowAdvance.ToString()
                }, "SYSTEM");
            }
        }

        return request.RequireAllSuccess ? results.All(r => r) : results.Any(r => r);
    }

    public async Task<bool> ValidateWithMasterDataAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        try
        {
            // Validate Vehicle
            var vehicleValid = await _masterDataService.ValidateVehicleAsync(transaction.VehicleId);
            
            // Validate Driver
            var driverValid = await _masterDataService.ValidateDriverAsync(transaction.DriverId);
            
            // Validate Supplier
            var supplierValid = await _masterDataService.ValidateSupplierAsync(transaction.SupplierId);
            
            // Validate Customer (if provided)
            var customerValid = string.IsNullOrEmpty(transaction.CustomerId) || 
                               await _masterDataService.ValidateCustomerAsync(transaction.CustomerId);
            
            // Validate Product
            var productValid = await _masterDataService.ValidateProductAsync(transaction.ProductId);
            
            // Validate Route
            var routeValid = await _masterDataService.ValidateRouteAsync(transaction.RouteId);

            return vehicleValid && driverValid && supplierValid && customerValid && productValid && routeValid;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> ProcessWithWeightDataAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        try
        {
            // Integration with Weight Data Service would happen here
            // For now, simulate weight data processing
            
            // Check if weights are within acceptable ranges
            if (transaction.GrossWeight.HasValue && transaction.TareWeight.HasValue)
            {
                var netWeight = transaction.GrossWeight.Value - transaction.TareWeight.Value;
                transaction.NetWeight = netWeight;
                
                // Validate weight consistency
                if (netWeight <= 0)
                {
                    return false;
                }
                
                await _transactionRepository.UpdateAsync(transaction);
                return true;
            }

            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> AdvanceWorkflowAsync(string transactionId, string workflowStep, string userId)
    {
        var workflow = await _workflowRepository.GetByTransactionAndStepAsync(transactionId, workflowStep);
        if (workflow == null) return false;

        workflow.Status = StepStatus.Completed;
        workflow.CompletedAt = DateTime.UtcNow;
        workflow.ProcessedBy = userId;

        await _workflowRepository.UpdateAsync(workflow);
        await _auditService.LogWorkflowAdvanceAsync(transactionId, workflowStep, userId);

        return true;
    }

    public async Task<bool> RequiresApprovalAsync(string transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        // Business rules for approval requirements
        // High priority transactions require approval
        if (transaction.Priority >= 3) return true;
        
        // Large weights require approval
        if (transaction.NetWeight.HasValue && transaction.NetWeight.Value > 10000) return true;
        
        // External reference transactions require approval
        if (!string.IsNullOrEmpty(transaction.ExternalReferenceId)) return true;

        return transaction.RequiresApproval;
    }

    public async Task<bool> ApproveTransactionAsync(string transactionId, string approvedBy, string? reason = null)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        transaction.ApprovedBy = approvedBy;
        transaction.ApprovedAt = DateTime.UtcNow;
        transaction.RequiresApproval = false;

        await _transactionRepository.UpdateAsync(transaction);

        await _auditService.CreateAuditLogAsync(new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = "Transaction Approved",
            EntityType = "WeighingTransaction",
            EntityId = transactionId,
            NewValues = new Dictionary<string, object>
            {
                ["ApprovedBy"] = approvedBy,
                ["ApprovedAt"] = DateTime.UtcNow,
                ["Reason"] = reason ?? "No reason provided"
            },
            AuditAction = AuditAction.Approval.ToString()
        }, approvedBy);

        return true;
    }

    public async Task<bool> RejectTransactionAsync(string transactionId, string rejectedBy, string reason)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null) return false;

        // Reset to in-progress state for correction
        transaction.CurrentState = TransactionStates.InProgress;
        transaction.StateLastChanged = DateTime.UtcNow;
        transaction.StateChangedBy = rejectedBy;

        await _transactionRepository.UpdateAsync(transaction);

        await _auditService.CreateAuditLogAsync(new CreateAuditRequest
        {
            TransactionId = transactionId,
            Action = "Transaction Rejected",
            EntityType = "WeighingTransaction",
            EntityId = transactionId,
            NewValues = new Dictionary<string, object>
            {
                ["RejectedBy"] = rejectedBy,
                ["RejectedAt"] = DateTime.UtcNow,
                ["Reason"] = reason
            },
            AuditAction = AuditAction.Rejection.ToString()
        }, rejectedBy);

        return true;
    }

    private async Task<List<ServiceIntegrationDto>> GetServiceIntegrationsAsync(string transactionId)
    {
        // This would typically query a service integration log table
        // For now, return mock data
        return await Task.FromResult(new List<ServiceIntegrationDto>
        {
            new() { ServiceName = "MasterData", Status = "Success", LastCalled = DateTime.UtcNow.AddMinutes(-5) },
            new() { ServiceName = "WeightData", Status = "Pending", LastCalled = null }
        });
    }

    private async Task<bool> CallExternalServiceAsync(string serviceName, ServiceCoordinationRequest request)
    {
        // This would implement actual external service calls
        // For now, simulate success
        await Task.Delay(100); // Simulate network call
        return true;
    }
}