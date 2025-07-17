using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Xunit;
using Xunit.Abstractions;
using QaliTrackGateway.Services;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;
using Moq;

namespace QaliTrackGateway.Tests.IntegrationTests;

public class HealthAwareRoutingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public HealthAwareRoutingIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task GetBestServiceInstanceAsync_WithHealthyInstances_ShouldReturnHealthiest()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            EnableHealthBasedLoadBalancing = true,
            EnableCircuitBreaker = true
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register service instances
        var instances = new[]
        {
            new ServiceInstance
            {
                ServiceName = "test-service",
                InstanceId = "instance-1",
                Host = "host1",
                Port = 8080,
                IsHealthy = true,
                HealthScore = 100,
                ResponseTimeMs = 100
            },
            new ServiceInstance
            {
                ServiceName = "test-service",
                InstanceId = "instance-2",
                Host = "host2",
                Port = 8080,
                IsHealthy = true,
                HealthScore = 85,
                ResponseTimeMs = 200
            },
            new ServiceInstance
            {
                ServiceName = "test-service",
                InstanceId = "instance-3",
                Host = "host3",
                Port = 8080,
                IsHealthy = false,
                HealthScore = 0,
                ResponseTimeMs = 1000
            }
        };

        foreach (var instance in instances)
        {
            await routingService.RegisterServiceInstanceAsync(instance);
        }

        // Act
        var bestInstance = await routingService.GetBestServiceInstanceAsync("test-service");

        // Assert
        Assert.NotNull(bestInstance);
        Assert.Equal("instance-1", bestInstance.InstanceId);
        Assert.Equal(100, bestInstance.HealthScore);
        Assert.Equal(100, bestInstance.ResponseTimeMs);
        Assert.True(bestInstance.IsHealthy);
    }

    [Fact]
    public async Task GetBestServiceInstanceAsync_WithCircuitBreakerOpen_ShouldReturnNull()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            EnableCircuitBreaker = true,
            CircuitBreakerFailureThreshold = 3,
            CircuitBreakerTimeoutSeconds = 60
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register service instance
        var instance = new ServiceInstance
        {
            ServiceName = "failing-service",
            InstanceId = "instance-1",
            Host = "host1",
            Port = 8080,
            IsHealthy = false,
            HealthScore = 0
        };

        await routingService.RegisterServiceInstanceAsync(instance);

        // Trigger circuit breaker by simulating failures
        for (int i = 0; i < 5; i++)
        {
            await routingService.UpdateCircuitBreakerAsync("failing-service", true, "Simulated failure");
        }

        // Act
        var shouldBypass = await routingService.ShouldBypassServiceAsync("failing-service");
        var bestInstance = await routingService.GetBestServiceInstanceAsync("failing-service");

        // Assert
        Assert.True(shouldBypass);
        Assert.Null(bestInstance);
    }

    [Fact]
    public async Task GetServiceInstancesAsync_WithRegisteredInstances_ShouldReturnAllInstances()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register multiple instances
        var instances = new[]
        {
            new ServiceInstance
            {
                ServiceName = "multi-instance-service",
                InstanceId = "instance-1",
                Host = "host1",
                Port = 8080
            },
            new ServiceInstance
            {
                ServiceName = "multi-instance-service",
                InstanceId = "instance-2",
                Host = "host2",
                Port = 8080
            },
            new ServiceInstance
            {
                ServiceName = "multi-instance-service",
                InstanceId = "instance-3",
                Host = "host3",
                Port = 8080
            }
        };

        foreach (var instance in instances)
        {
            await routingService.RegisterServiceInstanceAsync(instance);
        }

        // Act
        var retrievedInstances = await routingService.GetServiceInstancesAsync("multi-instance-service");

        // Assert
        Assert.NotNull(retrievedInstances);
        Assert.Equal(3, retrievedInstances.Count());
        Assert.Contains(retrievedInstances, i => i.InstanceId == "instance-1");
        Assert.Contains(retrievedInstances, i => i.InstanceId == "instance-2");
        Assert.Contains(retrievedInstances, i => i.InstanceId == "instance-3");
    }

    [Fact]
    public async Task UpdateServiceInstanceHealthAsync_WithHealthyUpdate_ShouldUpdateHealthStatus()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register service instance
        var instance = new ServiceInstance
        {
            ServiceName = "update-test-service",
            InstanceId = "instance-1",
            Host = "host1",
            Port = 8080,
            IsHealthy = false,
            HealthScore = 0,
            ConsecutiveFailures = 3
        };

        await routingService.RegisterServiceInstanceAsync(instance);

        // Act
        await routingService.UpdateServiceInstanceHealthAsync("update-test-service", "instance-1", true);

        // Assert
        var instances = await routingService.GetServiceInstancesAsync("update-test-service");
        var updatedInstance = instances.First(i => i.InstanceId == "instance-1");
        
        Assert.True(updatedInstance.IsHealthy);
        Assert.Equal(100.0, updatedInstance.HealthScore);
        Assert.Equal(0, updatedInstance.ConsecutiveFailures);
        Assert.True(updatedInstance.LastHealthCheck > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeregisterServiceInstanceAsync_WithExistingInstance_ShouldRemoveInstance()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register service instances
        var instances = new[]
        {
            new ServiceInstance
            {
                ServiceName = "deregister-test-service",
                InstanceId = "instance-1",
                Host = "host1",
                Port = 8080
            },
            new ServiceInstance
            {
                ServiceName = "deregister-test-service",
                InstanceId = "instance-2",
                Host = "host2",
                Port = 8080
            }
        };

        foreach (var instance in instances)
        {
            await routingService.RegisterServiceInstanceAsync(instance);
        }

        // Verify both instances exist
        var allInstances = await routingService.GetServiceInstancesAsync("deregister-test-service");
        Assert.Equal(2, allInstances.Count());

        // Act
        await routingService.DeregisterServiceInstanceAsync("deregister-test-service", "instance-1");

        // Assert
        var remainingInstances = await routingService.GetServiceInstancesAsync("deregister-test-service");
        Assert.Single(remainingInstances);
        Assert.Equal("instance-2", remainingInstances.First().InstanceId);
    }

    [Fact]
    public async Task GetCircuitBreakerStatusAsync_WithNewService_ShouldReturnDefaultStatus()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            CircuitBreakerFailureThreshold = 5,
            CircuitBreakerTimeoutSeconds = 30
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Act
        var circuitBreakerStatus = await routingService.GetCircuitBreakerStatusAsync("new-service");

        // Assert
        Assert.NotNull(circuitBreakerStatus);
        Assert.Equal("new-service", circuitBreakerStatus.ServiceName);
        Assert.False(circuitBreakerStatus.IsOpen);
        Assert.Equal(CircuitBreakerState.Closed, circuitBreakerStatus.State);
        Assert.Equal(0, circuitBreakerStatus.FailureCount);
        Assert.Equal(5, circuitBreakerStatus.FailureThreshold);
        Assert.Equal(TimeSpan.FromSeconds(30), circuitBreakerStatus.Timeout);
    }

    [Fact]
    public async Task UpdateCircuitBreakerAsync_WithMultipleFailures_ShouldOpenCircuitBreaker()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            CircuitBreakerFailureThreshold = 3,
            CircuitBreakerTimeoutSeconds = 60
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Act - Simulate multiple failures
        for (int i = 0; i < 3; i++)
        {
            await routingService.UpdateCircuitBreakerAsync("failing-service", true, $"Failure {i + 1}");
        }

        // Assert
        var circuitBreakerStatus = await routingService.GetCircuitBreakerStatusAsync("failing-service");
        Assert.True(circuitBreakerStatus.IsOpen);
        Assert.Equal(CircuitBreakerState.Open, circuitBreakerStatus.State);
        Assert.Equal(3, circuitBreakerStatus.FailureCount);
        Assert.NotNull(circuitBreakerStatus.OpenedAt);
        Assert.NotNull(circuitBreakerStatus.LastFailureAt);
        Assert.Equal("Failure 3", circuitBreakerStatus.LastFailureReason);
        Assert.True(circuitBreakerStatus.NextAttemptAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task UpdateCircuitBreakerAsync_WithSuccessAfterFailures_ShouldResetFailureCount()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            CircuitBreakerFailureThreshold = 5,
            CircuitBreakerTimeoutSeconds = 60
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Simulate some failures
        await routingService.UpdateCircuitBreakerAsync("recovery-service", true, "Failure 1");
        await routingService.UpdateCircuitBreakerAsync("recovery-service", true, "Failure 2");

        var statusAfterFailures = await routingService.GetCircuitBreakerStatusAsync("recovery-service");
        Assert.Equal(2, statusAfterFailures.FailureCount);

        // Act - Simulate success
        await routingService.UpdateCircuitBreakerAsync("recovery-service", false);

        // Assert
        var statusAfterSuccess = await routingService.GetCircuitBreakerStatusAsync("recovery-service");
        Assert.Equal(0, statusAfterSuccess.FailureCount);
        Assert.Equal(CircuitBreakerState.Closed, statusAfterSuccess.State);
        Assert.False(statusAfterSuccess.IsOpen);
    }

    [Fact]
    public async Task GetServiceDiscoveryInfoAsync_WithRegisteredServices_ShouldReturnCompleteInfo()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register service instances
        var healthyInstance = new ServiceInstance
        {
            ServiceName = "discovery-service",
            InstanceId = "healthy-instance",
            Host = "host1",
            Port = 8080,
            IsHealthy = true
        };

        var unhealthyInstance = new ServiceInstance
        {
            ServiceName = "discovery-service",
            InstanceId = "unhealthy-instance",
            Host = "host2",
            Port = 8080,
            IsHealthy = false
        };

        await routingService.RegisterServiceInstanceAsync(healthyInstance);
        await routingService.RegisterServiceInstanceAsync(unhealthyInstance);

        // Act
        var discoveryInfo = await routingService.GetServiceDiscoveryInfoAsync();

        // Assert
        Assert.NotNull(discoveryInfo);
        Assert.Equal(2, discoveryInfo.TotalInstances);
        Assert.Equal(1, discoveryInfo.HealthyInstances);
        Assert.Equal(1, discoveryInfo.UnhealthyInstances);
        Assert.Contains("discovery-service", discoveryInfo.Services.Keys);
        Assert.Equal(2, discoveryInfo.Services["discovery-service"].Count);
        Assert.Contains("discovery-service", discoveryInfo.DiscoveredServices);
        Assert.Contains("discovery-service", discoveryInfo.CircuitBreakers.Keys);
        Assert.True(discoveryInfo.LastDiscoveryRun <= DateTime.UtcNow);
    }

    [Fact]
    public async Task ShouldBypassServiceAsync_WhenCircuitBreakerDisabled_ShouldReturnFalse()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings
        {
            EnableCircuitBreaker = false
        });

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Act
        var shouldBypass = await routingService.ShouldBypassServiceAsync("any-service");

        // Assert
        Assert.False(shouldBypass);
    }

    [Fact]
    public async Task GetBestServiceInstanceAsync_WithNoHealthyInstances_ShouldUseFailover()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register only unhealthy instances
        var unhealthyInstance = new ServiceInstance
        {
            ServiceName = "failover-service",
            InstanceId = "unhealthy-instance",
            Host = "host1",
            Port = 8080,
            IsHealthy = false,
            IsEnabled = true,
            Status = "Active"
        };

        await routingService.RegisterServiceInstanceAsync(unhealthyInstance);

        // Act
        var bestInstance = await routingService.GetBestServiceInstanceAsync("failover-service");

        // Assert
        Assert.NotNull(bestInstance);
        Assert.Equal("unhealthy-instance", bestInstance.InstanceId);
        Assert.False(bestInstance.IsHealthy);
    }

    [Fact]
    public async Task GetBestServiceInstanceAsync_WithDisabledInstances_ShouldSkipDisabled()
    {
        // Arrange
        var healthMonitor = new Mock<IServiceHealthMonitor>();
        var logger = new Mock<ILogger<HealthAwareRoutingService>>();
        var auditService = new Mock<IAuditService>();
        var settings = Options.Create(new MonitoringSettings());

        var routingService = new HealthAwareRoutingService(
            healthMonitor.Object, logger.Object, auditService.Object, settings);

        // Register instances with one disabled
        var enabledInstance = new ServiceInstance
        {
            ServiceName = "enabled-service",
            InstanceId = "enabled-instance",
            Host = "host1",
            Port = 8080,
            IsHealthy = true,
            IsEnabled = true,
            Status = "Active"
        };

        var disabledInstance = new ServiceInstance
        {
            ServiceName = "enabled-service",
            InstanceId = "disabled-instance",
            Host = "host2",
            Port = 8080,
            IsHealthy = true,
            IsEnabled = false,
            Status = "Active"
        };

        await routingService.RegisterServiceInstanceAsync(enabledInstance);
        await routingService.RegisterServiceInstanceAsync(disabledInstance);

        // Act
        var bestInstance = await routingService.GetBestServiceInstanceAsync("enabled-service");

        // Assert
        Assert.NotNull(bestInstance);
        Assert.Equal("enabled-instance", bestInstance.InstanceId);
        Assert.True(bestInstance.IsEnabled);
    }
}