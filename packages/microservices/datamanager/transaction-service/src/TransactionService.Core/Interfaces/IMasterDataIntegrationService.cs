namespace TransactionService.Core.Interfaces;

public interface IMasterDataIntegrationService
{
    Task<bool> ValidateVehicleAsync(string vehicleId);
    Task<bool> ValidateDriverAsync(string driverId);
    Task<bool> ValidateSupplierAsync(string supplierId);
    Task<bool> ValidateCustomerAsync(string customerId);
    Task<bool> ValidateProductAsync(string productId);
    Task<bool> ValidateRouteAsync(string routeId);
    Task<bool> ValidateWeighbridgeAsync(string weighbridgeId);
    Task<bool> ValidateOrganizationAsync(string organizationId);
    Task<Dictionary<string, object>?> GetVehicleDetailsAsync(string vehicleId);
    Task<Dictionary<string, object>?> GetDriverDetailsAsync(string driverId);
    Task<Dictionary<string, object>?> GetSupplierDetailsAsync(string supplierId);
    Task<Dictionary<string, object>?> GetCustomerDetailsAsync(string customerId);
    Task<Dictionary<string, object>?> GetProductDetailsAsync(string productId);
}