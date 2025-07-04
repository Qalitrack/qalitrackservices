using Microsoft.AspNetCore.Mvc;

namespace QaliTrack.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GatewayController : ControllerBase
{
    private readonly ILogger<GatewayController> _logger;

    public GatewayController(ILogger<GatewayController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get gateway information and available services
    /// </summary>
    [HttpGet("info")]
    public IActionResult GetGatewayInfo()
    {
        var gatewayInfo = new
        {
            Name = "QaliTrack API Gateway",
            Version = "1.0.0",
            Description = "Central entry point for all QaliTrack microservices",
            Services = new[]
            {
                new { Name = "User Service", Path = "/api/users", Port = 7001, Description = "Authentication and user management" },
                new { Name = "Authentication", Path = "/api/auth", Port = 7001, Description = "Authentication endpoints" },
                new { Name = "Organization Service", Path = "/api/organizations", Port = 7002, Description = "Organization management" },
                new { Name = "Vehicle Service", Path = "/api/vehicles", Port = 7003, Description = "Vehicle master data" },
                new { Name = "Driver Service", Path = "/api/drivers", Port = 7004, Description = "Driver master data" },
                new { Name = "Product Service", Path = "/api/products", Port = 7005, Description = "Product master data" },
                new { Name = "Route Service", Path = "/api/routes", Port = 7006, Description = "Route master data" },
                new { Name = "Weighbridge Service", Path = "/api/weighbridges", Port = 7007, Description = "Weighbridge master data" },
                new { Name = "Customer Service", Path = "/api/customers", Port = 7008, Description = "Customer master data" },
                new { Name = "Supplier Service", Path = "/api/suppliers", Port = 7009, Description = "Supplier master data" },
                new { Name = "Transporter Service", Path = "/api/transporters", Port = 7010, Description = "Transporter master data" },
                new { Name = "Sacco Service", Path = "/api/saccos", Port = 7011, Description = "Sacco master data" },
                new { Name = "Weight Data Service", Path = "/api/weight-data", Port = 7012, Description = "Weight measurements" },
                new { Name = "Compliance Service", Path = "/api/compliance", Port = 7013, Description = "Compliance monitoring" },
                new { Name = "Operational Data Service", Path = "/api/operational-data", Port = 7014, Description = "Operational data management" },
                new { Name = "Transaction Service", Path = "/api/transactions", Port = 7015, Description = "Transaction processing" },
                new { Name = "Analytics Service", Path = "/api/analytics", Port = 7016, Description = "Analytics and reporting" },
                new { Name = "Data Sync Service", Path = "/api/data-sync", Port = 7017, Description = "Data synchronization" },
                new { Name = "Archive Service", Path = "/api/archive", Port = 7018, Description = "Data archival" }
            },
            HealthChecks = new
            {
                Gateway = "/health",
                Services = "/services/{service-name}/health"
            },
            Documentation = "/swagger"
        };

        return Ok(gatewayInfo);
    }

    /// <summary>
    /// Get service discovery information
    /// </summary>
    [HttpGet("services")]
    public IActionResult GetServices()
    {
        var services = new[]
        {
            new { 
                Name = "user-service", 
                Url = "https://localhost:7001", 
                Health = "/services/users/health",
                Status = "Available",
                Description = "Authentication and user management service"
            },
            new { 
                Name = "organization-service", 
                Url = "https://localhost:7002", 
                Health = "/services/organizations/health",
                Status = "Available",
                Description = "Organization management service"
            },
            new { 
                Name = "vehicle-service", 
                Url = "https://localhost:7003", 
                Health = "/services/vehicles/health",
                Status = "Available",
                Description = "Vehicle master data service"
            },
            new { 
                Name = "driver-service", 
                Url = "https://localhost:7004", 
                Health = "/services/drivers/health",
                Status = "Available",
                Description = "Driver master data service"
            },
            new { 
                Name = "product-service", 
                Url = "https://localhost:7005", 
                Health = "/services/products/health",
                Status = "Available",
                Description = "Product master data service"
            },
            new { 
                Name = "route-service", 
                Url = "https://localhost:7006", 
                Health = "/services/routes/health",
                Status = "Available",
                Description = "Route master data service"
            },
            new { 
                Name = "weighbridge-service", 
                Url = "https://localhost:7007", 
                Health = "/services/weighbridges/health",
                Status = "Available",
                Description = "Weighbridge master data service"
            },
            new { 
                Name = "customer-service", 
                Url = "https://localhost:7008", 
                Health = "/services/customers/health",
                Status = "Available",
                Description = "Customer master data service"
            },
            new { 
                Name = "supplier-service", 
                Url = "https://localhost:7009", 
                Health = "/services/suppliers/health",
                Status = "Available",
                Description = "Supplier master data service"
            },
            new { 
                Name = "transporter-service", 
                Url = "https://localhost:7010", 
                Health = "/services/transporters/health",
                Status = "Available",
                Description = "Transporter master data service"
            },
            new { 
                Name = "sacco-service", 
                Url = "https://localhost:7011", 
                Health = "/services/saccos/health",
                Status = "Available",
                Description = "Sacco master data service"
            },
            new { 
                Name = "weight-data-service", 
                Url = "https://localhost:7012", 
                Health = "/services/weight-data/health",
                Status = "Available",
                Description = "Weight measurement data service"
            },
            new { 
                Name = "compliance-service", 
                Url = "https://localhost:7013", 
                Health = "/services/compliance/health",
                Status = "Available",
                Description = "Compliance monitoring service"
            },
            new { 
                Name = "operational-data-service", 
                Url = "https://localhost:7014", 
                Health = "/services/operational-data/health",
                Status = "Available",
                Description = "Operational data management service"
            },
            new { 
                Name = "transaction-service", 
                Url = "https://localhost:7015", 
                Health = "/services/transactions/health",
                Status = "Available",
                Description = "Transaction processing service"
            },
            new { 
                Name = "analytics-service", 
                Url = "https://localhost:7016", 
                Health = "/services/analytics/health",
                Status = "Available",
                Description = "Analytics and reporting service"
            },
            new { 
                Name = "data-sync-service", 
                Url = "https://localhost:7017", 
                Health = "/services/data-sync/health",
                Status = "Available",
                Description = "Data synchronization service"
            },
            new { 
                Name = "archive-service", 
                Url = "https://localhost:7018", 
                Health = "/services/archive/health",
                Status = "Available",
                Description = "Data archival service"
            }
        };

        return Ok(services);
    }
}