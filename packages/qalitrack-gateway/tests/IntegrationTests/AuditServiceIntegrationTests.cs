using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

public class AuditServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _mockHttpClient;

    public AuditServiceIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        
        // Setup mock HTTP client for transaction service calls
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _mockHttpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }

    [Fact]
    public async Task LogAuditEventAsync_WithValidEvent_ShouldSucceed()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = true,
            TransactionServiceBaseUrl = "http://test-transaction-service",
            AuditEndpoint = "/api/audit/gateway",
            EnableBatchProcessing = false
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);

        // Setup mock HTTP response
        _httpMessageHandlerMock
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

        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var auditEvent = new AuditEvent
        {
            EventType = "GatewayRequest",
            UserId = "test-user-123",
            UserName = "Test User",
            ServiceName = "test-service",
            Action = "GET",
            Resource = "/api/test",
            IpAddress = "127.0.0.1",
            RequestPath = "/api/test",
            HttpMethod = "GET",
            StatusCode = 200
        };

        // Act
        var result = await auditService.LogAuditEventAsync(auditEvent);

        // Assert
        Assert.True(result);
        
        // Verify HTTP call was made
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("/api/audit/gateway")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task LogBatchAuditEventsAsync_WithMultipleEvents_ShouldSendBatch()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = true,
            TransactionServiceBaseUrl = "http://test-transaction-service",
            BatchAuditEndpoint = "/api/audit/gateway/batch",
            EnableBatchProcessing = true
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);

        _httpMessageHandlerMock
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

        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var auditEvents = new List<AuditEvent>
        {
            new AuditEvent
            {
                EventType = "GatewayRequest",
                UserId = "user-1",
                Action = "GET",
                Resource = "/api/test1"
            },
            new AuditEvent
            {
                EventType = "GatewayRequest",
                UserId = "user-2",
                Action = "POST",
                Resource = "/api/test2"
            }
        };

        // Act
        var result = await auditService.LogBatchAuditEventsAsync(auditEvents);

        // Assert
        Assert.True(result);
        
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Post &&
                req.RequestUri.ToString().Contains("/api/audit/gateway/batch")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task LogAuthorizationEventAsync_WithValidAuthEvent_ShouldSucceed()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = true,
            EnableAuthorizationAudit = true,
            TransactionServiceBaseUrl = "http://test-transaction-service",
            AuditEndpoint = "/api/audit/gateway"
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var authEvent = new AuthorizationAuditEvent
        {
            UserId = "test-user",
            UserName = "Test User",
            ServiceName = "test-service",
            Resource = "/api/protected",
            RequiredRoles = new[] { "Admin" },
            UserRoles = new[] { "User" },
            AuthorizationResult = false,
            AuthorizationReason = "Insufficient role privileges"
        };

        // Act
        var result = await auditService.LogAuthorizationEventAsync(authEvent);

        // Assert
        Assert.True(result);
        Assert.Equal("Authorization", authEvent.EventType);
        Assert.Equal("Deny", authEvent.Action);
    }

    [Fact]
    public void GenerateCorrelationId_ShouldReturnValidFormat()
    {
        // Arrange
        var auditSettings = new AuditSettings();
        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);
        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        // Act
        var correlationId = auditService.GenerateCorrelationId();

        // Assert
        Assert.NotEmpty(correlationId);
        Assert.StartsWith("gtw-", correlationId);
        Assert.Contains(DateTime.UtcNow.ToString("yyyyMMdd"), correlationId);
    }

    [Fact]
    public async Task LogAuditEventAsync_WhenAuditingDisabled_ShouldReturnTrue()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = false
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);
        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var auditEvent = new AuditEvent
        {
            EventType = "GatewayRequest",
            UserId = "test-user"
        };

        // Act
        var result = await auditService.LogAuditEventAsync(auditEvent);

        // Assert
        Assert.True(result);
        
        // Verify no HTTP call was made
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task LogAuditEventAsync_WhenHttpCallFails_ShouldReturnFalse()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = true,
            TransactionServiceBaseUrl = "http://test-transaction-service",
            AuditEndpoint = "/api/audit/gateway"
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            });

        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var auditEvent = new AuditEvent
        {
            EventType = "GatewayRequest",
            UserId = "test-user"
        };

        // Act
        var result = await auditService.LogAuditEventAsync(auditEvent);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task LogHealthCheckEventAsync_WithValidEvent_ShouldSucceed()
    {
        // Arrange
        var auditSettings = new AuditSettings
        {
            EnableAuditLogging = true,
            EnableHealthCheckAudit = true,
            TransactionServiceBaseUrl = "http://test-transaction-service",
            AuditEndpoint = "/api/audit/gateway"
        };

        var logger = new Mock<ILogger<AuditService>>();
        var options = Options.Create(auditSettings);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            });

        var auditService = new AuditService(_mockHttpClient, logger.Object, options);

        var healthEvent = new HealthCheckAuditEvent
        {
            CheckedServices = new[] { "service1", "service2" },
            HealthyServices = new[] { "service1" },
            UnhealthyServices = new[] { "service2" },
            TotalServices = 2,
            HealthyCount = 1,
            OverallHealthScore = 50.0
        };

        // Act
        var result = await auditService.LogHealthCheckEventAsync(healthEvent);

        // Assert
        Assert.True(result);
        Assert.Equal("HealthCheck", healthEvent.EventType);
        Assert.Equal("Monitor", healthEvent.Action);
    }
}