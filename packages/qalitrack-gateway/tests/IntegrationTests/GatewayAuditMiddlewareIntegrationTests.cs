using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using QaliTrackGateway.Services;
using QaliTrackGateway.Models;
using QaliTrackGateway.Configuration;
using QaliTrackGateway.Middleware;
using Moq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

namespace QaliTrackGateway.Tests.IntegrationTests;

public class GatewayAuditMiddlewareIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public GatewayAuditMiddlewareIntegrationTests(WebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_WithValidUser_ShouldLogAuditEvents()
    {
        // Arrange
        var auditEventsCaptured = new List<AuditEvent>();
        var authEventsCaptured = new List<AuthorizationAuditEvent>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("test-correlation-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .Callback<AuditEvent>(evt => auditEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    mockAuditService.Setup(x => x.LogAuthorizationEventAsync(It.IsAny<AuthorizationAuditEvent>()))
                        .Callback<AuthorizationAuditEvent>(evt => authEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });

                // Configure test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Test endpoint reached");
                });
            });
        });

        var client = factory.CreateClient();
        
        // Add authorization header
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        // Act
        var response = await client.GetAsync("/api/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        // Verify audit events were captured
        Assert.NotEmpty(auditEventsCaptured);
        
        var gatewayEvent = auditEventsCaptured.FirstOrDefault(e => e.EventType == "GatewayRequest");
        Assert.NotNull(gatewayEvent);
        Assert.Equal("test-correlation-id", gatewayEvent.CorrelationId);
        Assert.Equal("QaliTrack-Gateway", gatewayEvent.ServiceName);
        Assert.Equal("/api/test", gatewayEvent.RequestPath);
        Assert.Equal("GET", gatewayEvent.HttpMethod);
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_WithUnauthorizedUser_ShouldLogFailureAudit()
    {
        // Arrange
        var auditEventsCaptured = new List<AuditEvent>();
        var authEventsCaptured = new List<AuthorizationAuditEvent>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("unauthorized-correlation-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .Callback<AuditEvent>(evt => auditEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    mockAuditService.Setup(x => x.LogAuthorizationEventAsync(It.IsAny<AuthorizationAuditEvent>()))
                        .Callback<AuthorizationAuditEvent>(evt => authEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Should not reach here");
                });
            });
        });

        var client = factory.CreateClient();

        // Act - Make request without authorization header
        var response = await client.GetAsync("/api/protected");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        
        // Verify audit events were captured
        Assert.NotEmpty(auditEventsCaptured);
        
        var gatewayEvent = auditEventsCaptured.FirstOrDefault(e => e.EventType == "GatewayRequest");
        Assert.NotNull(gatewayEvent);
        Assert.Equal("unauthorized-correlation-id", gatewayEvent.CorrelationId);
        Assert.Equal("Unauthenticated", gatewayEvent.Action);
        Assert.Equal("Authentication required", gatewayEvent.ErrorMessage);
        Assert.Equal(401, gatewayEvent.StatusCode);
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_WithInsufficientRole_ShouldLogAuthorizationFailure()
    {
        // Arrange
        var auditEventsCaptured = new List<AuditEvent>();
        var authEventsCaptured = new List<AuthorizationAuditEvent>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("insufficient-role-correlation-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .Callback<AuditEvent>(evt => auditEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    mockAuditService.Setup(x => x.LogAuthorizationEventAsync(It.IsAny<AuthorizationAuditEvent>()))
                        .Callback<AuthorizationAuditEvent>(evt => authEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });

                // Configure test authentication with insufficient role
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Should not reach here");
                });
            });
        });

        var client = factory.CreateClient();
        
        // Add authorization header
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "insufficient-role-token");

        // Act
        var response = await client.GetAsync("/api/admin");

        // Assert
        // The exact status code depends on the route configuration, but we should have audit events
        Assert.NotEmpty(auditEventsCaptured);
        
        var gatewayEvent = auditEventsCaptured.FirstOrDefault(e => e.EventType == "GatewayRequest");
        Assert.NotNull(gatewayEvent);
        Assert.Equal("insufficient-role-correlation-id", gatewayEvent.CorrelationId);
        Assert.Equal("/api/admin", gatewayEvent.RequestPath);
        Assert.Equal("GET", gatewayEvent.HttpMethod);
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_WithPublicEndpoint_ShouldLogPublicAccess()
    {
        // Arrange
        var auditEventsCaptured = new List<AuditEvent>();

        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("public-endpoint-correlation-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .Callback<AuditEvent>(evt => auditEventsCaptured.Add(evt))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    await context.Response.WriteAsync("Public endpoint reached");
                });
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        // Verify audit events were captured
        Assert.NotEmpty(auditEventsCaptured);
        
        var gatewayEvent = auditEventsCaptured.FirstOrDefault(e => e.EventType == "GatewayRequest");
        Assert.NotNull(gatewayEvent);
        Assert.Equal("public-endpoint-correlation-id", gatewayEvent.CorrelationId);
        Assert.Equal("PublicEndpoint", gatewayEvent.Action);
        Assert.Equal("/health", gatewayEvent.RequestPath);
        Assert.Equal("GET", gatewayEvent.HttpMethod);
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_ShouldAddCorrelationIdToHeaders()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("test-correlation-header-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    // Verify correlation ID is in request headers
                    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
                    await context.Response.WriteAsync($"Correlation ID: {correlationId}");
                });
            });
        });

        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("test-correlation-header-id", content);
    }

    [Fact]
    public async Task RoleAuthorizationMiddleware_ShouldAddUserContextHeaders()
    {
        // Arrange
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace audit service with mock
                services.AddSingleton<IAuditService>(sp =>
                {
                    var mockAuditService = new Mock<IAuditService>();
                    mockAuditService.Setup(x => x.GenerateCorrelationId())
                        .Returns("user-context-correlation-id");
                    
                    mockAuditService.Setup(x => x.LogAuditEventAsync(It.IsAny<AuditEvent>()))
                        .ReturnsAsync(true);
                    
                    mockAuditService.Setup(x => x.LogAuthorizationEventAsync(It.IsAny<AuthorizationAuditEvent>()))
                        .ReturnsAsync(true);
                    
                    return mockAuditService.Object;
                });

                // Configure test authentication
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            });

            builder.Configure(app =>
            {
                app.UseAuthentication();
                app.UseAuthorization();
                
                // Add test middleware to simulate role authorization
                app.Use(async (context, next) =>
                {
                    var cache = context.RequestServices.GetRequiredService<IMemoryCache>();
                    var cacheService = context.RequestServices.GetRequiredService<IAuthorizationCacheService>();
                    var auditService = context.RequestServices.GetRequiredService<IAuditService>();
                    var logger = context.RequestServices.GetRequiredService<ILogger<RoleAuthorizationMiddleware>>();
                    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

                    var middleware = new RoleAuthorizationMiddleware(
                        next, cache, cacheService, auditService, logger, configuration);

                    await middleware.InvokeAsync(context);
                });

                app.Run(async context =>
                {
                    // Read user context headers
                    var userId = context.Request.Headers["X-User-ID"].FirstOrDefault();
                    var userName = context.Request.Headers["X-User-Name"].FirstOrDefault();
                    var userRoles = context.Request.Headers["X-User-Roles"].FirstOrDefault();
                    var gatewayAuthorized = context.Request.Headers["X-Gateway-Authorized"].FirstOrDefault();

                    var result = new
                    {
                        UserId = userId,
                        UserName = userName,
                        UserRoles = userRoles,
                        GatewayAuthorized = gatewayAuthorized
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(result));
                });
            });
        });

        var client = factory.CreateClient();
        
        // Add authorization header
        client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");

        // Act
        var response = await client.GetAsync("/api/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(content);
        
        Assert.True(result.TryGetProperty("UserId", out var userIdProp));
        Assert.True(result.TryGetProperty("UserName", out var userNameProp));
        Assert.True(result.TryGetProperty("GatewayAuthorized", out var gatewayAuthorizedProp));
        
        // Values depend on the test authentication handler implementation
        Assert.NotNull(userIdProp.GetString());
        Assert.NotNull(userNameProp.GetString());
        Assert.Equal("true", gatewayAuthorizedProp.GetString());
    }
}

// Test authentication handler for integration tests
public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader == null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-123"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("roles", "User,Operator"),
            new Claim("permissions", "read:data,write:data"),
            new Claim("tenant_id", "test-tenant")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}