using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using TransactionService.Core.Interfaces;

namespace TransactionService.Infrastructure.ExternalServices;

public class MasterDataIntegrationService : IMasterDataIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MasterDataIntegrationService> _logger;
    private readonly Dictionary<string, string> _serviceUrls;

    public MasterDataIntegrationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<MasterDataIntegrationService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        
        _serviceUrls = new Dictionary<string, string>
        {
            ["Vehicle"] = _configuration["MasterDataServices:VehicleService"] ?? "http://localhost:5001",
            ["Driver"] = _configuration["MasterDataServices:DriverService"] ?? "http://localhost:5002",
            ["Supplier"] = _configuration["MasterDataServices:SupplierService"] ?? "http://localhost:5003",
            ["Customer"] = _configuration["MasterDataServices:CustomerService"] ?? "http://localhost:5003", // Using same as supplier
            ["Product"] = _configuration["MasterDataServices:ProductService"] ?? "http://localhost:5004",
            ["Route"] = _configuration["MasterDataServices:RouteService"] ?? "http://localhost:5005",
            ["Weighbridge"] = _configuration["MasterDataServices:WeighbridgeService"] ?? "http://localhost:5006",
            ["Organization"] = _configuration["MasterDataServices:OrganizationService"] ?? "http://localhost:5007"
        };
    }

    public async Task<bool> ValidateVehicleAsync(string vehicleId)
    {
        return await ValidateEntityAsync("Vehicle", "vehicles", vehicleId);
    }

    public async Task<bool> ValidateDriverAsync(string driverId)
    {
        return await ValidateEntityAsync("Driver", "drivers", driverId);
    }

    public async Task<bool> ValidateSupplierAsync(string supplierId)
    {
        return await ValidateEntityAsync("Supplier", "suppliers", supplierId);
    }

    public async Task<bool> ValidateCustomerAsync(string customerId)
    {
        return await ValidateEntityAsync("Customer", "customers", customerId);
    }

    public async Task<bool> ValidateProductAsync(string productId)
    {
        return await ValidateEntityAsync("Product", "products", productId);
    }

    public async Task<bool> ValidateRouteAsync(string routeId)
    {
        return await ValidateEntityAsync("Route", "routes", routeId);
    }

    public async Task<bool> ValidateWeighbridgeAsync(string weighbridgeId)
    {
        return await ValidateEntityAsync("Weighbridge", "weighbridges", weighbridgeId);
    }

    public async Task<bool> ValidateOrganizationAsync(string organizationId)
    {
        return await ValidateEntityAsync("Organization", "organizations", organizationId);
    }

    public async Task<Dictionary<string, object>?> GetVehicleDetailsAsync(string vehicleId)
    {
        return await GetEntityDetailsAsync("Vehicle", "vehicles", vehicleId);
    }

    public async Task<Dictionary<string, object>?> GetDriverDetailsAsync(string driverId)
    {
        return await GetEntityDetailsAsync("Driver", "drivers", driverId);
    }

    public async Task<Dictionary<string, object>?> GetSupplierDetailsAsync(string supplierId)
    {
        return await GetEntityDetailsAsync("Supplier", "suppliers", supplierId);
    }

    public async Task<Dictionary<string, object>?> GetCustomerDetailsAsync(string customerId)
    {
        return await GetEntityDetailsAsync("Customer", "customers", customerId);
    }

    public async Task<Dictionary<string, object>?> GetProductDetailsAsync(string productId)
    {
        return await GetEntityDetailsAsync("Product", "products", productId);
    }

    private async Task<bool> ValidateEntityAsync(string serviceName, string endpoint, string entityId)
    {
        try
        {
            if (!_serviceUrls.TryGetValue(serviceName, out var baseUrl))
            {
                _logger.LogWarning("Service URL not configured for {ServiceName}", serviceName);
                return false; // Fail gracefully if service not configured
            }

            var url = $"{baseUrl}/api/{endpoint}/{entityId}";
            _logger.LogDebug("Validating {ServiceName} with ID {EntityId} at {Url}", serviceName, entityId, url);

            var response = await _httpClient.GetAsync(url);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogDebug("{ServiceName} {EntityId} validation successful", serviceName, entityId);
                return true;
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("{ServiceName} {EntityId} not found", serviceName, entityId);
                return false;
            }

            _logger.LogWarning("Unexpected response from {ServiceName} service: {StatusCode}", serviceName, response.StatusCode);
            return false; // Fail for other status codes
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error validating {ServiceName} {EntityId}", serviceName, entityId);
            return false; // Fail if service is unreachable
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout validating {ServiceName} {EntityId}", serviceName, entityId);
            return false; // Fail on timeout
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating {ServiceName} {EntityId}", serviceName, entityId);
            return false; // Fail for any other error
        }
    }

    private async Task<Dictionary<string, object>?> GetEntityDetailsAsync(string serviceName, string endpoint, string entityId)
    {
        try
        {
            if (!_serviceUrls.TryGetValue(serviceName, out var baseUrl))
            {
                _logger.LogWarning("Service URL not configured for {ServiceName}", serviceName);
                return null;
            }

            var url = $"{baseUrl}/api/{endpoint}/{entityId}";
            _logger.LogDebug("Getting {ServiceName} details for ID {EntityId} at {Url}", serviceName, entityId, url);

            var response = await _httpClient.GetAsync(url);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var details = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                _logger.LogDebug("{ServiceName} {EntityId} details retrieved successfully", serviceName, entityId);
                return details;
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("{ServiceName} {EntityId} not found", serviceName, entityId);
                return null;
            }

            _logger.LogWarning("Unexpected response from {ServiceName} service: {StatusCode}", serviceName, response.StatusCode);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting {ServiceName} {EntityId} details", serviceName, entityId);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timeout getting {ServiceName} {EntityId} details", serviceName, entityId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {ServiceName} {EntityId} details", serviceName, entityId);
            return null;
        }
    }
}