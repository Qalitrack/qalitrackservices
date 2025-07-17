using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;

namespace QaliTrackGateway.Services;

/// <summary>
/// Health-aware routing and service discovery implementation
/// </summary>
public class HealthAwareRoutingService : IHealthAwareRoutingService, IDisposable
{
    private readonly IServiceHealthMonitor _healthMonitor;
    private readonly ILogger<HealthAwareRoutingService> _logger;
    private readonly IAuditService _auditService;
    private readonly MonitoringSettings _settings;
    private readonly ConcurrentDictionary<string, List<ServiceInstance>> _serviceInstances = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerStatus> _circuitBreakers = new();
    private readonly ConcurrentDictionary<string, ServiceRoutingConfig> _routingConfigs = new();
    private readonly Timer? _discoveryTimer;
    private readonly SemaphoreSlim _discoveryLock = new(1, 1);
    private readonly Random _random = new();

    public HealthAwareRoutingService(
        IServiceHealthMonitor healthMonitor,
        ILogger<HealthAwareRoutingService> logger,
        IAuditService auditService,
        IOptions<MonitoringSettings> settings)
    {
        _healthMonitor = healthMonitor;
        _logger = logger;
        _auditService = auditService;
        _settings = settings.Value;

        // Initialize discovery timer if auto-discovery is enabled
        if (_settings.EnableAutoDiscovery)
        {
            _discoveryTimer = new Timer(async _ => await PerformServiceDiscoveryAsync(), 
                null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        // Initialize default routing configs
        InitializeDefaultRoutingConfigs();
    }

    public async Task<ServiceInstance?> GetBestServiceInstanceAsync(string serviceName)
    {
        try
        {
            // Check circuit breaker first
            if (await ShouldBypassServiceAsync(serviceName))
            {
                _logger.LogWarning("Service {ServiceName} is circuit broken", serviceName);
                return null;
            }

            if (!_serviceInstances.TryGetValue(serviceName, out var instances) || !instances.Any())
            {
                _logger.LogWarning("No instances found for service {ServiceName}", serviceName);
                return null;
            }

            var config = _routingConfigs.GetValueOrDefault(serviceName) ?? new ServiceRoutingConfig
            {
                ServiceName = serviceName,
                Algorithm = LoadBalancingAlgorithm.HealthBased
            };

            // Filter healthy instances
            var healthyInstances = instances.Where(i => i.IsHealthy && i.IsEnabled && i.Status == "Active").ToList();
            
            if (!healthyInstances.Any())
            {
                _logger.LogWarning("No healthy instances found for service {ServiceName}", serviceName);
                
                // Try to use unhealthy instances as fallback if enabled
                if (config.EnableFailover)
                {
                    healthyInstances = instances.Where(i => i.IsEnabled && i.Status == "Active").ToList();
                }
                
                if (!healthyInstances.Any())
                {
                    return null;
                }
            }

            // Select best instance based on algorithm
            var selectedInstance = config.Algorithm switch
            {
                LoadBalancingAlgorithm.RoundRobin => GetRoundRobinInstance(healthyInstances),
                LoadBalancingAlgorithm.LeastConnections => GetLeastConnectionsInstance(healthyInstances),
                LoadBalancingAlgorithm.WeightedRoundRobin => GetWeightedRoundRobinInstance(healthyInstances),
                LoadBalancingAlgorithm.ResponseTime => GetBestResponseTimeInstance(healthyInstances),
                LoadBalancingAlgorithm.HealthBased => GetHealthBasedInstance(healthyInstances),
                _ => GetHealthBasedInstance(healthyInstances)
            };

            if (selectedInstance != null)
            {
                selectedInstance.LastSeen = DateTime.UtcNow;
                _logger.LogDebug("Selected instance {InstanceId} for service {ServiceName}", 
                    selectedInstance.InstanceId, serviceName);
            }

            return selectedInstance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting service instance for {ServiceName}", serviceName);
            return null;
        }
    }

    public async Task<IEnumerable<ServiceInstance>> GetServiceInstancesAsync(string serviceName)
    {
        try
        {
            if (_serviceInstances.TryGetValue(serviceName, out var instances))
            {
                return instances.ToList();
            }
            
            return Enumerable.Empty<ServiceInstance>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service instances for {ServiceName}", serviceName);
            return Enumerable.Empty<ServiceInstance>();
        }
    }

    public async Task RegisterServiceInstanceAsync(ServiceInstance serviceInstance)
    {
        try
        {
            _serviceInstances.AddOrUpdate(serviceInstance.ServiceName,
                new List<ServiceInstance> { serviceInstance },
                (key, existingInstances) =>
                {
                    var existingInstance = existingInstances.FirstOrDefault(i => i.InstanceId == serviceInstance.InstanceId);
                    if (existingInstance != null)
                    {
                        existingInstances.Remove(existingInstance);
                    }
                    existingInstances.Add(serviceInstance);
                    return existingInstances;
                });

            // Initialize circuit breaker for new service
            if (!_circuitBreakers.ContainsKey(serviceInstance.ServiceName))
            {
                _circuitBreakers.TryAdd(serviceInstance.ServiceName, new CircuitBreakerStatus
                {
                    ServiceName = serviceInstance.ServiceName,
                    FailureThreshold = _settings.CircuitBreakerFailureThreshold,
                    Timeout = TimeSpan.FromSeconds(_settings.CircuitBreakerTimeoutSeconds)
                });
            }

            _logger.LogInformation("Service instance {InstanceId} registered for {ServiceName}", 
                serviceInstance.InstanceId, serviceInstance.ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service instance {InstanceId} for {ServiceName}", 
                serviceInstance.InstanceId, serviceInstance.ServiceName);
        }
    }

    public async Task DeregisterServiceInstanceAsync(string serviceName, string instanceId)
    {
        try
        {
            if (_serviceInstances.TryGetValue(serviceName, out var instances))
            {
                var instanceToRemove = instances.FirstOrDefault(i => i.InstanceId == instanceId);
                if (instanceToRemove != null)
                {
                    instances.Remove(instanceToRemove);
                    _logger.LogInformation("Service instance {InstanceId} deregistered from {ServiceName}", 
                        instanceId, serviceName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deregistering service instance {InstanceId} from {ServiceName}", 
                instanceId, serviceName);
        }
    }

    public async Task UpdateServiceInstanceHealthAsync(string serviceName, string instanceId, bool isHealthy)
    {
        try
        {
            if (_serviceInstances.TryGetValue(serviceName, out var instances))
            {
                var instance = instances.FirstOrDefault(i => i.InstanceId == instanceId);
                if (instance != null)
                {
                    instance.IsHealthy = isHealthy;
                    instance.LastHealthCheck = DateTime.UtcNow;
                    
                    if (isHealthy)
                    {
                        instance.ConsecutiveFailures = 0;
                        instance.HealthScore = 100.0;
                    }
                    else
                    {
                        instance.ConsecutiveFailures++;
                        instance.HealthScore = Math.Max(0, 100 - (instance.ConsecutiveFailures * 20));
                    }

                    // Update circuit breaker
                    await UpdateCircuitBreakerAsync(serviceName, !isHealthy, 
                        isHealthy ? null : $"Health check failed for instance {instanceId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service instance health for {ServiceName}:{InstanceId}", 
                serviceName, instanceId);
        }
    }

    public async Task<ServiceDiscoveryInfo> GetServiceDiscoveryInfoAsync()
    {
        try
        {
            var totalInstances = _serviceInstances.Values.Sum(instances => instances.Count);
            var healthyInstances = _serviceInstances.Values.Sum(instances => instances.Count(i => i.IsHealthy));
            var unhealthyInstances = totalInstances - healthyInstances;

            return new ServiceDiscoveryInfo
            {
                Services = _serviceInstances.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList()),
                TotalInstances = totalInstances,
                HealthyInstances = healthyInstances,
                UnhealthyInstances = unhealthyInstances,
                CircuitBreakers = _circuitBreakers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                DiscoveredServices = _serviceInstances.Keys.ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service discovery info");
            return new ServiceDiscoveryInfo();
        }
    }

    public async Task PerformServiceDiscoveryAsync()
    {
        if (!_settings.EnableAutoDiscovery)
        {
            return;
        }

        await _discoveryLock.WaitAsync();
        try
        {
            _logger.LogDebug("Starting service discovery");

            // Get all registered services from health monitor
            var registeredServices = await _healthMonitor.GetAllServicesHealthAsync();
            
            foreach (var service in registeredServices)
            {
                var serviceInstance = new ServiceInstance
                {
                    ServiceName = service.ServiceName,
                    Host = ExtractHostFromUrl(service.HealthCheckUrl),
                    Port = ExtractPortFromUrl(service.HealthCheckUrl),
                    IsHealthy = service.Status == "Healthy",
                    ResponseTimeMs = service.ResponseTimeMs,
                    LastHealthCheck = service.LastChecked,
                    Tags = service.Tags,
                    HealthScore = service.HealthScore
                };

                await RegisterServiceInstanceAsync(serviceInstance);
            }

            // Discover services based on patterns
            if (_settings.AutoDiscoveryPatterns?.Any() == true)
            {
                await DiscoverServicesByPatternAsync();
            }

            _logger.LogDebug("Service discovery completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during service discovery");
        }
        finally
        {
            _discoveryLock.Release();
        }
    }

    public async Task<CircuitBreakerStatus> GetCircuitBreakerStatusAsync(string serviceName)
    {
        if (_circuitBreakers.TryGetValue(serviceName, out var status))
        {
            return status;
        }

        // Create default circuit breaker status
        var defaultStatus = new CircuitBreakerStatus
        {
            ServiceName = serviceName,
            FailureThreshold = _settings.CircuitBreakerFailureThreshold,
            Timeout = TimeSpan.FromSeconds(_settings.CircuitBreakerTimeoutSeconds)
        };

        _circuitBreakers.TryAdd(serviceName, defaultStatus);
        return defaultStatus;
    }

    public async Task UpdateCircuitBreakerAsync(string serviceName, bool isOpen, string? reason = null)
    {
        try
        {
            var status = await GetCircuitBreakerStatusAsync(serviceName);
            
            if (isOpen)
            {
                status.FailureCount++;
                status.LastFailureAt = DateTime.UtcNow;
                status.LastFailureReason = reason;

                if (status.FailureCount >= status.FailureThreshold && status.State == CircuitBreakerState.Closed)
                {
                    status.State = CircuitBreakerState.Open;
                    status.IsOpen = true;
                    status.OpenedAt = DateTime.UtcNow;
                    status.NextAttemptAt = DateTime.UtcNow.Add(status.Timeout);
                    
                    _logger.LogWarning("Circuit breaker opened for service {ServiceName} after {FailureCount} failures", 
                        serviceName, status.FailureCount);
                }
            }
            else
            {
                // Success - reset failure count
                status.FailureCount = 0;
                
                if (status.State == CircuitBreakerState.Open && DateTime.UtcNow >= status.NextAttemptAt)
                {
                    status.State = CircuitBreakerState.HalfOpen;
                    _logger.LogInformation("Circuit breaker half-opened for service {ServiceName}", serviceName);
                }
                else if (status.State == CircuitBreakerState.HalfOpen)
                {
                    status.State = CircuitBreakerState.Closed;
                    status.IsOpen = false;
                    status.OpenedAt = null;
                    _logger.LogInformation("Circuit breaker closed for service {ServiceName}", serviceName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating circuit breaker for service {ServiceName}", serviceName);
        }
    }

    public async Task<bool> ShouldBypassServiceAsync(string serviceName)
    {
        if (!_settings.EnableCircuitBreaker)
        {
            return false;
        }

        var status = await GetCircuitBreakerStatusAsync(serviceName);
        
        if (status.State == CircuitBreakerState.Open)
        {
            if (DateTime.UtcNow >= status.NextAttemptAt)
            {
                // Transition to half-open
                status.State = CircuitBreakerState.HalfOpen;
                return false;
            }
            return true;
        }

        return false;
    }

    private ServiceInstance? GetRoundRobinInstance(List<ServiceInstance> instances)
    {
        if (!instances.Any()) return null;
        
        // Simple round-robin based on timestamp
        var index = (int)(DateTime.UtcNow.Ticks % instances.Count);
        return instances[index];
    }

    private ServiceInstance? GetLeastConnectionsInstance(List<ServiceInstance> instances)
    {
        // For now, use health score as a proxy for connections
        return instances.OrderBy(i => 100 - i.HealthScore).FirstOrDefault();
    }

    private ServiceInstance? GetWeightedRoundRobinInstance(List<ServiceInstance> instances)
    {
        var totalWeight = instances.Sum(i => i.Weight);
        if (totalWeight == 0) return instances.FirstOrDefault();
        
        var random = _random.Next(totalWeight);
        var currentWeight = 0;
        
        foreach (var instance in instances)
        {
            currentWeight += instance.Weight;
            if (random < currentWeight)
            {
                return instance;
            }
        }
        
        return instances.FirstOrDefault();
    }

    private ServiceInstance? GetBestResponseTimeInstance(List<ServiceInstance> instances)
    {
        return instances.OrderBy(i => i.ResponseTimeMs).FirstOrDefault();
    }

    private ServiceInstance? GetHealthBasedInstance(List<ServiceInstance> instances)
    {
        // Combine health score and response time
        return instances.OrderByDescending(i => i.HealthScore)
                       .ThenBy(i => i.ResponseTimeMs)
                       .FirstOrDefault();
    }

    private void InitializeDefaultRoutingConfigs()
    {
        var defaultServices = new[]
        {
            "user-service", "transaction-service", "customer-service", "product-service",
            "vehicle-service", "driver-service", "route-service", "weighbridge-service"
        };

        foreach (var serviceName in defaultServices)
        {
            _routingConfigs.TryAdd(serviceName, new ServiceRoutingConfig
            {
                ServiceName = serviceName,
                Algorithm = LoadBalancingAlgorithm.HealthBased,
                EnableHealthCheck = true,
                EnableCircuitBreaker = _settings.EnableCircuitBreaker,
                EnableFailover = true
            });
        }
    }

    private async Task DiscoverServicesByPatternAsync()
    {
        // Simple pattern-based discovery
        // In a real implementation, this would integrate with service registries like Consul, Eureka, etc.
        _logger.LogDebug("Performing pattern-based service discovery");
        
        // This is a placeholder - actual implementation would query service registries
        // or use network discovery mechanisms
    }

    private string ExtractHostFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Host;
        }
        catch
        {
            return "localhost";
        }
    }

    private int ExtractPortFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return uri.Port;
        }
        catch
        {
            return 80;
        }
    }

    public void Dispose()
    {
        _discoveryTimer?.Dispose();
        _discoveryLock?.Dispose();
    }
}