using Microsoft.AspNetCore.Mvc;
using QaliTrack.Gateway.Services;
using Newtonsoft.Json;

namespace QaliTrack.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SwaggerController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SwaggerController> _logger;

    public SwaggerController(
        IConfigurationService configurationService,
        IHttpClientFactory httpClientFactory,
        ILogger<SwaggerController> logger)
    {
        _configurationService = configurationService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get aggregated Swagger documentation for all enabled services
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> GetAggregatedSwagger()
    {
        try
        {
            var services = _configurationService.GetEnabledServices();
            var aggregatedSwagger = await BuildAggregatedSwaggerAsync(services);
            
            return Content(JsonConvert.SerializeObject(aggregatedSwagger, Formatting.Indented), 
                          "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate aggregated Swagger documentation");
            return StatusCode(500, "Failed to generate Swagger documentation");
        }
    }

    /// <summary>
    /// Get list of available services and their Swagger endpoints
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var services = _configurationService.GetEnabledServices();
        
        // Check health status for each service (except gateway itself)
        foreach (var service in services.Where(s => s.Name != "API Gateway"))
        {
            var serviceName = service.Name.Replace(" ", "-").ToLower();
            var config = _configurationService.GetClientConfiguration();
            
            if (config.Services.TryGetValue(serviceName, out var serviceConfig))
            {
                service.Available = await _configurationService.IsServiceHealthyAsync(serviceName, serviceConfig.Port);
            }
        }
        
        return Ok(services);
    }

    /// <summary>
    /// Get Swagger documentation for a specific service
    /// </summary>
    [HttpGet("services/{serviceName}")]
    public async Task<IActionResult> GetServiceSwagger(string serviceName)
    {
        try
        {
            if (serviceName.Equals("gateway", StringComparison.OrdinalIgnoreCase) ||
                serviceName.Equals("api-gateway", StringComparison.OrdinalIgnoreCase))
            {
                // Return gateway's own swagger
                var request = HttpContext.Request;
                var gatewaySwaggerUrl = $"{request.Scheme}://{request.Host}/swagger/v1/swagger.json";
                
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync(gatewaySwaggerUrl);
                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }

            var services = _configurationService.GetEnabledServices();
            var service = services.FirstOrDefault(s => 
                s.Name.Replace(" ", "-").Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            if (service == null)
            {
                return NotFound($"Service '{serviceName}' not found or not enabled");
            }

            var httpClient = _httpClientFactory.CreateClient();
            var serviceResponse = await httpClient.GetAsync(service.Url);

            if (!serviceResponse.IsSuccessStatusCode)
            {
                return StatusCode((int)serviceResponse.StatusCode, 
                    $"Failed to fetch Swagger documentation for {serviceName}");
            }

            var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
            return Content(serviceContent, "application/json");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Swagger documentation for service {ServiceName}", serviceName);
            return StatusCode(500, $"Failed to fetch Swagger documentation for {serviceName}");
        }
    }

    private async Task<object> BuildAggregatedSwaggerAsync(List<Models.SwaggerEndpoint> services)
    {
        var clientConfig = _configurationService.GetClientConfiguration();
        
        var aggregatedSwagger = new
        {
            openapi = "3.0.1",
            info = new
            {
                title = $"{clientConfig.Client.Name} - QaliTrack API",
                description = $"Aggregated API documentation for {clientConfig.Client.Description}",
                version = "1.0.0",
                contact = new
                {
                    name = "QaliTrack Support",
                    email = "support@qalitrack.com"
                }
            },
            servers = new[]
            {
                new
                {
                    url = $"http://{clientConfig.Deployment.Domain}:{clientConfig.Deployment.Ports.Gateway}",
                    description = $"{clientConfig.Client.Name} API Gateway"
                }
            },
            paths = new Dictionary<string, object>(),
            components = new
            {
                schemas = new Dictionary<string, object>(),
                securitySchemes = new
                {
                    Bearer = new
                    {
                        type = "http",
                        scheme = "bearer",
                        bearerFormat = "JWT",
                        description = "JWT Authorization header using the Bearer scheme"
                    }
                }
            },
            tags = services.Where(s => s.Available).Select(s => new
            {
                name = s.Name,
                description = s.Description
            }).ToArray(),
            externalDocs = new
            {
                description = "QaliTrack Documentation",
                url = "https://docs.qalitrack.com"
            }
        };

        // Fetch and merge individual service Swagger docs
        foreach (var service in services.Where(s => s.Available))
        {
            try
            {
                await MergeServiceSwaggerAsync(service, aggregatedSwagger);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to merge Swagger documentation for service {ServiceName}", service.Name);
            }
        }

        return aggregatedSwagger;
    }

    private async Task MergeServiceSwaggerAsync(Models.SwaggerEndpoint service, dynamic aggregatedSwagger)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            
            string content;
            if (service.Name == "API Gateway")
            {
                // Get gateway's own swagger
                var request = HttpContext.Request;
                var gatewaySwaggerUrl = $"{request.Scheme}://{request.Host}/swagger/v1/swagger.json";
                var response = await client.GetAsync(gatewaySwaggerUrl);
                content = await response.Content.ReadAsStringAsync();
            }
            else
            {
                var response = await client.GetAsync(service.Url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch Swagger for {ServiceName}: {StatusCode}", 
                        service.Name, response.StatusCode);
                    return;
                }
                content = await response.Content.ReadAsStringAsync();
            }

            var serviceSwagger = JsonConvert.DeserializeObject<dynamic>(content);

            if (serviceSwagger?.paths != null)
            {
                foreach (var path in serviceSwagger.paths)
                {
                    // Add service tag to all operations
                    if (path.Value != null)
                    {
                        foreach (var operation in path.Value)
                        {
                            if (operation.Value?.tags == null)
                            {
                                operation.Value.tags = new[] { service.Name };
                            }
                        }
                    }

                    aggregatedSwagger.paths[path.Name] = path.Value;
                }
            }

            // Merge schemas with service prefix
            if (serviceSwagger?.components?.schemas != null)
            {
                foreach (var schema in serviceSwagger.components.schemas)
                {
                    var prefixedSchema = $"{service.Name.Replace(" ", "")}{schema.Name}";
                    aggregatedSwagger.components.schemas[prefixedSchema] = schema.Value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error merging Swagger documentation for service {ServiceName}", service.Name);
        }
    }
}