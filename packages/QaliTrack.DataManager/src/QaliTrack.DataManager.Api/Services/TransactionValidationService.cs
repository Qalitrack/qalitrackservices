using QaliTrack.DataManager.Core.Modules.Transactions.DTOs;

namespace QaliTrack.DataManager.Api.Services;

public interface ITransactionValidationService
{
    Task<ValidationResult> ValidateTransactionAsync(CreateWeighingTransactionDto dto, Guid organizationId);
    Task<ValidationResult> ValidateWeightMeasurementAsync(Guid transactionId, Guid weighbridgeId, Guid organizationId);
}

public class TransactionValidationService : ITransactionValidationService
{
    private readonly IMasterDataClient _masterDataClient;
    private readonly ILogger<TransactionValidationService> _logger;

    public TransactionValidationService(IMasterDataClient masterDataClient, ILogger<TransactionValidationService> logger)
    {
        _masterDataClient = masterDataClient;
        _logger = logger;
    }

    public async Task<ValidationResult> ValidateTransactionAsync(CreateWeighingTransactionDto dto, Guid organizationId)
    {
        var result = new ValidationResult();

        try
        {
            // Validate required master data entities in parallel
            var validationTasks = new List<Task<(bool IsValid, string EntityType, Guid EntityId)>>
            {
                ValidateEntityAsync(() => _masterDataClient.ValidateVehicleAsync(dto.VehicleId, organizationId), "Vehicle", dto.VehicleId),
                ValidateEntityAsync(() => _masterDataClient.ValidateDriverAsync(dto.DriverId, organizationId), "Driver", dto.DriverId),
                ValidateEntityAsync(() => _masterDataClient.ValidateProductAsync(dto.ProductId, organizationId), "Product", dto.ProductId),
                ValidateEntityAsync(() => _masterDataClient.ValidateRouteAsync(dto.RouteId, organizationId), "Route", dto.RouteId),
                ValidateEntityAsync(() => _masterDataClient.ValidateWeighbridgeAsync(dto.WeighbridgeId, organizationId), "Weighbridge", dto.WeighbridgeId)
            };

            // Validate optional entities
            if (dto.CustomerId.HasValue)
            {
                validationTasks.Add(ValidateEntityAsync(() => _masterDataClient.ValidateCustomerAsync(dto.CustomerId.Value, organizationId), "Customer", dto.CustomerId.Value));
            }

            if (dto.SupplierId.HasValue)
            {
                validationTasks.Add(ValidateEntityAsync(() => _masterDataClient.ValidateSupplierAsync(dto.SupplierId.Value, organizationId), "Supplier", dto.SupplierId.Value));
            }

            var validationResults = await Task.WhenAll(validationTasks);

            foreach (var (isValid, entityType, entityId) in validationResults)
            {
                if (!isValid)
                {
                    result.AddError($"{entityType} with ID {entityId} not found or invalid");
                }
            }

            // Business rule validations
            await ValidateBusinessRulesAsync(dto, organizationId, result);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating transaction for organization {OrganizationId}", organizationId);
            result.AddError("Validation service temporarily unavailable");
        }

        return result;
    }

    public async Task<ValidationResult> ValidateWeightMeasurementAsync(Guid transactionId, Guid weighbridgeId, Guid organizationId)
    {
        var result = new ValidationResult();

        try
        {
            // Validate weighbridge exists and is accessible
            var weighbridgeValid = await _masterDataClient.ValidateWeighbridgeAsync(weighbridgeId, organizationId);
            if (!weighbridgeValid)
            {
                result.AddError($"Weighbridge with ID {weighbridgeId} not found or not accessible");
            }

            // Additional business rules can be added here
            // e.g., check if weighbridge is operational, calibrated, etc.

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating weight measurement for transaction {TransactionId}", transactionId);
            result.AddError("Weight measurement validation service temporarily unavailable");
        }

        return result;
    }

    private async Task<(bool IsValid, string EntityType, Guid EntityId)> ValidateEntityAsync(
        Func<Task<bool>> validationFunc, 
        string entityType, 
        Guid entityId)
    {
        try
        {
            var isValid = await validationFunc();
            return (isValid, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating {EntityType} {EntityId}", entityType, entityId);
            return (false, entityType, entityId);
        }
    }

    private async Task ValidateBusinessRulesAsync(CreateWeighingTransactionDto dto, Guid organizationId, ValidationResult result)
    {
        // Example business rule validations
        
        // Rule 1: Ensure transaction type is valid
        if (!IsValidTransactionType(dto.TransactionType))
        {
            result.AddError($"Invalid transaction type: {dto.TransactionType}");
        }

        // Rule 2: Validate currency
        if (!IsValidCurrency(dto.Currency))
        {
            result.AddError($"Invalid currency: {dto.Currency}");
        }

        // Rule 3: Validate priority level
        if (dto.Priority < 1 || dto.Priority > 10)
        {
            result.AddError("Priority must be between 1 and 10");
        }

        // Rule 4: For purchase transactions, supplier is required
        if (dto.TransactionType.Equals("Purchase", StringComparison.OrdinalIgnoreCase) && !dto.SupplierId.HasValue)
        {
            result.AddError("Supplier is required for purchase transactions");
        }

        // Rule 5: For sale transactions, customer is required
        if (dto.TransactionType.Equals("Sale", StringComparison.OrdinalIgnoreCase) && !dto.CustomerId.HasValue)
        {
            result.AddError("Customer is required for sale transactions");
        }

        // Rule 6: Validate unit price is positive if provided
        if (dto.UnitPrice.HasValue && dto.UnitPrice.Value <= 0)
        {
            result.AddError("Unit price must be positive");
        }

        await Task.CompletedTask; // For async consistency
    }

    private static bool IsValidTransactionType(string transactionType)
    {
        var validTypes = new[] { "Purchase", "Sale", "Transfer", "Internal", "Return" };
        return validTypes.Contains(transactionType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsValidCurrency(string currency)
    {
        var validCurrencies = new[] { "USD", "EUR", "GBP", "KES", "UGX", "TZS", "RWF" };
        return validCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase);
    }
}

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<string> Errors { get; private set; } = new();
    public List<string> Warnings { get; private set; } = new();

    public void AddError(string error)
    {
        Errors.Add(error);
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }
}