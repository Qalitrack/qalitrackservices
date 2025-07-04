using QaliTrack.Gateway.Models;
using YamlDotNet.Serialization;

namespace QaliTrack.Gateway.Services;

public interface IConfigurationService
{
    ClientConfiguration GetClientConfiguration();
    List<SwaggerEndpoint> GetEnabledServices();
    Task<bool> IsServiceHealthyAsync(string serviceName, int port);
}

public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ConfigurationService> _logger;
    private ClientConfiguration? _clientConfig;

    public ConfigurationService(
        IConfiguration configuration, 
        IHttpClientFactory httpClientFactory,
        ILogger<ConfigurationService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        LoadConfiguration();
    }

    public ClientConfiguration GetClientConfiguration()
    {
        return _clientConfig ?? new ClientConfiguration();
    }

    public List<SwaggerEndpoint> GetEnabledServices()
    {
        var config = GetClientConfiguration();
        var endpoints = new List<SwaggerEndpoint>();

        // Add gateway itself
        endpoints.Add(new SwaggerEndpoint
        {
            Name = "API Gateway",
            Url = "/swagger/v1/swagger.json",
            Description = "QaliTrack API Gateway - Central routing and authentication",
            Version = "v1",
            Available = true
        });

        // Add enabled services
        foreach (var service in config.Services.Where(s => s.Value.Enabled))
        {
            var endpoint = new SwaggerEndpoint
            {
                Name = FormatServiceName(service.Key),
                Url = GetSwaggerUrl(service.Key, service.Value.Port),
                Description = GetServiceDescription(service.Key),
                Version = "v1"
            };

            endpoints.Add(endpoint);
        }

        return endpoints;
    }

    public async Task<bool> IsServiceHealthyAsync(string serviceName, int port)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            
            var healthUrl = GetHealthUrl(serviceName, port);
            var response = await client.GetAsync(healthUrl);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for service {ServiceName} on port {Port}", serviceName, port);
            return false;
        }
    }

    private void LoadConfiguration()
    {
        try
        {
            var clientCode = _configuration["CLIENT_CODE"] ?? "testing";
            var configPath = Path.Combine("configs", "clients", $"{clientCode}.yml");

            if (!File.Exists(configPath))
            {
                _logger.LogWarning("Configuration file not found: {ConfigPath}. Using default testing configuration.", configPath);
                configPath = Path.Combine("configs", "clients", "testing.yml");
            }

            if (!File.Exists(configPath))
            {
                _logger.LogWarning("No configuration files found. Using minimal default configuration.");
                _clientConfig = CreateDefaultConfiguration();
                return;
            }

            var yaml = File.ReadAllText(configPath);
            var deserializer = new DeserializerBuilder().Build();
            _clientConfig = deserializer.Deserialize<ClientConfiguration>(yaml);

            _logger.LogInformation("Loaded configuration for client: {ClientName}", _clientConfig?.Client?.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load client configuration. Using default.");
            _clientConfig = CreateDefaultConfiguration();
        }
    }

    private ClientConfiguration CreateDefaultConfiguration()
    {
        return new ClientConfiguration
        {
            Client = new ClientInfo
            {
                Name = "Default Testing",
                Code = "testing",
                Description = "Default testing configuration"
            },
            Deployment = new DeploymentInfo
            {
                Environment = "Development",
                Domain = "localhost",
                Ports = new PortConfiguration
                {
                    Gateway = 7000,
                    SwaggerAggregator = 8000
                }
            },
            Services = new Dictionary<string, ServiceInfo>
            {
                ["user-service"] = new ServiceInfo { Enabled = true, Port = 7001 }
            }
        };
    }

    private string GetSwaggerUrl(string serviceName, int port)
    {
        var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
        var host = isDevelopment ? "localhost" : serviceName.Replace("-", "");
        
        return $"http://{host}:{port}/swagger/v1/swagger.json";
    }

    private string GetHealthUrl(string serviceName, int port)
    {
        var isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
        var host = isDevelopment ? "localhost" : serviceName.Replace("-", "");
        
        return $"http://{host}:{port}/health";
    }

    private string FormatServiceName(string serviceName)
    {
        return serviceName.Replace("-", " ")
            .Split(' ')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower())
            .Aggregate((current, next) => current + " " + next);
    }

    private string GetServiceDescription(string serviceName)
    {
        return serviceName switch
        {
            "user-service" => "User Service - Authentication and user management",
            "organization-service" => "Organization Service - Multi-tenant organization management",
            "vehicle-service" => "Vehicle Service - Vehicle master data management",
            "driver-service" => "Driver Service - Driver master data management",
            "product-service" => "Product Service - Product catalog and specifications",
            "route-service" => "Route Service - Transportation route management",
            "weighbridge-service" => "Weighbridge Service - Weighbridge configuration and management",
            "customer-service" => "Customer Service - Customer relationship management",
            "supplier-service" => "Supplier Service - Supplier management and procurement",
            "transporter-service" => "Transporter Service - Transportation provider management",
            "sacco-service" => "Sacco Service - Savings and credit cooperative management",
            "weight-data-service" => "Weight Data Service - Weight measurement data management",
            "compliance-service" => "Compliance Service - Regulatory compliance monitoring",
            "operational-data-service" => "Operational Data Service - Operational metrics and KPIs",
            "transaction-service" => "Transaction Service - Business transaction processing",
            "analytics-service" => "Analytics Service - Business intelligence and reporting",
            "data-sync-service" => "Data Sync Service - Multi-site data synchronization",
            "archive-service" => "Archive Service - Data archival and retention",
            _ => $"{FormatServiceName(serviceName)} - QaliTrack microservice"
        };
    }
}