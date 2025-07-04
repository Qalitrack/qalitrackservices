using AutoMapper;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IWorkflowService _workflowService;
    private readonly IChargeService _chargeService;
    private readonly IMasterDataIntegrationService _masterDataService;
    private readonly IMapper _mapper;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IWorkflowService workflowService,
        IChargeService chargeService,
        IMasterDataIntegrationService masterDataService,
        IMapper mapper)
    {
        _transactionRepository = transactionRepository;
        _workflowService = workflowService;
        _chargeService = chargeService;
        _masterDataService = masterDataService;
        _mapper = mapper;
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request)
    {
        // Validate master data entities
        await ValidateMasterDataAsync(request);
        
        var transaction = _mapper.Map<WeighingTransaction>(request);
        transaction.TransactionNumber = await _transactionRepository.GenerateTransactionNumberAsync();
        
        var createdTransaction = await _transactionRepository.AddAsync(transaction);
        
        // Initialize workflow
        await _workflowService.InitializeWorkflowAsync(createdTransaction.Id);
        
        // Calculate initial charges
        await _chargeService.CalculateChargesAsync(createdTransaction.Id);
        
        return _mapper.Map<TransactionDto>(createdTransaction);
    }

    private async Task ValidateMasterDataAsync(CreateTransactionRequest request)
    {
        var validationTasks = new List<Task<(string entity, bool isValid)>>
        {
            ValidateEntityAsync("Vehicle", () => _masterDataService.ValidateVehicleAsync(request.VehicleId)),
            ValidateEntityAsync("Driver", () => _masterDataService.ValidateDriverAsync(request.DriverId)),
            ValidateEntityAsync("Supplier", () => _masterDataService.ValidateSupplierAsync(request.SupplierId)),
            ValidateEntityAsync("Product", () => _masterDataService.ValidateProductAsync(request.ProductId)),
            ValidateEntityAsync("Route", () => _masterDataService.ValidateRouteAsync(request.RouteId)),
            ValidateEntityAsync("Weighbridge", () => _masterDataService.ValidateWeighbridgeAsync(request.WeighbridgeId)),
            ValidateEntityAsync("Organization", () => _masterDataService.ValidateOrganizationAsync(request.OrganizationId))
        };

        if (!string.IsNullOrEmpty(request.CustomerId))
        {
            validationTasks.Add(ValidateEntityAsync("Customer", () => _masterDataService.ValidateCustomerAsync(request.CustomerId)));
        }

        var results = await Task.WhenAll(validationTasks);
        var invalidEntities = results.Where(r => !r.isValid).Select(r => r.entity).ToList();

        if (invalidEntities.Any())
        {
            throw new InvalidOperationException($"Invalid master data entities: {string.Join(", ", invalidEntities)}");
        }
    }

    private static async Task<(string entity, bool isValid)> ValidateEntityAsync(string entityName, Func<Task<bool>> validationFunc)
    {
        try
        {
            var isValid = await validationFunc();
            return (entityName, isValid);
        }
        catch
        {
            return (entityName, false);
        }
    }

    public async Task<TransactionDto?> GetTransactionAsync(string id)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
    }

    public async Task<TransactionDto?> GetCompleteTransactionAsync(string id)
    {
        var transaction = await _transactionRepository.GetCompleteAsync(id);
        return transaction == null ? null : _mapper.Map<TransactionDto>(transaction);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var transactions = await _transactionRepository.GetPagedAsync(pageNumber, pageSize);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByVehicleAsync(string vehicleId)
    {
        var transactions = await _transactionRepository.GetByVehicleIdAsync(vehicleId);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByOrganizationAsync(string organizationId)
    {
        var transactions = await _transactionRepository.GetByOrganizationIdAsync(organizationId);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByStatusAsync(TransactionStatus status)
    {
        var transactions = await _transactionRepository.GetByStatusAsync(status);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
        return _mapper.Map<IEnumerable<TransactionDto>>(transactions);
    }

    public async Task<TransactionDto> UpdateTransactionAsync(string id, UpdateTransactionRequest request)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            throw new InvalidOperationException($"Transaction with ID {id} not found");

        // Update properties
        if (request.Status.HasValue)
            transaction.Status = request.Status.Value;
        if (request.GrossWeight.HasValue)
            transaction.GrossWeight = request.GrossWeight.Value;
        if (request.TareWeight.HasValue)
            transaction.TareWeight = request.TareWeight.Value;
        if (request.NetWeight.HasValue)
            transaction.NetWeight = request.NetWeight.Value;
        if (request.EntryWeighingTime.HasValue)
            transaction.EntryWeighingTime = request.EntryWeighingTime.Value;
        if (request.ExitWeighingTime.HasValue)
            transaction.ExitWeighingTime = request.ExitWeighingTime.Value;
        if (!string.IsNullOrEmpty(request.DeliveryNoteNumber))
            transaction.DeliveryNoteNumber = request.DeliveryNoteNumber;
        if (!string.IsNullOrEmpty(request.PermitNumber))
            transaction.PermitNumber = request.PermitNumber;
        if (!string.IsNullOrEmpty(request.Remarks))
            transaction.Remarks = request.Remarks;
        if (request.Metadata != null)
            transaction.Metadata = request.Metadata;

        var updatedTransaction = await _transactionRepository.UpdateAsync(transaction);
        return _mapper.Map<TransactionDto>(updatedTransaction);
    }

    public async Task<bool> DeleteTransactionAsync(string id)
    {
        return await _transactionRepository.DeleteByIdAsync(id);
    }

    public async Task<bool> CancelTransactionAsync(string id, string reason)
    {
        var transaction = await _transactionRepository.GetByIdAsync(id);
        if (transaction == null)
            return false;

        transaction.Status = TransactionStatus.Cancelled;
        transaction.Remarks = $"{transaction.Remarks}\nCancelled: {reason}";
        
        await _transactionRepository.UpdateAsync(transaction);
        return true;
    }
}