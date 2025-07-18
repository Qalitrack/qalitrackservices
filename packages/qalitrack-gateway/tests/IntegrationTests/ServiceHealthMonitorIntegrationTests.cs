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
using Moq.Protected;

namespace QaliTrackGateway.Tests.IntegrationTests;

public class ServiceHealthMonitorIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _mockHttpClient;

    public ServiceHealthMonitorIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    [Fact]
    public async Task RegisterServiceAsync_WithValidService_ShouldSucceed()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultHealthCheckIntervalSeconds = 30,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        var serviceRegistration = new ServiceRegistration
        {
            ServiceName = "test-service",
            HealthCheckUrl = "http://test-service/health",
            BaseUrl = "http://test-service",
            IsEnabled = true,
            Tags = new[] { "test", "integration" }
        };

        // Act
        await healthMonitor.RegisterServiceAsync(serviceRegistration);

        // Assert
        var serviceHealth = await healthMonitor.GetServiceHealthAsync("test-service");
        Assert.NotNull(serviceHealth);
        Assert.Equal("test-service", serviceHealth.ServiceName);
        Assert.Equal("http://test-service/health", serviceHealth.HealthCheckUrl);
        Assert.True(serviceHealth.IsEnabled);
        Assert.Contains("test", serviceHealth.Tags);
    }

    [Fact]
    public async Task GetServiceHealthAsync_WithHealthyService_ShouldReturnHealthyStatus()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        // Setup healthy response
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"status\": \"healthy\"}")
            });

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        var serviceRegistration = new ServiceRegistration
        {
            ServiceName = "healthy-service",
            HealthCheckUrl = "http://healthy-service/health",
            TimeoutSeconds = 5,
            IsEnabled = true
        };

        await healthMonitor.RegisterServiceAsync(serviceRegistration);

        // Act
        var result = await healthMonitor.GetServiceHealthAsync("healthy-service");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("healthy-service", result.ServiceName);
        Assert.Equal("Healthy", result.Status);
        Assert.Equal(100.0, result.HealthScore);
        Assert.Equal(0, result.ConsecutiveFailures);
        Assert.NotNull(result.LastHealthyTime);
    }

    [Fact]
    public async Task GetServiceHealthAsync_WithUnhealthyService_ShouldReturnUnhealthyStatus()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        // Setup unhealthy response
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
                ReasonPhrase = "Internal Server Error"
            });

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        var serviceRegistration = new ServiceRegistration
        {
            ServiceName = "unhealthy-service",
            HealthCheckUrl = "http://unhealthy-service/health",
            TimeoutSeconds = 5,
            IsEnabled = true
        };

        await healthMonitor.RegisterServiceAsync(serviceRegistration);

        // Act
        var result = await healthMonitor.GetServiceHealthAsync("unhealthy-service");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("unhealthy-service", result.ServiceName);
        Assert.Equal("Unhealthy", result.Status);
        Assert.Equal(0.0, result.HealthScore);
        Assert.True(result.ConsecutiveFailures > 0);
        Assert.Contains("HTTP InternalServerError", result.ErrorMessage);
    }

    [Fact]
    public async Task GetServiceHealthAsync_WithSlowService_ShouldReturnDegradedStatus()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10,
            DegradedResponseTimeMs = 1000 // 1 second threshold
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        // Setup slow response
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(1500); // Simulate slow response
                return new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent("{\"status\": \"healthy\"}")
                };
            });

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        var serviceRegistration = new ServiceRegistration
        {
            ServiceName = "slow-service",
            HealthCheckUrl = "http://slow-service/health",
            TimeoutSeconds = 5,
            IsEnabled = true
        };

        await healthMonitor.RegisterServiceAsync(serviceRegistration);

        // Act
        var result = await healthMonitor.GetServiceHealthAsync("slow-service");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("slow-service", result.ServiceName);
        Assert.Equal("Degraded", result.Status);
        Assert.Equal(75.0, result.HealthScore);
        Assert.True(result.ResponseTimeMs > 1000);
    }

    [Fact]
    public async Task GetSystemHealthScoreAsync_WithMixedServices_ShouldCalculateCorrectScore()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register multiple services
        var services = new[]
        {
            new ServiceRegistration { ServiceName = "service1", HealthCheckUrl = "http://service1/health", IsEnabled = true },
            new ServiceRegistration { ServiceName = "service2", HealthCheckUrl = "http://service2/health", IsEnabled = true },
            new ServiceRegistration { ServiceName = "service3", HealthCheckUrl = "http://service3/health", IsEnabled = true }
        };

        foreach (var service in services)
        {
            await healthMonitor.RegisterServiceAsync(service);
        }

        // Setup mixed responses
        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK }) // service1 healthy
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK }) // service2 healthy
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError }); // service3 unhealthy

        // Trigger health checks
        await healthMonitor.GetServiceHealthAsync("service1");
        await healthMonitor.GetServiceHealthAsync("service2");
        await healthMonitor.GetServiceHealthAsync("service3");

        // Act
        var systemHealth = await healthMonitor.GetSystemHealthScoreAsync();

        // Assert
        Assert.NotNull(systemHealth);
        Assert.Equal(3, systemHealth.TotalServices);
        Assert.Equal(2, systemHealth.HealthyServices);
        Assert.Equal(1, systemHealth.UnhealthyServices);
        Assert.Equal(0, systemHealth.DegradedServices);
        Assert.Equal(66.67, systemHealth.OverallScore, 1); // 2/3 * 100 = 66.67
        Assert.Equal("Degraded", systemHealth.SystemStatus);
        Assert.Single(systemHealth.CriticalIssues);
    }

    [Fact]
    public async Task GetAllServicesHealthAsync_WithRegisteredServices_ShouldReturnAllStatuses()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register services
        var services = new[]
        {
            new ServiceRegistration { ServiceName = "service-a", HealthCheckUrl = "http://service-a/health", IsEnabled = true },
            new ServiceRegistration { ServiceName = "service-b", HealthCheckUrl = "http://service-b/health", IsEnabled = true }
        };

        foreach (var service in services)
        {
            await healthMonitor.RegisterServiceAsync(service);
        }

        // Act
        var allServices = await healthMonitor.GetAllServicesHealthAsync();

        // Assert
        Assert.NotNull(allServices);
        Assert.Equal(2, allServices.Count());
        Assert.Contains(allServices, s => s.ServiceName == "service-a");
        Assert.Contains(allServices, s => s.ServiceName == "service-b");
    }

    [Fact]
    public async Task GetUnhealthyServicesAsync_WithMixedServices_ShouldReturnOnlyUnhealthy()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register services
        await healthMonitor.RegisterServiceAsync(new ServiceRegistration
        {
            ServiceName = "healthy-service",
            HealthCheckUrl = "http://healthy-service/health",
            IsEnabled = true
        });

        await healthMonitor.RegisterServiceAsync(new ServiceRegistration
        {
            ServiceName = "unhealthy-service",
            HealthCheckUrl = "http://unhealthy-service/health",
            IsEnabled = true
        });

        // Setup responses
        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK }) // healthy
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.InternalServerError }); // unhealthy

        // Trigger health checks
        await healthMonitor.GetServiceHealthAsync("healthy-service");
        await healthMonitor.GetServiceHealthAsync("unhealthy-service");

        // Act
        var unhealthyServices = await healthMonitor.GetUnhealthyServicesAsync();

        // Assert
        Assert.NotNull(unhealthyServices);
        Assert.Single(unhealthyServices);
        Assert.Equal("unhealthy-service", unhealthyServices.First().ServiceName);
        Assert.Equal("Unhealthy", unhealthyServices.First().Status);
    }

    [Fact]
    public async Task UnregisterServiceAsync_WithExistingService_ShouldRemoveService()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register service
        await healthMonitor.RegisterServiceAsync(new ServiceRegistration
        {
            ServiceName = "temp-service",
            HealthCheckUrl = "http://temp-service/health",
            IsEnabled = true
        });

        // Verify service exists
        var servicesBefore = await healthMonitor.GetAllServicesHealthAsync();
        Assert.Contains(servicesBefore, s => s.ServiceName == "temp-service");

        // Act
        await healthMonitor.UnregisterServiceAsync("temp-service");

        // Assert
        var servicesAfter = await healthMonitor.GetAllServicesHealthAsync();
        Assert.DoesNotContain(servicesAfter, s => s.ServiceName == "temp-service");
    }

    [Fact]
    public async Task GetServiceHealthHistoryAsync_WithTrackedService_ShouldReturnHistory()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register service
        await healthMonitor.RegisterServiceAsync(new ServiceRegistration
        {
            ServiceName = "tracked-service",
            HealthCheckUrl = "http://tracked-service/health",
            IsEnabled = true
        });

        // Setup response
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

        // Trigger multiple health checks
        await healthMonitor.GetServiceHealthAsync("tracked-service");
        await Task.Delay(100); // Small delay to ensure different timestamps
        await healthMonitor.GetServiceHealthAsync("tracked-service");

        // Act
        var history = await healthMonitor.GetServiceHealthHistoryAsync("tracked-service", 1);

        // Assert
        Assert.NotNull(history);
        Assert.True(history.Count() >= 1);
        Assert.All(history, h => Assert.Equal("tracked-service", h.ServiceName));
        Assert.All(history, h => Assert.True(h.CheckTime <= DateTime.UtcNow));
    }

    [Fact]
    public async Task GetServicePerformanceAsync_WithServiceHistory_ShouldReturnMetrics()
    {
        // Arrange
        var monitoringSettings = new MonitoringSettings
        {
            EnableHealthMonitoring = true,
            DefaultTimeoutSeconds = 10
        };

        var logger = new Mock<ILogger<ServiceHealthMonitor>>();
        var auditService = new Mock<IAuditService>();
        var options = Options.Create(monitoringSettings);

        var healthMonitor = new ServiceHealthMonitor(_mockHttpClient, logger.Object, auditService.Object, options);

        // Register service
        await healthMonitor.RegisterServiceAsync(new ServiceRegistration
        {
            ServiceName = "perf-service",
            HealthCheckUrl = "http://perf-service/health",
            IsEnabled = true
        });

        // Setup response
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

        // Trigger health check to generate performance data
        await healthMonitor.GetServiceHealthAsync("perf-service");

        // Act
        var performance = await healthMonitor.GetServicePerformanceAsync("perf-service");

        // Assert
        Assert.NotNull(performance);
        Assert.Equal("perf-service", performance.ServiceName);
        Assert.True(performance.PeriodStart <= performance.PeriodEnd);
        Assert.True(performance.PeriodEnd <= DateTime.UtcNow);
    }
}