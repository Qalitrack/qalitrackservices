using ServiceDiscovery.Models;
using System.Text.Json;

namespace ServiceDiscovery.Services;

public class ServiceAggregatorService : IServiceAggregatorService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServiceAggregatorService> _logger;
    private readonly IConfiguration _configuration;

    // Known service configurations - can be moved to config later
    private readonly Dictionary<int, string> _knownServices = new()
    {
        { 7000, "API Gateway" },
        { 7001, "User Service" },
        { 7002, "Organization Service" },
        { 7003, "Vehicle Service" },
        { 7004, "Driver Service" },
        { 7005, "Product Service" },
        { 7006, "Route Service" },
        { 7007, "Weighbridge Service" },
        { 7008, "Customer Service" },
        { 7009, "Supplier Service" },
        { 7010, "Transporter Service" },
        { 7011, "Sacco Service" },
        { 7012, "Weight Data Service" },
        { 7013, "Compliance Service" },
        { 7014, "Operational Data Service" },
        { 7015, "Transaction Service" },
        { 7016, "Analytics Service" },
        { 7017, "Data Sync Service" },
        { 7018, "Archive Service" }
    };

    public ServiceAggregatorService(
        IHttpClientFactory httpClientFactory,
        ILogger<ServiceAggregatorService> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<ServiceDiscoveryResponse> GetAllServicesAsync()
    {
        var services = new List<ServiceInfo>();
        
        // Get Gateway services first
        var gatewayServices = await GetGatewayServicesAsync();
        services.AddRange(gatewayServices);

        // Discover direct services
        var directServices = await DiscoverDirectServicesAsync();
        services.AddRange(directServices);

        // Remove duplicates and merge information
        var mergedServices = MergeAndDeduplicateServices(services);

        var response = new ServiceDiscoveryResponse
        {
            Services = mergedServices.OrderBy(s => s.Category).ThenBy(s => s.Name).ToList(),
            Statistics = new ServiceStatistics
            {
                TotalServices = mergedServices.Count,
                TotalEndpoints = mergedServices.Sum(s => s.Endpoints.Count),
                HealthyServices = mergedServices.Count(s => s.IsHealthy),
                Categories = mergedServices.GroupBy(s => s.Category).Count()
            }
        };

        return response;
    }

    public async Task<string> GetServicesHtmlAsync()
    {
        var serviceDiscovery = await GetAllServicesAsync();
        return GenerateHtml(serviceDiscovery);
    }

    private async Task<List<ServiceInfo>> GetGatewayServicesAsync()
    {
        var services = new List<ServiceInfo>();
        var gatewayUrl = "http://localhost:7000";

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            // Check if gateway is healthy
            var healthResponse = await httpClient.GetAsync($"{gatewayUrl}/health");
            var isGatewayHealthy = healthResponse.IsSuccessStatusCode;

            // Add gateway service
            services.Add(new ServiceInfo
            {
                Name = "API Gateway",
                Category = "Infrastructure",
                BaseUrl = gatewayUrl,
                IsHealthy = isGatewayHealthy,
                Status = isGatewayHealthy ? "Healthy" : "Unhealthy",
                Endpoints = new List<EndpointInfo>
                {
                    new EndpointInfo { Path = "/api/gateway/info", Method = "GET", Description = "Gateway information" },
                    new EndpointInfo { Path = "/api/gateway/services", Method = "GET", Description = "Service discovery" },
                    new EndpointInfo { Path = "/health", Method = "GET", Description = "Gateway health check" },
                    new EndpointInfo { Path = "/swagger", Method = "GET", Description = "Aggregated API Documentation" }
                }
            });

            // Try to get services from gateway
            if (isGatewayHealthy)
            {
                try
                {
                    var servicesResponse = await httpClient.GetAsync($"{gatewayUrl}/api/gateway/services");
                    if (servicesResponse.IsSuccessStatusCode)
                    {
                        var content = await servicesResponse.Content.ReadAsStringAsync();
                        var gatewayData = JsonSerializer.Deserialize<GatewayServicesResponse>(content, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });

                        if (gatewayData?.Services != null)
                        {
                            foreach (var service in gatewayData.Services)
                            {
                                services.Add(new ServiceInfo
                                {
                                    Name = service.Name ?? "Unknown Service",
                                    Category = GetServiceCategory(service.Name),
                                    BaseUrl = gatewayUrl,
                                    IsHealthy = service.IsHealthy,
                                    Status = service.IsHealthy ? "Healthy" : "Unhealthy",
                                    Endpoints = GetGatewayRoutedEndpoints(service.Name, gatewayUrl)
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve services from gateway");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to gateway");
            
            // Add gateway as unhealthy
            services.Add(new ServiceInfo
            {
                Name = "API Gateway",
                Category = "Infrastructure",
                BaseUrl = gatewayUrl,
                IsHealthy = false,
                Status = "Unreachable",
                Endpoints = new List<EndpointInfo>()
            });
        }

        return services;
    }

    private async Task<List<ServiceInfo>> DiscoverDirectServicesAsync()
    {
        var services = new List<ServiceInfo>();
        var tasks = new List<Task<ServiceInfo?>>();

        // Check each known service port
        foreach (var kvp in _knownServices)
        {
            if (kvp.Key == 7000) continue; // Skip gateway, already handled
            
            tasks.Add(CheckServiceAsync(kvp.Key, kvp.Value));
        }

        var results = await Task.WhenAll(tasks);
        
        foreach (var service in results)
        {
            if (service != null)
            {
                services.Add(service);
            }
        }

        return services;
    }

    private async Task<ServiceInfo?> CheckServiceAsync(int port, string serviceName)
    {
        var baseUrl = $"http://localhost:{port}";
        
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            var healthResponse = await httpClient.GetAsync($"{baseUrl}/health");
            var isHealthy = healthResponse.IsSuccessStatusCode;

            return new ServiceInfo
            {
                Name = serviceName,
                Category = GetServiceCategory(serviceName),
                BaseUrl = baseUrl,
                IsHealthy = isHealthy,
                Status = isHealthy ? "Healthy" : "Unhealthy",
                Endpoints = GetServiceEndpoints(serviceName, baseUrl)
            };
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Service {ServiceName} on port {Port} is not reachable", serviceName, port);
            
            return new ServiceInfo
            {
                Name = serviceName,
                Category = GetServiceCategory(serviceName),
                BaseUrl = baseUrl,
                IsHealthy = false,
                Status = "Unreachable",
                Endpoints = new List<EndpointInfo>()
            };
        }
    }

    private List<ServiceInfo> MergeAndDeduplicateServices(List<ServiceInfo> services)
    {
        var merged = new Dictionary<string, ServiceInfo>();

        foreach (var service in services)
        {
            var key = service.Name;
            
            if (merged.ContainsKey(key))
            {
                // Merge information - prefer direct service info over gateway info
                var existing = merged[key];
                if (service.BaseUrl.Contains(":700") && !service.BaseUrl.Contains(":7000"))
                {
                    // This is a direct service, prefer it
                    existing.BaseUrl = service.BaseUrl;
                    existing.IsHealthy = service.IsHealthy;
                    existing.Status = service.Status;
                    
                    // Merge endpoints
                    var allEndpoints = existing.Endpoints.Concat(service.Endpoints)
                        .GroupBy(e => e.Path)
                        .Select(g => g.First())
                        .ToList();
                    existing.Endpoints = allEndpoints;
                }
            }
            else
            {
                merged[key] = service;
            }
        }

        return merged.Values.ToList();
    }

    private string GetServiceCategory(string? serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) return "Unknown";
        
        var lower = serviceName.ToLowerInvariant();
        
        return lower switch
        {
            var s when s.Contains("gateway") => "Infrastructure",
            var s when s.Contains("user") || s.Contains("auth") => "Authentication & Users",
            var s when s.Contains("organization") || s.Contains("customer") || s.Contains("supplier") || s.Contains("transporter") || s.Contains("sacco") => "Master Data",
            var s when s.Contains("vehicle") || s.Contains("driver") || s.Contains("product") || s.Contains("route") || s.Contains("weighbridge") => "Operational Master Data",
            var s when s.Contains("weight") || s.Contains("transaction") => "Transactional Data",
            var s when s.Contains("compliance") => "Compliance & Regulatory",
            var s when s.Contains("operational") => "Operational Management",
            var s when s.Contains("analytics") => "Analytics & Reporting",
            var s when s.Contains("sync") || s.Contains("archive") => "Data Management",
            _ => "Microservices"
        };
    }

    private List<EndpointInfo> GetServiceEndpoints(string serviceName, string baseUrl)
    {
        var endpoints = new List<EndpointInfo>
        {
            new EndpointInfo { Path = "/health", Method = "GET", Description = "Health check" },
            new EndpointInfo { Path = "/swagger", Method = "GET", Description = "API Documentation" }
        };

        // Add service-specific endpoints based on service name
        var lower = serviceName.ToLowerInvariant();
        
        if (lower.Contains("user"))
        {
            endpoints.AddRange(new[]
            {
                new EndpointInfo { Path = "/api/auth/login", Method = "POST", Description = "User login" },
                new EndpointInfo { Path = "/api/auth/register", Method = "POST", Description = "User registration" },
                new EndpointInfo { Path = "/api/users", Method = "GET", Description = "Get all users" },
                new EndpointInfo { Path = "/api/users/profile", Method = "GET", Description = "Get user profile" }
            });
        }
        else if (!lower.Contains("gateway"))
        {
            // Generic API endpoints for other services
            var apiPath = $"/api/{GetApiPath(serviceName)}";
            endpoints.AddRange(new[]
            {
                new EndpointInfo { Path = apiPath, Method = "GET", Description = $"Get {serviceName} data" },
                new EndpointInfo { Path = apiPath, Method = "POST", Description = $"Create {serviceName} data" }
            });
        }

        return endpoints;
    }

    private List<EndpointInfo> GetGatewayRoutedEndpoints(string? serviceName, string gatewayUrl)
    {
        if (string.IsNullOrEmpty(serviceName)) return new List<EndpointInfo>();

        var apiPath = $"/api/{GetApiPath(serviceName)}";
        
        return new List<EndpointInfo>
        {
            new EndpointInfo { Path = apiPath, Method = "GET", Description = $"{serviceName} API (via Gateway)" },
            new EndpointInfo { Path = $"/health", Method = "GET", Description = $"{serviceName} Health Check" }
        };
    }

    private string GetApiPath(string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName)) return "unknown";
        
        return serviceName.ToLowerInvariant() switch
        {
            var s when s.Contains("organization") => "organizations",
            var s when s.Contains("vehicle") => "vehicles", 
            var s when s.Contains("driver") => "drivers",
            var s when s.Contains("product") => "products",
            var s when s.Contains("route") => "routes",
            var s when s.Contains("weighbridge") => "weighbridges",
            var s when s.Contains("customer") => "customers",
            var s when s.Contains("supplier") => "suppliers",
            var s when s.Contains("transporter") => "transporters",
            var s when s.Contains("sacco") => "saccos",
            var s when s.Contains("weight") => "weight-data",
            var s when s.Contains("compliance") => "compliance",
            var s when s.Contains("operational") => "operational-data",
            var s when s.Contains("transaction") => "transactions",
            var s when s.Contains("analytics") => "analytics",
            var s when s.Contains("sync") => "sync",
            var s when s.Contains("archive") => "archive",
            var s when s.Contains("user") => "users",
            _ => serviceName.ToLowerInvariant().Replace(" ", "-")
        };
    }

    private string GenerateHtml(ServiceDiscoveryResponse serviceDiscovery)
    {
        var services = serviceDiscovery.Services;
        var stats = serviceDiscovery.Statistics;

        var html = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>QaliTrack Services Discovery</title>
    <style>
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', system-ui, sans-serif; margin: 0; padding: 20px; background: #f5f5f5; }}
        .container {{ max-width: 1400px; margin: 0 auto; }}
        .header {{ background: white; padding: 30px; border-radius: 12px; margin-bottom: 20px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header h1 {{ margin: 0; color: #2563eb; font-size: 28px; }}
        .header p {{ margin: 10px 0 0 0; color: #64748b; font-size: 16px; }}
        .last-updated {{ color: #94a3b8; font-size: 14px; margin-top: 10px; }}
        
        .stats {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 20px; }}
        .stat {{ background: white; padding: 20px; border-radius: 8px; text-align: center; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .stat-number {{ font-size: 32px; font-weight: 700; color: #2563eb; }}
        .stat-label {{ color: #64748b; font-size: 14px; margin-top: 5px; }}
        
        .category-section {{ margin-bottom: 30px; }}
        .category-title {{ font-size: 20px; font-weight: 600; color: #1e293b; margin-bottom: 15px; padding-left: 5px; }}
        .service-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(450px, 1fr)); gap: 20px; }}
        .service-card {{ background: white; border-radius: 8px; padding: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); border-left: 4px solid #e2e8f0; }}
        .service-card.healthy {{ border-left-color: #10b981; }}
        .service-card.unhealthy {{ border-left-color: #ef4444; }}
        .service-card.unreachable {{ border-left-color: #f59e0b; }}
        
        .service-header {{ display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px; }}
        .service-title {{ font-size: 18px; font-weight: 600; color: #1e293b; }}
        .service-status {{ padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600; }}
        .status-healthy {{ background: #dcfce7; color: #166534; }}
        .status-unhealthy {{ background: #fecaca; color: #991b1b; }}
        .status-unreachable {{ background: #fef3c7; color: #92400e; }}
        
        .service-url {{ color: #64748b; font-size: 14px; margin-bottom: 15px; font-family: 'Monaco', 'Consolas', monospace; }}
        
        .endpoint {{ display: flex; justify-content: space-between; align-items: center; padding: 8px 0; border-bottom: 1px solid #f1f5f9; }}
        .endpoint:last-child {{ border-bottom: none; }}
        .endpoint-left {{ display: flex; align-items: center; gap: 8px; }}
        .endpoint-method {{ padding: 2px 6px; border-radius: 4px; font-size: 11px; font-weight: 600; min-width: 40px; text-align: center; }}
        .method-get {{ background: #dcfce7; color: #166534; }}
        .method-post {{ background: #fef3c7; color: #92400e; }}
        .method-put {{ background: #dbeafe; color: #1d4ed8; }}
        .method-delete {{ background: #fecaca; color: #991b1b; }}
        .endpoint-path {{ font-family: 'Monaco', 'Consolas', monospace; font-size: 13px; color: #374151; }}
        .endpoint-url {{ color: #2563eb; text-decoration: none; }}
        .endpoint-url:hover {{ text-decoration: underline; }}
        .endpoint-description {{ color: #64748b; font-size: 12px; }}
        
        .refresh-btn {{ position: fixed; top: 20px; right: 20px; background: #2563eb; color: white; border: none; padding: 10px 16px; border-radius: 6px; cursor: pointer; font-size: 14px; }}
        .refresh-btn:hover {{ background: #1d4ed8; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🚀 QaliTrack Services Discovery</h1>
            <p>Complete overview of all microservices and API endpoints</p>
            <div class='last-updated'>Last updated: {serviceDiscovery.LastUpdated:yyyy-MM-dd HH:mm:ss} UTC</div>
        </div>
        
        <div class='stats'>
            <div class='stat'>
                <div class='stat-number'>{stats.TotalServices}</div>
                <div class='stat-label'>Total Services</div>
            </div>
            <div class='stat'>
                <div class='stat-number'>{stats.HealthyServices}</div>
                <div class='stat-label'>Healthy Services</div>
            </div>
            <div class='stat'>
                <div class='stat-number'>{stats.TotalEndpoints}</div>
                <div class='stat-label'>API Endpoints</div>
            </div>
            <div class='stat'>
                <div class='stat-number'>{stats.Categories}</div>
                <div class='stat-label'>Categories</div>
            </div>
        </div>";

        // Group services by category
        var groupedServices = services.GroupBy(s => s.Category).OrderBy(g => g.Key);

        foreach (var group in groupedServices)
        {
            html += $@"
        <div class='category-section'>
            <h2 class='category-title'>📂 {group.Key}</h2>
            <div class='service-grid'>";

            foreach (var service in group.OrderBy(s => s.Name))
            {
                var statusClass = service.Status?.ToLowerInvariant() switch
                {
                    "healthy" => "healthy",
                    "unhealthy" => "unhealthy",
                    _ => "unreachable"
                };

                var statusCssClass = service.Status?.ToLowerInvariant() switch
                {
                    "healthy" => "status-healthy",
                    "unhealthy" => "status-unhealthy",
                    _ => "status-unreachable"
                };

                html += $@"
                <div class='service-card {statusClass}'>
                    <div class='service-header'>
                        <div class='service-title'>{service.Name}</div>
                        <div class='service-status {statusCssClass}'>{service.Status}</div>
                    </div>
                    <div class='service-url'>{service.BaseUrl}</div>";

                foreach (var endpoint in service.Endpoints)
                {
                    var methodClass = $"method-{endpoint.Method.ToLowerInvariant()}";
                    var fullUrl = $"{service.BaseUrl}{endpoint.Path}";
                    
                    html += $@"
                    <div class='endpoint'>
                        <div class='endpoint-left'>
                            <span class='endpoint-method {methodClass}'>{endpoint.Method}</span>
                            <a href='{fullUrl}' target='_blank' class='endpoint-url endpoint-path'>{endpoint.Path}</a>
                        </div>
                        <div class='endpoint-description'>{endpoint.Description}</div>
                    </div>";
                }

                html += "</div>";
            }

            html += "</div></div>";
        }

        html += @"
    </div>
    <button class='refresh-btn' onclick='window.location.reload()'>🔄 Refresh</button>
    
    <script>
        // Auto-refresh every 30 seconds
        setTimeout(() => window.location.reload(), 30000);
    </script>
</body>
</html>";

        return html;
    }
}