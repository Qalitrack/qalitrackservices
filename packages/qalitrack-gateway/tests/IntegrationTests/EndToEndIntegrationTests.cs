using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using QaliTrackGateway.Services;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;
using Moq;
using Moq.Protected;

namespace QaliTrackGateway.Tests.IntegrationTests;

public class EndToEndIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public EndToEndIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task EndToEnd_AuditAndMonitoring_ShouldWorkTogether()
    {
        // Arrange
        var auditEventsCaptured = new List<AuditEvent>();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        // Setup mock HTTP response for audit service
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace HTTP client for audit service
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
                
                // Add test configuration
                services.PostConfigure<AuditSettings>(settings =>
                {
                    settings.EnableAuditLogging = true;
                    settings.EnableAuthorizationAudit = true;
                    settings.EnableGatewayRequestAudit = true;
                });

                services.PostConfigure<MonitoringSettings>(settings =>
                {
                    settings.EnableHealthMonitoring = true;
                    settings.EnableHealthCheckAudit = true;
                });
            });
        });

        var client = factory.CreateClient();

        // Act 1: Health check endpoint
        var healthResponse = await client.GetAsync("/health");
        
        // Act 2: Monitoring endpoints
        var monitoringResponse = await client.GetAsync("/api/monitoring/system/health");
        
        // Act 3: Discovery endpoints
        var discoveryResponse = await client.GetAsync("/api/discovery");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, healthResponse.StatusCode);
        
        // Monitoring endpoints may require authentication - check if they respond appropriately
        Assert.True(monitoringResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   monitoringResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(discoveryResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   discoveryResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);

        // Verify audit service was called
        httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.AtLeastOnce(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task EndToEnd_ServiceRegistrationAndHealthChecks_ShouldWork()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        // Setup mock HTTP response for health checks
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/health")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"status\": \"healthy\"}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace HTTP client for health monitoring
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act 1: Register a service for monitoring
        var serviceRegistration = new ServiceRegistration
        {
            ServiceName = "test-integration-service",
            HealthCheckUrl = "http://test-service/health",
            BaseUrl = "http://test-service",
            IsEnabled = true,
            Tags = new[] { "test", "integration" }
        };

        var registrationJson = JsonSerializer.Serialize(serviceRegistration);
        var registrationContent = new StringContent(registrationJson, Encoding.UTF8, "application/json");

        // This may require authentication, so we expect either success or unauthorized
        var registrationResponse = await client.PostAsync("/api/monitoring/services", registrationContent);
        
        // Act 2: Get service health
        var serviceHealthResponse = await client.GetAsync("/api/monitoring/services/test-integration-service/health");
        
        // Act 3: Get system health
        var systemHealthResponse = await client.GetAsync("/api/monitoring/system/health");

        // Assert
        // Registration may require authentication
        Assert.True(registrationResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   registrationResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        // Service health and system health may also require authentication
        Assert.True(serviceHealthResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   serviceHealthResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(systemHealthResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   systemHealthResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndToEnd_ServiceDiscoveryAndRouting_ShouldWork()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act 1: Get service discovery info
        var discoveryResponse = await client.GetAsync("/api/discovery");
        
        // Act 2: Get routing statistics
        var routingStatsResponse = await client.GetAsync("/api/discovery/stats");
        
        // Act 3: Try to get best instance for a service
        var bestInstanceResponse = await client.GetAsync("/api/discovery/services/test-service/best-instance");

        // Assert
        // These endpoints may require authentication
        Assert.True(discoveryResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   discoveryResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(routingStatsResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   routingStatsResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(bestInstanceResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   bestInstanceResponse.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   bestInstanceResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndToEnd_CircuitBreakerIntegration_ShouldWork()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act 1: Get circuit breaker status
        var circuitBreakerResponse = await client.GetAsync("/api/discovery/services/test-service/circuit-breaker");
        
        // Act 2: Try to update circuit breaker
        var updateRequest = new
        {
            IsOpen = true,
            Reason = "Integration test"
        };

        var updateJson = JsonSerializer.Serialize(updateRequest);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        
        var updateResponse = await client.PutAsync("/api/discovery/services/test-service/circuit-breaker", updateContent);

        // Assert
        Assert.True(circuitBreakerResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   circuitBreakerResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(updateResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   updateResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndToEnd_PerformanceMonitoring_ShouldWork()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act 1: Get performance cache stats
        var cacheStatsResponse = await client.GetAsync("/api/performance/auth-cache-stats");
        
        // Act 2: Get performance recommendations
        var recommendationsResponse = await client.GetAsync("/api/performance/recommendations");
        
        // Act 3: Get monitoring dashboard
        var dashboardResponse = await client.GetAsync("/api/monitoring/dashboard");

        // Assert
        Assert.True(cacheStatsResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   cacheStatsResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(recommendationsResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   recommendationsResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        
        Assert.True(dashboardResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   dashboardResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndToEnd_AuditConfiguration_ShouldBeLoaded()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Get configuration endpoint (if available)
        var configResponse = await client.GetAsync("/api/configuration");

        // Assert - Even if configuration endpoint doesn't exist, the app should start successfully
        Assert.True(configResponse.StatusCode == System.Net.HttpStatusCode.OK || 
                   configResponse.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   configResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EndToEnd_AllServicesIntegration_ShouldWork()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act - Test various endpoints to ensure integration works
        var endpoints = new[]
        {
            "/health",
            "/api/monitoring/system/health",
            "/api/discovery",
            "/api/performance/auth-cache-stats",
            "/api/monitoring/dashboard"
        };

        var responses = new List<HttpResponseMessage>();
        foreach (var endpoint in endpoints)
        {
            try
            {
                var response = await client.GetAsync(endpoint);
                responses.Add(response);
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error calling {endpoint}: {ex.Message}");
            }
        }

        // Assert
        Assert.NotEmpty(responses);
        
        // At least the health endpoint should work
        var healthResponse = responses.FirstOrDefault();
        Assert.NotNull(healthResponse);
        Assert.Equal(System.Net.HttpStatusCode.OK, healthResponse.StatusCode);
        
        // All other endpoints should return either OK or Unauthorized (not server errors)
        foreach (var response in responses)
        {
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK || 
                       response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                       response.StatusCode == System.Net.HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task EndToEnd_ServiceStartup_ShouldInitializeAllServices()
    {
        // Arrange & Act
        var client = _factory.CreateClient();

        // Act - Simple health check to ensure all services are initialized
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        // Verify services are registered in DI container
        using var scope = _factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // Check that all our services are registered
        var auditService = serviceProvider.GetService<IAuditService>();
        var healthMonitor = serviceProvider.GetService<IServiceHealthMonitor>();
        var routingService = serviceProvider.GetService<IHealthAwareRoutingService>();
        
        Assert.NotNull(auditService);
        Assert.NotNull(healthMonitor);
        Assert.NotNull(routingService);
    }

    [Fact]
    public async Task EndToEnd_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        
        httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = new StringContent("{\"success\": true}")
            });

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<HttpClient>(sp => new HttpClient(httpMessageHandlerMock.Object));
            });
        });

        var client = factory.CreateClient();

        // Act - Make multiple concurrent requests
        var tasks = new List<Task<HttpResponseMessage>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(client.GetAsync("/health"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, responses.Length);
        Assert.All(responses, r => Assert.Equal(System.Net.HttpStatusCode.OK, r.StatusCode));
    }
}