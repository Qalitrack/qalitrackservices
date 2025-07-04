using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Core.Interfaces;

public interface IMasterDataIntegrationService
{
    // Product Integration
    Task<ProductCatalogDto?> GetProductFromMasterDataAsync(string productId);
    Task<List<ProductCatalogDto>> GetProductsFromMasterDataAsync(string organizationId);
    Task<ProductPricingDto?> GetProductPricingFromMasterDataAsync(string productId, DateTime date);
    Task<bool> ValidateProductWithMasterDataAsync(string productId);
    
    // Route Integration
    Task<OptimizedRoute?> GetRouteFromMasterDataAsync(string routeId);
    Task<List<OptimizedRoute>> GetRoutesFromMasterDataAsync(string organizationId);
    Task<TrafficData?> GetTrafficDataFromExternalSourceAsync(string routeId);
    
    // Weighbridge Integration
    Task<WeighbridgeCapacity?> GetWeighbridgeStatusFromMasterDataAsync(string weighbridgeId);
    Task<List<WeighbridgeCapacity>> GetWeighbridgesFromMasterDataAsync(string organizationId);
    Task<bool> UpdateWeighbridgeStatusInMasterDataAsync(string weighbridgeId, WeighbridgeCapacity capacity);
    
    // Vehicle Integration
    Task<VehicleInfo?> GetVehicleInfoAsync(string vehicleId);
    Task<List<VehicleInfo>> GetVehiclesByOrganizationAsync(string organizationId);
    Task<bool> ValidateVehicleForProductAsync(string vehicleId, string productId);
    
    // Organization Integration
    Task<OrganizationInfo?> GetOrganizationInfoAsync(string organizationId);
    Task<List<OrganizationInfo>> GetAllOrganizationsAsync();
    
    // Synchronization
    Task<SyncResult> SyncAllDataAsync(string organizationId);
    Task<SyncResult> SyncProductDataAsync(string organizationId);
    Task<SyncResult> SyncRouteDataAsync(string organizationId);
    Task<SyncResult> SyncWeighbridgeDataAsync(string organizationId);
    
    // Health Check
    Task<bool> IsServiceHealthyAsync(string serviceName);
    Task<Dictionary<string, bool>> GetAllServicesHealthAsync();
}

public class VehicleInfo
{
    public string Id { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal MaxWeight { get; set; }
    public List<string> AllowedProducts { get; set; } = new();
    public bool IsActive { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
}

public class OrganizationInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<string> WeighbridgeIds { get; set; } = new();
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class SyncResult
{
    public bool IsSuccess { get; set; }
    public DateTime SyncTime { get; set; } = DateTime.UtcNow;
    public int RecordsSynced { get; set; }
    public int RecordsSkipped { get; set; }
    public int RecordsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Details { get; set; }
}