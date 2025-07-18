using QaliTrackGateway.Models;

namespace QaliTrackGateway.Services;

/// <summary>
/// Interface for health-aware routing and service discovery
/// </summary>
public interface IHealthAwareRoutingService
{
    /// <summary>
    /// Get best available service instance for routing
    /// </summary>
    Task<ServiceInstance?> GetBestServiceInstanceAsync(string serviceName);
    
    /// <summary>
    /// Get all available service instances
    /// </summary>
    Task<IEnumerable<ServiceInstance>> GetServiceInstancesAsync(string serviceName);
    
    /// <summary>
    /// Register a service instance
    /// </summary>
    Task RegisterServiceInstanceAsync(ServiceInstance serviceInstance);
    
    /// <summary>
    /// Deregister a service instance
    /// </summary>
    Task DeregisterServiceInstanceAsync(string serviceName, string instanceId);
    
    /// <summary>
    /// Update service instance health status
    /// </summary>
    Task UpdateServiceInstanceHealthAsync(string serviceName, string instanceId, bool isHealthy);
    
    /// <summary>
    /// Get service discovery information
    /// </summary>
    Task<ServiceDiscoveryInfo> GetServiceDiscoveryInfoAsync();
    
    /// <summary>
    /// Perform automatic service discovery
    /// </summary>
    Task PerformServiceDiscoveryAsync();
    
    /// <summary>
    /// Get circuit breaker status for a service
    /// </summary>
    Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(string serviceName);
    
    /// <summary>
    /// Update circuit breaker status
    /// </summary>
    Task UpdateCircuitBreakerAsync(string serviceName, bool isOpen, string? reason = null);
    
    /// <summary>
    /// Check if service should be bypassed due to circuit breaker
    /// </summary>
    Task<bool> ShouldBypassServiceAsync(string serviceName);
}