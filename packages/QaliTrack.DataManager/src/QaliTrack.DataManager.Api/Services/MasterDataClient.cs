using System.Text.Json;

namespace QaliTrack.DataManager.Api.Services;

public interface IMasterDataClient
{
    Task<bool> ValidateVehicleAsync(Guid vehicleId, Guid organizationId);
    Task<bool> ValidateDriverAsync(Guid driverId, Guid organizationId);
    Task<bool> ValidateCustomerAsync(Guid customerId, Guid organizationId);
    Task<bool> ValidateSupplierAsync(Guid supplierId, Guid organizationId);
    Task<bool> ValidateProductAsync(Guid productId, Guid organizationId);
    Task<bool> ValidateRouteAsync(Guid routeId, Guid organizationId);
    Task<bool> ValidateWeighbridgeAsync(Guid weighbridgeId, Guid organizationId);
    Task<MasterDataEntity?> GetVehicleAsync(Guid vehicleId, Guid organizationId);
    Task<MasterDataEntity?> GetDriverAsync(Guid driverId, Guid organizationId);
    Task<MasterDataEntity?> GetCustomerAsync(Guid customerId, Guid organizationId);
    Task<MasterDataEntity?> GetSupplierAsync(Guid supplierId, Guid organizationId);
    Task<MasterDataEntity?> GetProductAsync(Guid productId, Guid organizationId);
}

public class MasterDataClient : IMasterDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MasterDataClient> _logger;
    private readonly string _baseUrl;

    public MasterDataClient(HttpClient httpClient, ILogger<MasterDataClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["MASTER_DATA_SERVICE_URL"] ?? "http://localhost:5000";
        
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "QaliTrack-DataManager/1.0");
    }

    public async Task<bool> ValidateVehicleAsync(Guid vehicleId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/vehicle/{vehicleId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            
            _logger.LogWarning("Vehicle validation failed for {VehicleId}: {StatusCode}", vehicleId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating vehicle {VehicleId}", vehicleId);
            return false; // Fail closed for validation
        }
    }

    public async Task<bool> ValidateDriverAsync(Guid driverId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/driver/{driverId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating driver {DriverId}", driverId);
            return false;
        }
    }

    public async Task<bool> ValidateCustomerAsync(Guid customerId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/businessentity/{customerId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating customer {CustomerId}", customerId);
            return false;
        }
    }

    public async Task<bool> ValidateSupplierAsync(Guid supplierId, Guid organizationId)
    {
        return await ValidateCustomerAsync(supplierId, organizationId); // Same endpoint for business entities
    }

    public async Task<bool> ValidateProductAsync(Guid productId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> ValidateRouteAsync(Guid routeId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/route/{routeId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating route {RouteId}", routeId);
            return false;
        }
    }

    public async Task<bool> ValidateWeighbridgeAsync(Guid weighbridgeId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/weighbridge/{weighbridgeId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                return apiResponse?.Success == true && apiResponse.Data != null;
            }
            
            return response.StatusCode != System.Net.HttpStatusCode.NotFound;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating weighbridge {WeighbridgeId}", weighbridgeId);
            return false;
        }
    }

    public async Task<MasterDataEntity?> GetVehicleAsync(Guid vehicleId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/vehicle/{vehicleId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return JsonSerializer.Deserialize<MasterDataEntity>(apiResponse.Data.ToString()!, JsonOptions());
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicle {VehicleId}", vehicleId);
            return null;
        }
    }

    public async Task<MasterDataEntity?> GetDriverAsync(Guid driverId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/driver/{driverId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return JsonSerializer.Deserialize<MasterDataEntity>(apiResponse.Data.ToString()!, JsonOptions());
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting driver {DriverId}", driverId);
            return null;
        }
    }

    public async Task<MasterDataEntity?> GetCustomerAsync(Guid customerId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/businessentity/{customerId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return JsonSerializer.Deserialize<MasterDataEntity>(apiResponse.Data.ToString()!, JsonOptions());
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", customerId);
            return null;
        }
    }

    public async Task<MasterDataEntity?> GetSupplierAsync(Guid supplierId, Guid organizationId)
    {
        return await GetCustomerAsync(supplierId, organizationId); // Same endpoint for business entities
    }

    public async Task<MasterDataEntity?> GetProductAsync(Guid productId, Guid organizationId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Remove("X-Organization-Id");
            _httpClient.DefaultRequestHeaders.Add("X-Organization-Id", organizationId.ToString());
            
            var response = await _httpClient.GetAsync($"/product/{productId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<MasterDataApiResponse>(content, JsonOptions());
                
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    return JsonSerializer.Deserialize<MasterDataEntity>(apiResponse.Data.ToString()!, JsonOptions());
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return null;
        }
    }

    private static JsonSerializerOptions JsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}

public class MasterDataApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; }
}

public class MasterDataEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}