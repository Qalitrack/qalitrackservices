using QaliTrackGateway.Models;

namespace QaliTrackGateway.Services;

/// <summary>
/// Interface for enhanced service health monitoring
/// </summary>
public interface IServiceHealthMonitor
{
    /// <summary>
    /// Get real-time health status of all services
    /// </summary>
    Task<IEnumerable<ServiceHealthStatus>> GetAllServicesHealthAsync();
    
    /// <summary>
    /// Get health status of a specific service
    /// </summary>
    Task<ServiceHealthStatus> GetServiceHealthAsync(string serviceName);
    
    /// <summary>
    /// Get health history for a service
    /// </summary>
    Task<IEnumerable<ServiceHealthCheck>> GetServiceHealthHistoryAsync(string serviceName, int limitHours = 24);
    
    /// <summary>
    /// Get overall system health score
    /// </summary>
    Task<SystemHealthScore> GetSystemHealthScoreAsync();
    
    /// <summary>
    /// Get service dependency map
    /// </summary>
    Task<ServiceDependencyMap> GetServiceDependencyMapAsync();
    
    /// <summary>
    /// Start monitoring all services
    /// </summary>
    Task StartMonitoringAsync();
    
    /// <summary>
    /// Stop monitoring all services
    /// </summary>
    Task StopMonitoringAsync();
    
    /// <summary>
    /// Register a new service for monitoring
    /// </summary>
    Task RegisterServiceAsync(ServiceRegistration serviceRegistration);
    
    /// <summary>
    /// Unregister a service from monitoring
    /// </summary>
    Task UnregisterServiceAsync(string serviceName);
    
    /// <summary>
    /// Get unhealthy services for alerting
    /// </summary>
    Task<IEnumerable<ServiceHealthStatus>> GetUnhealthyServicesAsync();
    
    /// <summary>
    /// Get service performance metrics
    /// </summary>
    Task<ServicePerformanceMetrics> GetServicePerformanceAsync(string serviceName);
}