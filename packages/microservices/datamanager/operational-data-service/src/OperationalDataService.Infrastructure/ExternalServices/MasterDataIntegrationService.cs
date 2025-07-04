using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Interfaces;
using System.Text;

namespace OperationalDataService.Infrastructure.ExternalServices;

public class MasterDataIntegrationService : IMasterDataIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MasterDataIntegrationService> _logger;

    public MasterDataIntegrationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MasterDataIntegrationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ProductCatalogDto?> GetProductFromMasterDataAsync(string productId)
    {
        try
        {
            var productServiceUrl = _configuration["MasterDataServices:ProductService"];
            var response = await _httpClient.GetAsync($"{productServiceUrl}/api/products/{productId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductCatalogDto>(content);
            }

            _logger.LogWarning("Failed to get product {ProductId} from master data service. Status: {StatusCode}", 
                productId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId} from master data service", productId);
            return null;
        }
    }

    public async Task<List<ProductCatalogDto>> GetProductsFromMasterDataAsync(string organizationId)
    {
        try
        {
            var productServiceUrl = _configuration["MasterDataServices:ProductService"];
            var response = await _httpClient.GetAsync($"{productServiceUrl}/api/products/organization/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ProductCatalogDto>>(content) ?? new List<ProductCatalogDto>();
            }

            _logger.LogWarning("Failed to get products for organization {OrganizationId} from master data service. Status: {StatusCode}", 
                organizationId, response.StatusCode);
            return new List<ProductCatalogDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for organization {OrganizationId} from master data service", organizationId);
            return new List<ProductCatalogDto>();
        }
    }

    public async Task<ProductPricingDto?> GetProductPricingFromMasterDataAsync(string productId, DateTime date)
    {
        try
        {
            var productServiceUrl = _configuration["MasterDataServices:ProductService"];
            var response = await _httpClient.GetAsync($"{productServiceUrl}/api/products/{productId}/pricing?date={date:yyyy-MM-dd}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ProductPricingDto>(content);
            }

            _logger.LogWarning("Failed to get pricing for product {ProductId} from master data service. Status: {StatusCode}", 
                productId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pricing for product {ProductId} from master data service", productId);
            return null;
        }
    }

    public async Task<bool> ValidateProductWithMasterDataAsync(string productId)
    {
        try
        {
            var productServiceUrl = _configuration["MasterDataServices:ProductService"];
            var response = await _httpClient.GetAsync($"{productServiceUrl}/api/products/{productId}/validate");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId} with master data service", productId);
            return false;
        }
    }

    public async Task<OptimizedRoute?> GetRouteFromMasterDataAsync(string routeId)
    {
        try
        {
            var routeServiceUrl = _configuration["MasterDataServices:RouteService"];
            var response = await _httpClient.GetAsync($"{routeServiceUrl}/api/routes/{routeId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OptimizedRoute>(content);
            }

            _logger.LogWarning("Failed to get route {RouteId} from master data service. Status: {StatusCode}", 
                routeId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route {RouteId} from master data service", routeId);
            return null;
        }
    }

    public async Task<List<OptimizedRoute>> GetRoutesFromMasterDataAsync(string organizationId)
    {
        try
        {
            var routeServiceUrl = _configuration["MasterDataServices:RouteService"];
            var response = await _httpClient.GetAsync($"{routeServiceUrl}/api/routes/organization/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<OptimizedRoute>>(content) ?? new List<OptimizedRoute>();
            }

            _logger.LogWarning("Failed to get routes for organization {OrganizationId} from master data service. Status: {StatusCode}", 
                organizationId, response.StatusCode);
            return new List<OptimizedRoute>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting routes for organization {OrganizationId} from master data service", organizationId);
            return new List<OptimizedRoute>();
        }
    }

    public async Task<TrafficData?> GetTrafficDataFromExternalSourceAsync(string routeId)
    {
        try
        {
            // This would typically integrate with external traffic services like Google Maps, Waze, etc.
            // For now, we'll use a placeholder implementation
            var routeServiceUrl = _configuration["MasterDataServices:RouteService"];
            var response = await _httpClient.GetAsync($"{routeServiceUrl}/api/routes/{routeId}/traffic");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TrafficData>(content);
            }

            _logger.LogWarning("Failed to get traffic data for route {RouteId} from external source. Status: {StatusCode}", 
                routeId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting traffic data for route {RouteId} from external source", routeId);
            return null;
        }
    }

    public async Task<WeighbridgeCapacity?> GetWeighbridgeStatusFromMasterDataAsync(string weighbridgeId)
    {
        try
        {
            var weighbridgeServiceUrl = _configuration["MasterDataServices:WeighbridgeService"];
            var response = await _httpClient.GetAsync($"{weighbridgeServiceUrl}/api/weighbridges/{weighbridgeId}/status");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<WeighbridgeCapacity>(content);
            }

            _logger.LogWarning("Failed to get weighbridge status for {WeighbridgeId} from master data service. Status: {StatusCode}", 
                weighbridgeId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weighbridge status for {WeighbridgeId} from master data service", weighbridgeId);
            return null;
        }
    }

    public async Task<List<WeighbridgeCapacity>> GetWeighbridgesFromMasterDataAsync(string organizationId)
    {
        try
        {
            var weighbridgeServiceUrl = _configuration["MasterDataServices:WeighbridgeService"];
            var response = await _httpClient.GetAsync($"{weighbridgeServiceUrl}/api/weighbridges/organization/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<WeighbridgeCapacity>>(content) ?? new List<WeighbridgeCapacity>();
            }

            _logger.LogWarning("Failed to get weighbridges for organization {OrganizationId} from master data service. Status: {StatusCode}", 
                organizationId, response.StatusCode);
            return new List<WeighbridgeCapacity>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weighbridges for organization {OrganizationId} from master data service", organizationId);
            return new List<WeighbridgeCapacity>();
        }
    }

    public async Task<bool> UpdateWeighbridgeStatusInMasterDataAsync(string weighbridgeId, WeighbridgeCapacity capacity)
    {
        try
        {
            var weighbridgeServiceUrl = _configuration["MasterDataServices:WeighbridgeService"];
            var json = JsonConvert.SerializeObject(capacity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PutAsync($"{weighbridgeServiceUrl}/api/weighbridges/{weighbridgeId}/status", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully updated weighbridge status for {WeighbridgeId}", weighbridgeId);
                return true;
            }

            _logger.LogWarning("Failed to update weighbridge status for {WeighbridgeId}. Status: {StatusCode}", 
                weighbridgeId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating weighbridge status for {WeighbridgeId}", weighbridgeId);
            return false;
        }
    }

    public async Task<VehicleInfo?> GetVehicleInfoAsync(string vehicleId)
    {
        try
        {
            var vehicleServiceUrl = _configuration["MasterDataServices:VehicleService"];
            var response = await _httpClient.GetAsync($"{vehicleServiceUrl}/api/vehicles/{vehicleId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<VehicleInfo>(content);
            }

            _logger.LogWarning("Failed to get vehicle info for {VehicleId} from master data service. Status: {StatusCode}", 
                vehicleId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle info for {VehicleId} from master data service", vehicleId);
            return null;
        }
    }

    public async Task<List<VehicleInfo>> GetVehiclesByOrganizationAsync(string organizationId)
    {
        try
        {
            var vehicleServiceUrl = _configuration["MasterDataServices:VehicleService"];
            var response = await _httpClient.GetAsync($"{vehicleServiceUrl}/api/vehicles/organization/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VehicleInfo>>(content) ?? new List<VehicleInfo>();
            }

            _logger.LogWarning("Failed to get vehicles for organization {OrganizationId} from master data service. Status: {StatusCode}", 
                organizationId, response.StatusCode);
            return new List<VehicleInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles for organization {OrganizationId} from master data service", organizationId);
            return new List<VehicleInfo>();
        }
    }

    public async Task<bool> ValidateVehicleForProductAsync(string vehicleId, string productId)
    {
        try
        {
            var vehicleServiceUrl = _configuration["MasterDataServices:VehicleService"];
            var response = await _httpClient.GetAsync($"{vehicleServiceUrl}/api/vehicles/{vehicleId}/validate-product/{productId}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating vehicle {VehicleId} for product {ProductId}", vehicleId, productId);
            return false;
        }
    }

    public async Task<OrganizationInfo?> GetOrganizationInfoAsync(string organizationId)
    {
        try
        {
            var organizationServiceUrl = _configuration["MasterDataServices:OrganizationService"];
            var response = await _httpClient.GetAsync($"{organizationServiceUrl}/api/organizations/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<OrganizationInfo>(content);
            }

            _logger.LogWarning("Failed to get organization info for {OrganizationId} from master data service. Status: {StatusCode}", 
                organizationId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting organization info for {OrganizationId} from master data service", organizationId);
            return null;
        }
    }

    public async Task<List<OrganizationInfo>> GetAllOrganizationsAsync()
    {
        try
        {
            var organizationServiceUrl = _configuration["MasterDataServices:OrganizationService"];
            var response = await _httpClient.GetAsync($"{organizationServiceUrl}/api/organizations");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<OrganizationInfo>>(content) ?? new List<OrganizationInfo>();
            }

            _logger.LogWarning("Failed to get all organizations from master data service. Status: {StatusCode}", response.StatusCode);
            return new List<OrganizationInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all organizations from master data service");
            return new List<OrganizationInfo>();
        }
    }

    public async Task<SyncResult> SyncAllDataAsync(string organizationId)
    {
        var result = new SyncResult { IsSuccess = true };

        try
        {
            // Sync products
            var productSync = await SyncProductDataAsync(organizationId);
            result.RecordsSynced += productSync.RecordsSynced;
            result.RecordsFailed += productSync.RecordsFailed;
            result.Errors.AddRange(productSync.Errors);

            // Sync routes
            var routeSync = await SyncRouteDataAsync(organizationId);
            result.RecordsSynced += routeSync.RecordsSynced;
            result.RecordsFailed += routeSync.RecordsFailed;
            result.Errors.AddRange(routeSync.Errors);

            // Sync weighbridges
            var weighbridgeSync = await SyncWeighbridgeDataAsync(organizationId);
            result.RecordsSynced += weighbridgeSync.RecordsSynced;
            result.RecordsFailed += weighbridgeSync.RecordsFailed;
            result.Errors.AddRange(weighbridgeSync.Errors);

            result.IsSuccess = result.RecordsFailed == 0;
            result.Details = $"Synced {result.RecordsSynced} records, {result.RecordsFailed} failed";

            _logger.LogInformation("Sync completed for organization {OrganizationId}. Success: {IsSuccess}, Records: {RecordsSynced}, Failed: {RecordsFailed}", 
                organizationId, result.IsSuccess, result.RecordsSynced, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during full sync for organization {OrganizationId}", organizationId);
            result.IsSuccess = false;
            result.Errors.Add($"Sync failed with error: {ex.Message}");
        }

        return result;
    }

    public async Task<SyncResult> SyncProductDataAsync(string organizationId)
    {
        var result = new SyncResult();

        try
        {
            var products = await GetProductsFromMasterDataAsync(organizationId);
            result.RecordsSynced = products.Count;
            result.IsSuccess = true;
            result.Details = $"Successfully synced {products.Count} products";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing product data for organization {OrganizationId}", organizationId);
            result.IsSuccess = false;
            result.RecordsFailed = 1;
            result.Errors.Add($"Product sync failed: {ex.Message}");
        }

        return result;
    }

    public async Task<SyncResult> SyncRouteDataAsync(string organizationId)
    {
        var result = new SyncResult();

        try
        {
            var routes = await GetRoutesFromMasterDataAsync(organizationId);
            result.RecordsSynced = routes.Count;
            result.IsSuccess = true;
            result.Details = $"Successfully synced {routes.Count} routes";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing route data for organization {OrganizationId}", organizationId);
            result.IsSuccess = false;
            result.RecordsFailed = 1;
            result.Errors.Add($"Route sync failed: {ex.Message}");
        }

        return result;
    }

    public async Task<SyncResult> SyncWeighbridgeDataAsync(string organizationId)
    {
        var result = new SyncResult();

        try
        {
            var weighbridges = await GetWeighbridgesFromMasterDataAsync(organizationId);
            result.RecordsSynced = weighbridges.Count;
            result.IsSuccess = true;
            result.Details = $"Successfully synced {weighbridges.Count} weighbridges";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing weighbridge data for organization {OrganizationId}", organizationId);
            result.IsSuccess = false;
            result.RecordsFailed = 1;
            result.Errors.Add($"Weighbridge sync failed: {ex.Message}");
        }

        return result;
    }

    public async Task<bool> IsServiceHealthyAsync(string serviceName)
    {
        try
        {
            var serviceUrl = _configuration[$"MasterDataServices:{serviceName}"];
            if (string.IsNullOrEmpty(serviceUrl))
            {
                return false;
            }

            var response = await _httpClient.GetAsync($"{serviceUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for service {ServiceName}", serviceName);
            return false;
        }
    }

    public async Task<Dictionary<string, bool>> GetAllServicesHealthAsync()
    {
        var services = new[] { "VehicleService", "DriverService", "SupplierService", "ProductService", "RouteService", "WeighbridgeService", "OrganizationService" };
        var healthStatus = new Dictionary<string, bool>();

        var healthTasks = services.Select(async service =>
        {
            var isHealthy = await IsServiceHealthyAsync(service);
            return new { Service = service, IsHealthy = isHealthy };
        });

        var results = await Task.WhenAll(healthTasks);

        foreach (var result in results)
        {
            healthStatus[result.Service] = result.IsHealthy;
        }

        return healthStatus;
    }
}