using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QaliTrackGateway.Services;
using QaliTrackGateway.Models;

namespace QaliTrackGateway.Controllers;

/// <summary>
/// Service monitoring and health dashboard controller
/// </summary>
[ApiController]
[Route("api/monitoring")]
[Authorize(Roles = "Admin,SuperAdmin,SiteManager")]
public class MonitoringController : ControllerBase
{
    private readonly IServiceHealthMonitor _healthMonitor;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        IServiceHealthMonitor healthMonitor,
        ILogger<MonitoringController> logger)
    {
        _healthMonitor = healthMonitor;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time health status of all services
    /// </summary>
    [HttpGet("services/health")]
    public async Task<ActionResult<IEnumerable<ServiceHealthStatus>>> GetAllServicesHealth()
    {
        try
        {
            var healthStatuses = await _healthMonitor.GetAllServicesHealthAsync();
            return Ok(healthStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all services health");
            return StatusCode(500, new { error = "Failed to retrieve services health" });
        }
    }

    /// <summary>
    /// Get health status of a specific service
    /// </summary>
    [HttpGet("services/{serviceName}/health")]
    public async Task<ActionResult<ServiceHealthStatus>> GetServiceHealth(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            var healthStatus = await _healthMonitor.GetServiceHealthAsync(serviceName);
            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service health for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve service health" });
        }
    }

    /// <summary>
    /// Get health history for a service
    /// </summary>
    [HttpGet("services/{serviceName}/health/history")]
    public async Task<ActionResult<IEnumerable<ServiceHealthCheck>>> GetServiceHealthHistory(
        string serviceName, 
        [FromQuery] int hours = 24)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (hours < 1 || hours > 168) // Max 7 days
            {
                return BadRequest(new { error = "Hours must be between 1 and 168" });
            }

            var healthHistory = await _healthMonitor.GetServiceHealthHistoryAsync(serviceName, hours);
            return Ok(healthHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service health history for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve service health history" });
        }
    }

    /// <summary>
    /// Get overall system health score
    /// </summary>
    [HttpGet("system/health")]
    public async Task<ActionResult<SystemHealthScore>> GetSystemHealthScore()
    {
        try
        {
            var systemHealth = await _healthMonitor.GetSystemHealthScoreAsync();
            return Ok(systemHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system health score");
            return StatusCode(500, new { error = "Failed to retrieve system health score" });
        }
    }

    /// <summary>
    /// Get service dependency map
    /// </summary>
    [HttpGet("services/dependencies")]
    public async Task<ActionResult<ServiceDependencyMap>> GetServiceDependencyMap()
    {
        try
        {
            var dependencyMap = await _healthMonitor.GetServiceDependencyMapAsync();
            return Ok(dependencyMap);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service dependency map");
            return StatusCode(500, new { error = "Failed to retrieve service dependency map" });
        }
    }

    /// <summary>
    /// Get unhealthy services for alerting
    /// </summary>
    [HttpGet("services/unhealthy")]
    public async Task<ActionResult<IEnumerable<ServiceHealthStatus>>> GetUnhealthyServices()
    {
        try
        {
            var unhealthyServices = await _healthMonitor.GetUnhealthyServicesAsync();
            return Ok(unhealthyServices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unhealthy services");
            return StatusCode(500, new { error = "Failed to retrieve unhealthy services" });
        }
    }

    /// <summary>
    /// Get service performance metrics
    /// </summary>
    [HttpGet("services/{serviceName}/performance")]
    public async Task<ActionResult<ServicePerformanceMetrics>> GetServicePerformance(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            var performance = await _healthMonitor.GetServicePerformanceAsync(serviceName);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service performance for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve service performance" });
        }
    }

    /// <summary>
    /// Register a new service for monitoring
    /// </summary>
    [HttpPost("services")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> RegisterService([FromBody] ServiceRegistration serviceRegistration)
    {
        try
        {
            if (serviceRegistration == null)
            {
                return BadRequest(new { error = "Service registration is required" });
            }

            if (string.IsNullOrWhiteSpace(serviceRegistration.ServiceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (string.IsNullOrWhiteSpace(serviceRegistration.HealthCheckUrl))
            {
                return BadRequest(new { error = "Health check URL is required" });
            }

            await _healthMonitor.RegisterServiceAsync(serviceRegistration);
            
            _logger.LogInformation("Service {ServiceName} registered by {UserId}", 
                serviceRegistration.ServiceName, User.FindFirst("sub")?.Value);

            return Ok(new { message = $"Service {serviceRegistration.ServiceName} registered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service {ServiceName}", serviceRegistration?.ServiceName);
            return StatusCode(500, new { error = "Failed to register service" });
        }
    }

    /// <summary>
    /// Unregister a service from monitoring
    /// </summary>
    [HttpDelete("services/{serviceName}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> UnregisterService(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            await _healthMonitor.UnregisterServiceAsync(serviceName);
            
            _logger.LogInformation("Service {ServiceName} unregistered by {UserId}", 
                serviceName, User.FindFirst("sub")?.Value);

            return Ok(new { message = $"Service {serviceName} unregistered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering service {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to unregister service" });
        }
    }

    /// <summary>
    /// Start monitoring all services
    /// </summary>
    [HttpPost("start")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> StartMonitoring()
    {
        try
        {
            await _healthMonitor.StartMonitoringAsync();
            
            _logger.LogInformation("Service monitoring started by {UserId}", User.FindFirst("sub")?.Value);

            return Ok(new { message = "Service monitoring started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting service monitoring");
            return StatusCode(500, new { error = "Failed to start service monitoring" });
        }
    }

    /// <summary>
    /// Stop monitoring all services
    /// </summary>
    [HttpPost("stop")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult> StopMonitoring()
    {
        try
        {
            await _healthMonitor.StopMonitoringAsync();
            
            _logger.LogInformation("Service monitoring stopped by {UserId}", User.FindFirst("sub")?.Value);

            return Ok(new { message = "Service monitoring stopped successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping service monitoring");
            return StatusCode(500, new { error = "Failed to stop service monitoring" });
        }
    }

    /// <summary>
    /// Get monitoring dashboard summary
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboard()
    {
        try
        {
            var systemHealth = await _healthMonitor.GetSystemHealthScoreAsync();
            var allServices = await _healthMonitor.GetAllServicesHealthAsync();
            var unhealthyServices = await _healthMonitor.GetUnhealthyServicesAsync();

            var dashboard = new
            {
                SystemHealth = systemHealth,
                ServicesSummary = new
                {
                    Total = allServices.Count(),
                    Healthy = allServices.Count(s => s.Status == "Healthy"),
                    Unhealthy = allServices.Count(s => s.Status == "Unhealthy"),
                    Degraded = allServices.Count(s => s.Status == "Degraded"),
                    Unknown = allServices.Count(s => s.Status == "Unknown")
                },
                UnhealthyServices = unhealthyServices.Select(s => new
                {
                    s.ServiceName,
                    s.Status,
                    s.ErrorMessage,
                    s.ConsecutiveFailures,
                    s.LastChecked
                }),
                LastUpdated = DateTime.UtcNow
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring dashboard");
            return StatusCode(500, new { error = "Failed to retrieve monitoring dashboard" });
        }
    }

    /// <summary>
    /// Get service metrics for a specific time period
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<object>> GetMetrics([FromQuery] string? serviceName = null, [FromQuery] int hours = 24)
    {
        try
        {
            if (hours < 1 || hours > 168) // Max 7 days
            {
                return BadRequest(new { error = "Hours must be between 1 and 168" });
            }

            var metrics = new Dictionary<string, object>();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                // Get metrics for all services
                var allServices = await _healthMonitor.GetAllServicesHealthAsync();
                foreach (var service in allServices)
                {
                    var performance = await _healthMonitor.GetServicePerformanceAsync(service.ServiceName);
                    metrics[service.ServiceName] = performance;
                }
            }
            else
            {
                // Get metrics for specific service
                var performance = await _healthMonitor.GetServicePerformanceAsync(serviceName);
                metrics[serviceName] = performance;
            }

            return Ok(new
            {
                Metrics = metrics,
                PeriodHours = hours,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service metrics");
            return StatusCode(500, new { error = "Failed to retrieve service metrics" });
        }
    }
}