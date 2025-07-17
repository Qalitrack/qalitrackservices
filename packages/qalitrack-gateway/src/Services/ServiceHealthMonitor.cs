using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Extensions.Options;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;

namespace QaliTrackGateway.Services;

/// <summary>
/// Enhanced service health monitoring implementation
/// </summary>
public class ServiceHealthMonitor : IServiceHealthMonitor, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ServiceHealthMonitor> _logger;
    private readonly IAuditService _auditService;
    private readonly MonitoringSettings _settings;
    private readonly ConcurrentDictionary<string, ServiceHealthStatus> _serviceHealthCache = new();
    private readonly ConcurrentDictionary<string, ServiceRegistration> _registeredServices = new();
    private readonly ConcurrentDictionary<string, List<ServiceHealthCheck>> _healthHistory = new();
    private readonly Timer? _monitoringTimer;
    private readonly SemaphoreSlim _monitoringSemaphore = new(1, 1);
    private bool _isMonitoring = false;

    public ServiceHealthMonitor(
        HttpClient httpClient,
        ILogger<ServiceHealthMonitor> logger,
        IAuditService auditService,
        IOptions<MonitoringSettings> settings)
    {
        _httpClient = httpClient;
        _logger = logger;
        _auditService = auditService;
        _settings = settings.Value;

        // Initialize monitoring timer
        _monitoringTimer = new Timer(PerformHealthChecksAsync, null, 
            TimeSpan.FromSeconds(10), // Initial delay
            TimeSpan.FromSeconds(_settings.DefaultHealthCheckIntervalSeconds));
    }

    public async Task<IEnumerable<ServiceHealthStatus>> GetAllServicesHealthAsync()
    {
        try
        {
            // Return cached health status for all services
            return _serviceHealthCache.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all services health");
            return Enumerable.Empty<ServiceHealthStatus>();
        }
    }

    public async Task<ServiceHealthStatus> GetServiceHealthAsync(string serviceName)
    {
        try
        {
            if (_serviceHealthCache.TryGetValue(serviceName, out var cachedStatus))
            {
                return cachedStatus;
            }

            // If not in cache, perform immediate health check
            if (_registeredServices.TryGetValue(serviceName, out var registration))
            {
                var healthStatus = await PerformHealthCheckAsync(registration);
                _serviceHealthCache.TryAdd(serviceName, healthStatus);
                return healthStatus;
            }

            // Service not registered
            return new ServiceHealthStatus
            {
                ServiceName = serviceName,
                Status = "Unknown",
                ErrorMessage = "Service not registered for monitoring",
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service health for {ServiceName}", serviceName);
            return new ServiceHealthStatus
            {
                ServiceName = serviceName,
                Status = "Error",
                ErrorMessage = ex.Message,
                LastChecked = DateTime.UtcNow
            };
        }
    }

    public async Task<IEnumerable<ServiceHealthCheck>> GetServiceHealthHistoryAsync(string serviceName, int limitHours = 24)
    {
        try
        {
            if (_healthHistory.TryGetValue(serviceName, out var history))
            {
                var cutoff = DateTime.UtcNow.AddHours(-limitHours);
                return history.Where(h => h.CheckTime >= cutoff)
                            .OrderByDescending(h => h.CheckTime)
                            .Take(1000) // Limit to prevent memory issues
                            .ToList();
            }

            return Enumerable.Empty<ServiceHealthCheck>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service health history for {ServiceName}", serviceName);
            return Enumerable.Empty<ServiceHealthCheck>();
        }
    }

    public async Task<SystemHealthScore> GetSystemHealthScoreAsync()
    {
        try
        {
            var allServices = _serviceHealthCache.Values.ToList();
            var totalServices = allServices.Count;
            
            if (totalServices == 0)
            {
                return new SystemHealthScore
                {
                    OverallScore = 0,
                    SystemStatus = "Unknown",
                    CriticalIssues = new List<string> { "No services registered for monitoring" }
                };
            }

            var healthyServices = allServices.Count(s => s.Status == "Healthy");
            var unhealthyServices = allServices.Count(s => s.Status == "Unhealthy");
            var degradedServices = allServices.Count(s => s.Status == "Degraded");

            var overallScore = (double)healthyServices / totalServices * 100;
            var averageResponseTime = allServices.Where(s => s.Status == "Healthy")
                                                .Average(s => s.ResponseTimeMs);

            var systemStatus = overallScore switch
            {
                >= 95 => "Healthy",
                >= 80 => "Degraded",
                _ => "Critical"
            };

            var criticalIssues = new List<string>();
            var warnings = new List<string>();

            foreach (var service in allServices.Where(s => s.Status == "Unhealthy"))
            {
                criticalIssues.Add($"{service.ServiceName}: {service.ErrorMessage}");
            }

            foreach (var service in allServices.Where(s => s.Status == "Degraded"))
            {
                warnings.Add($"{service.ServiceName}: Response time {service.ResponseTimeMs}ms");
            }

            return new SystemHealthScore
            {
                OverallScore = overallScore,
                TotalServices = totalServices,
                HealthyServices = healthyServices,
                UnhealthyServices = unhealthyServices,
                DegradedServices = degradedServices,
                CriticalIssues = criticalIssues,
                Warnings = warnings,
                AverageResponseTime = averageResponseTime,
                SystemStatus = systemStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating system health score");
            return new SystemHealthScore
            {
                OverallScore = 0,
                SystemStatus = "Error",
                CriticalIssues = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ServiceDependencyMap> GetServiceDependencyMapAsync()
    {
        try
        {
            var services = _registeredServices.Values.ToList();
            var nodes = new List<ServiceNode>();
            var dependencies = new List<ServiceDependency>();

            // Create service nodes
            foreach (var service in services)
            {
                var healthStatus = await GetServiceHealthAsync(service.ServiceName);
                nodes.Add(new ServiceNode
                {
                    ServiceName = service.ServiceName,
                    Status = healthStatus.Status,
                    Type = DetermineServiceType(service.ServiceName),
                    Tags = service.Tags,
                    IsCritical = service.Tags.Contains("critical")
                });
            }

            // Add gateway node
            nodes.Add(new ServiceNode
            {
                ServiceName = "qalitrack-gateway",
                Status = "Healthy",
                Type = "Gateway",
                IsCritical = true
            });

            // Create dependencies (simplified - in real implementation, this would be more sophisticated)
            foreach (var service in services)
            {
                dependencies.Add(new ServiceDependency
                {
                    FromService = "qalitrack-gateway",
                    ToService = service.ServiceName,
                    DependencyType = "API",
                    IsRequired = true,
                    Description = $"Gateway routes requests to {service.ServiceName}"
                });
            }

            return new ServiceDependencyMap
            {
                Services = nodes,
                Dependencies = dependencies
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating service dependency map");
            return new ServiceDependencyMap();
        }
    }

    public async Task StartMonitoringAsync()
    {
        await _monitoringSemaphore.WaitAsync();
        try
        {
            if (!_isMonitoring)
            {
                _isMonitoring = true;
                _logger.LogInformation("Service health monitoring started");
                
                // Load default services from configuration
                await LoadDefaultServicesAsync();
            }
        }
        finally
        {
            _monitoringSemaphore.Release();
        }
    }

    public async Task StopMonitoringAsync()
    {
        await _monitoringSemaphore.WaitAsync();
        try
        {
            if (_isMonitoring)
            {
                _isMonitoring = false;
                _logger.LogInformation("Service health monitoring stopped");
            }
        }
        finally
        {
            _monitoringSemaphore.Release();
        }
    }

    public async Task RegisterServiceAsync(ServiceRegistration serviceRegistration)
    {
        try
        {
            _registeredServices.AddOrUpdate(serviceRegistration.ServiceName, serviceRegistration, 
                (key, oldValue) => serviceRegistration);
            
            // Initialize health status
            _serviceHealthCache.TryAdd(serviceRegistration.ServiceName, new ServiceHealthStatus
            {
                ServiceName = serviceRegistration.ServiceName,
                Status = "Unknown",
                HealthCheckUrl = serviceRegistration.HealthCheckUrl,
                IsEnabled = serviceRegistration.IsEnabled,
                Tags = serviceRegistration.Tags
            });

            _logger.LogInformation("Service {ServiceName} registered for monitoring", serviceRegistration.ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering service {ServiceName}", serviceRegistration.ServiceName);
        }
    }

    public async Task UnregisterServiceAsync(string serviceName)
    {
        try
        {
            _registeredServices.TryRemove(serviceName, out _);
            _serviceHealthCache.TryRemove(serviceName, out _);
            _healthHistory.TryRemove(serviceName, out _);
            
            _logger.LogInformation("Service {ServiceName} unregistered from monitoring", serviceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering service {ServiceName}", serviceName);
        }
    }

    public async Task<IEnumerable<ServiceHealthStatus>> GetUnhealthyServicesAsync()
    {
        try
        {
            return _serviceHealthCache.Values
                .Where(s => s.Status == "Unhealthy" || s.Status == "Degraded")
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unhealthy services");
            return Enumerable.Empty<ServiceHealthStatus>();
        }
    }

    public async Task<ServicePerformanceMetrics> GetServicePerformanceAsync(string serviceName)
    {
        try
        {
            if (_healthHistory.TryGetValue(serviceName, out var history))
            {
                var recentHistory = history.Where(h => h.CheckTime >= DateTime.UtcNow.AddHours(-24))
                                          .OrderByDescending(h => h.CheckTime)
                                          .ToList();

                if (recentHistory.Any())
                {
                    var responseTimes = recentHistory.Where(h => h.ResponseTimeMs > 0)
                                                   .Select(h => (double)h.ResponseTimeMs);
                    
                    var successfulChecks = recentHistory.Count(h => h.Status == "Healthy");
                    var totalChecks = recentHistory.Count;

                    return new ServicePerformanceMetrics
                    {
                        ServiceName = serviceName,
                        AverageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0,
                        MinResponseTime = responseTimes.Any() ? responseTimes.Min() : 0,
                        MaxResponseTime = responseTimes.Any() ? responseTimes.Max() : 0,
                        SuccessRate = totalChecks > 0 ? (double)successfulChecks / totalChecks * 100 : 0,
                        TotalRequests = totalChecks,
                        SuccessfulRequests = successfulChecks,
                        FailedRequests = totalChecks - successfulChecks,
                        PeriodStart = DateTime.UtcNow.AddHours(-24),
                        PeriodEnd = DateTime.UtcNow,
                        ResponseTimeHistory = recentHistory.Select(h => new ResponseTimePoint
                        {
                            Timestamp = h.CheckTime,
                            ResponseTime = h.ResponseTimeMs,
                            Status = h.Status
                        }).ToList()
                    };
                }
            }

            return new ServicePerformanceMetrics
            {
                ServiceName = serviceName,
                PeriodStart = DateTime.UtcNow.AddHours(-24),
                PeriodEnd = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service performance for {ServiceName}", serviceName);
            return new ServicePerformanceMetrics { ServiceName = serviceName };
        }
    }

    private async void PerformHealthChecksAsync(object? state)
    {
        if (!_isMonitoring) return;

        try
        {
            var services = _registeredServices.Values.Where(s => s.IsEnabled).ToList();
            
            var healthCheckTasks = services.Select(async service =>
            {
                try
                {
                    var healthStatus = await PerformHealthCheckAsync(service);
                    _serviceHealthCache.AddOrUpdate(service.ServiceName, healthStatus, 
                        (key, oldValue) => healthStatus);
                    
                    // Store health history
                    RecordHealthCheck(service.ServiceName, healthStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error performing health check for {ServiceName}", service.ServiceName);
                }
            });

            await Task.WhenAll(healthCheckTasks);

            // Log system health audit event if enabled
            if (_settings.EnableHealthCheckAudit)
            {
                await LogHealthCheckAuditAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during health check cycle");
        }
    }

    private async Task<ServiceHealthStatus> PerformHealthCheckAsync(ServiceRegistration service)
    {
        var stopwatch = Stopwatch.StartNew();
        var healthStatus = new ServiceHealthStatus
        {
            ServiceName = service.ServiceName,
            HealthCheckUrl = service.HealthCheckUrl,
            IsEnabled = service.IsEnabled,
            Tags = service.Tags,
            Version = service.Version
        };

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(service.TimeoutSeconds));
            using var response = await _httpClient.GetAsync(service.HealthCheckUrl, cts.Token);

            stopwatch.Stop();
            healthStatus.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            healthStatus.LastChecked = DateTime.UtcNow;

            if (response.IsSuccessStatusCode)
            {
                healthStatus.Status = "Healthy";
                healthStatus.LastHealthyTime = DateTime.UtcNow;
                healthStatus.ConsecutiveFailures = 0;
                healthStatus.HealthScore = 100;
                
                // Reset consecutive failures in cache
                if (_serviceHealthCache.TryGetValue(service.ServiceName, out var existingStatus))
                {
                    existingStatus.ConsecutiveFailures = 0;
                }
            }
            else
            {
                healthStatus.Status = "Unhealthy";
                healthStatus.ErrorMessage = $"HTTP {response.StatusCode}: {response.ReasonPhrase}";
                healthStatus.ConsecutiveFailures = GetConsecutiveFailures(service.ServiceName) + 1;
                healthStatus.HealthScore = 0;
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            healthStatus.Status = "Unhealthy";
            healthStatus.ErrorMessage = $"Health check timeout after {service.TimeoutSeconds} seconds";
            healthStatus.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            healthStatus.LastChecked = DateTime.UtcNow;
            healthStatus.ConsecutiveFailures = GetConsecutiveFailures(service.ServiceName) + 1;
            healthStatus.HealthScore = 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            healthStatus.Status = "Unhealthy";
            healthStatus.ErrorMessage = ex.Message;
            healthStatus.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            healthStatus.LastChecked = DateTime.UtcNow;
            healthStatus.ConsecutiveFailures = GetConsecutiveFailures(service.ServiceName) + 1;
            healthStatus.HealthScore = 0;
        }

        // Determine if service is degraded based on response time
        if (healthStatus.Status == "Healthy" && healthStatus.ResponseTimeMs > _settings.DegradedResponseTimeMs)
        {
            healthStatus.Status = "Degraded";
            healthStatus.HealthScore = 75;
        }

        return healthStatus;
    }

    private int GetConsecutiveFailures(string serviceName)
    {
        if (_serviceHealthCache.TryGetValue(serviceName, out var status))
        {
            return status.ConsecutiveFailures;
        }
        return 0;
    }

    private void RecordHealthCheck(string serviceName, ServiceHealthStatus healthStatus)
    {
        var healthCheck = new ServiceHealthCheck
        {
            ServiceName = serviceName,
            Status = healthStatus.Status,
            ResponseTimeMs = healthStatus.ResponseTimeMs,
            ErrorMessage = healthStatus.ErrorMessage,
            CheckTime = healthStatus.LastChecked
        };

        _healthHistory.AddOrUpdate(serviceName, 
            new List<ServiceHealthCheck> { healthCheck },
            (key, existingList) =>
            {
                existingList.Add(healthCheck);
                
                // Keep only last 1000 records per service
                if (existingList.Count > 1000)
                {
                    existingList.RemoveRange(0, existingList.Count - 1000);
                }
                
                return existingList;
            });
    }

    private async Task LogHealthCheckAuditAsync()
    {
        try
        {
            var allServices = _serviceHealthCache.Values.ToList();
            var healthyServices = allServices.Where(s => s.Status == "Healthy").Select(s => s.ServiceName).ToArray();
            var unhealthyServices = allServices.Where(s => s.Status == "Unhealthy").Select(s => s.ServiceName).ToArray();
            var systemHealth = await GetSystemHealthScoreAsync();

            var healthAuditEvent = new HealthCheckAuditEvent
            {
                CheckedServices = allServices.Select(s => s.ServiceName).ToArray(),
                HealthyServices = healthyServices,
                UnhealthyServices = unhealthyServices,
                TotalServices = allServices.Count,
                HealthyCount = healthyServices.Length,
                OverallHealthScore = systemHealth.OverallScore,
                Action = "SystemHealthCheck",
                ServiceName = "QaliTrack-Gateway",
                UserId = "system",
                UserName = "HealthMonitor"
            };

            await _auditService.LogHealthCheckEventAsync(healthAuditEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging health check audit event");
        }
    }

    private async Task LoadDefaultServicesAsync()
    {
        // Load default services from configuration
        var defaultServices = new[]
        {
            new ServiceRegistration
            {
                ServiceName = "user-service",
                HealthCheckUrl = "http://user-service/health",
                BaseUrl = "http://user-service",
                Tags = new[] { "core", "critical" }
            },
            new ServiceRegistration
            {
                ServiceName = "transaction-service",
                HealthCheckUrl = "http://transaction-service/health",
                BaseUrl = "http://transaction-service",
                Tags = new[] { "core", "critical" }
            },
            new ServiceRegistration
            {
                ServiceName = "customer-service",
                HealthCheckUrl = "http://customer-service/health",
                BaseUrl = "http://customer-service",
                Tags = new[] { "business" }
            },
            new ServiceRegistration
            {
                ServiceName = "product-service",
                HealthCheckUrl = "http://product-service/health",
                BaseUrl = "http://product-service",
                Tags = new[] { "business" }
            }
        };

        foreach (var service in defaultServices)
        {
            await RegisterServiceAsync(service);
        }
    }

    private string DetermineServiceType(string serviceName)
    {
        return serviceName.ToLower() switch
        {
            var name when name.Contains("gateway") => "Gateway",
            var name when name.Contains("user") => "Authentication",
            var name when name.Contains("transaction") => "Core",
            var name when name.Contains("database") => "Database",
            _ => "Microservice"
        };
    }

    public void Dispose()
    {
        _monitoringTimer?.Dispose();
        _monitoringSemaphore?.Dispose();
    }
}