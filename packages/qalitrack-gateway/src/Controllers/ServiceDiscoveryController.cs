using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QaliTrackGateway.Services;
using QaliTrackGateway.Models;

namespace QaliTrackGateway.Controllers;

/// <summary>
/// Service discovery and health-aware routing controller
/// </summary>
[ApiController]
[Route("api/discovery")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ServiceDiscoveryController : ControllerBase
{
    private readonly IHealthAwareRoutingService _routingService;
    private readonly IServiceHealthMonitor _healthMonitor;
    private readonly ILogger<ServiceDiscoveryController> _logger;

    public ServiceDiscoveryController(
        IHealthAwareRoutingService routingService,
        IServiceHealthMonitor healthMonitor,
        ILogger<ServiceDiscoveryController> logger)
    {
        _routingService = routingService;
        _healthMonitor = healthMonitor;
        _logger = logger;
    }

    /// <summary>
    /// Get service discovery information
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ServiceDiscoveryInfo>> GetServiceDiscoveryInfo()
    {
        try
        {
            var discoveryInfo = await _routingService.GetServiceDiscoveryInfoAsync();
            return Ok(discoveryInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service discovery info");
            return StatusCode(500, new { error = "Failed to retrieve service discovery info" });
        }
    }

    /// <summary>
    /// Get all service instances for a specific service
    /// </summary>
    [HttpGet("services/{serviceName}/instances")]
    public async Task<ActionResult<IEnumerable<ServiceInstance>>> GetServiceInstances(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            var instances = await _routingService.GetServiceInstancesAsync(serviceName);
            return Ok(instances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service instances for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve service instances" });
        }
    }

    /// <summary>
    /// Get best available service instance for routing
    /// </summary>
    [HttpGet("services/{serviceName}/best-instance")]
    public async Task<ActionResult<ServiceInstance>> GetBestServiceInstance(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            var instance = await _routingService.GetBestServiceInstanceAsync(serviceName);
            if (instance == null)
            {
                return NotFound(new { error = $"No available instances found for service {serviceName}" });
            }

            return Ok(instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best service instance for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve best service instance" });
        }
    }

    /// <summary>
    /// Register a new service instance
    /// </summary>
    [HttpPost("services/{serviceName}/instances")]
    public async Task<ActionResult> RegisterServiceInstance(string serviceName, [FromBody] ServiceInstance serviceInstance)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (serviceInstance == null)
            {
                return BadRequest(new { error = "Service instance is required" });
            }

            if (string.IsNullOrWhiteSpace(serviceInstance.Host))
            {
                return BadRequest(new { error = "Host is required" });
            }

            if (serviceInstance.Port <= 0)
            {
                return BadRequest(new { error = "Valid port is required" });
            }

            // Ensure service name matches
            serviceInstance.ServiceName = serviceName;

            await _routingService.RegisterServiceInstanceAsync(serviceInstance);
            
            _logger.LogInformation("Service instance {InstanceId} registered for {ServiceName} by {UserId}", 
                serviceInstance.InstanceId, serviceName, User.FindFirst("sub")?.Value);

            return Ok(new { message = $"Service instance registered successfully", instanceId = serviceInstance.InstanceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service instance for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to register service instance" });
        }
    }

    /// <summary>
    /// Deregister a service instance
    /// </summary>
    [HttpDelete("services/{serviceName}/instances/{instanceId}")]
    public async Task<ActionResult> DeregisterServiceInstance(string serviceName, string instanceId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return BadRequest(new { error = "Instance ID is required" });
            }

            await _routingService.DeregisterServiceInstanceAsync(serviceName, instanceId);
            
            _logger.LogInformation("Service instance {InstanceId} deregistered from {ServiceName} by {UserId}", 
                instanceId, serviceName, User.FindFirst("sub")?.Value);

            return Ok(new { message = "Service instance deregistered successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deregistering service instance {InstanceId} from {ServiceName}", instanceId, serviceName);
            return StatusCode(500, new { error = "Failed to deregister service instance" });
        }
    }

    /// <summary>
    /// Update service instance health status
    /// </summary>
    [HttpPut("services/{serviceName}/instances/{instanceId}/health")]
    public async Task<ActionResult> UpdateServiceInstanceHealth(string serviceName, string instanceId, [FromBody] HealthUpdateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (string.IsNullOrWhiteSpace(instanceId))
            {
                return BadRequest(new { error = "Instance ID is required" });
            }

            if (request == null)
            {
                return BadRequest(new { error = "Health update request is required" });
            }

            await _routingService.UpdateServiceInstanceHealthAsync(serviceName, instanceId, request.IsHealthy);
            
            _logger.LogDebug("Service instance {InstanceId} health updated to {IsHealthy} for {ServiceName}", 
                instanceId, request.IsHealthy, serviceName);

            return Ok(new { message = "Service instance health updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service instance health for {ServiceName}:{InstanceId}", serviceName, instanceId);
            return StatusCode(500, new { error = "Failed to update service instance health" });
        }
    }

    /// <summary>
    /// Get circuit breaker status for a service
    /// </summary>
    [HttpGet("services/{serviceName}/circuit-breaker")]
    public async Task<ActionResult<CircuitBreakerStatus>> GetCircuitBreakerStatus(string serviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            var status = await _routingService.GetCircuitBreakerStatusAsync(serviceName);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting circuit breaker status for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to retrieve circuit breaker status" });
        }
    }

    /// <summary>
    /// Update circuit breaker status for a service
    /// </summary>
    [HttpPut("services/{serviceName}/circuit-breaker")]
    public async Task<ActionResult> UpdateCircuitBreaker(string serviceName, [FromBody] CircuitBreakerUpdateRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return BadRequest(new { error = "Service name is required" });
            }

            if (request == null)
            {
                return BadRequest(new { error = "Circuit breaker update request is required" });
            }

            await _routingService.UpdateCircuitBreakerAsync(serviceName, request.IsOpen, request.Reason);
            
            _logger.LogInformation("Circuit breaker {Action} for {ServiceName} by {UserId}: {Reason}", 
                request.IsOpen ? "opened" : "closed", serviceName, User.FindFirst("sub")?.Value, request.Reason);

            return Ok(new { message = $"Circuit breaker {(request.IsOpen ? "opened" : "closed")} successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating circuit breaker for {ServiceName}", serviceName);
            return StatusCode(500, new { error = "Failed to update circuit breaker" });
        }
    }

    /// <summary>
    /// Perform manual service discovery
    /// </summary>
    [HttpPost("discover")]
    public async Task<ActionResult> PerformServiceDiscovery()
    {
        try
        {
            await _routingService.PerformServiceDiscoveryAsync();
            
            _logger.LogInformation("Manual service discovery performed by {UserId}", User.FindFirst("sub")?.Value);

            return Ok(new { message = "Service discovery completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing service discovery");
            return StatusCode(500, new { error = "Failed to perform service discovery" });
        }
    }

    /// <summary>
    /// Get routing statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetRoutingStats()
    {
        try
        {
            var discoveryInfo = await _routingService.GetServiceDiscoveryInfoAsync();
            var systemHealth = await _healthMonitor.GetSystemHealthScoreAsync();

            var stats = new
            {
                ServiceDiscovery = new
                {
                    TotalServices = discoveryInfo.Services.Count,
                    TotalInstances = discoveryInfo.TotalInstances,
                    HealthyInstances = discoveryInfo.HealthyInstances,
                    UnhealthyInstances = discoveryInfo.UnhealthyInstances,
                    LastDiscoveryRun = discoveryInfo.LastDiscoveryRun
                },
                CircuitBreakers = discoveryInfo.CircuitBreakers.Values.Select(cb => new
                {
                    cb.ServiceName,
                    cb.State,
                    cb.IsOpen,
                    cb.FailureCount,
                    cb.FailureThreshold,
                    cb.OpenedAt,
                    cb.NextAttemptAt
                }),
                SystemHealth = new
                {
                    systemHealth.OverallScore,
                    systemHealth.SystemStatus,
                    systemHealth.TotalServices,
                    systemHealth.HealthyServices,
                    systemHealth.UnhealthyServices,
                    systemHealth.AverageResponseTime
                },
                GeneratedAt = DateTime.UtcNow
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting routing statistics");
            return StatusCode(500, new { error = "Failed to retrieve routing statistics" });
        }
    }
}

/// <summary>
/// Request model for updating service instance health
/// </summary>
public class HealthUpdateRequest
{
    public bool IsHealthy { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Request model for updating circuit breaker status
/// </summary>
public class CircuitBreakerUpdateRequest
{
    public bool IsOpen { get; set; }
    public string? Reason { get; set; }
}