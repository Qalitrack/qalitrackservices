using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Api.Controllers;
using ProductService.Core.Interfaces;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Security;

/// <summary>
/// Tests for gateway-forwarded user context functionality.
/// In the QaliTrack hybrid auth model, the gateway handles JWT authentication
/// and forwards user context via headers to downstream services.
/// </summary>
[Trait("Category", "Security")]
public class AuthenticationTests : IClassFixture<TestWebApplicationFactory<Program>>, IDisposable
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;

    public AuthenticationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.SeedData();
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
    }

    #region Gateway Header Forwarding Tests

    [Fact]
    public async Task ProductEndpoints_ShouldAcceptValidGatewayHeaders()
    {
        // Arrange - Simulate gateway-forwarded headers
        _client.DefaultRequestHeaders.Add("X-User-ID", "12345");
        _client.DefaultRequestHeaders.Add("X-User-Name", "testuser");
        _client.DefaultRequestHeaders.Add("X-User-Email", "test@example.com");
        _client.DefaultRequestHeaders.Add("X-User-Roles", "User,Operator");
        _client.DefaultRequestHeaders.Add("X-Gateway-Authorized", "true");

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProductEndpoints_ShouldWork_WhenMinimalGatewayHeaders()
    {
        // Arrange - Only essential gateway headers
        _client.DefaultRequestHeaders.Add("X-User-ID", "12345");
        _client.DefaultRequestHeaders.Add("X-User-Roles", "User");
        _client.DefaultRequestHeaders.Add("X-Gateway-Authorized", "true");

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task ProductEndpoints_ShouldWork_WhenNoGatewayHeaders()
    {
        // Arrange - No gateway headers (service running standalone)

        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        // Service should still work without gateway headers (for testing/development)
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    #endregion

    #region BaseController Context Extraction Tests

    [Fact]
    public void BaseController_ShouldExtractUserId_FromGatewayHeaders()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-ID"] = "test-user-123"
        });

        // Act
        var userId = controller.GetUserId();

        // Assert
        userId.Should().Be("test-user-123");
    }

    [Fact]
    public void BaseController_ShouldReturnDefaultUserId_WhenHeaderMissing()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>());

        // Act
        var userId = controller.GetUserId();

        // Assert
        userId.Should().Be("system");
    }

    [Fact]
    public void BaseController_ShouldExtractUserName_FromGatewayHeaders()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Name"] = "john.doe"
        });

        // Act
        var userName = controller.GetUserName();

        // Assert
        userName.Should().Be("john.doe");
    }

    [Fact]
    public void BaseController_ShouldExtractUserEmail_FromGatewayHeaders()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Email"] = "john.doe@example.com"
        });

        // Act
        var userEmail = controller.GetUserEmail();

        // Assert
        userEmail.Should().Be("john.doe@example.com");
    }

    [Fact]
    public void BaseController_ShouldExtractUserRoles_FromGatewayHeaders()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = "User,Operator,Admin"
        });

        // Act
        var userRoles = controller.GetUserRoles();

        // Assert
        userRoles.Should().HaveCount(3);
        userRoles.Should().Contain("User");
        userRoles.Should().Contain("Operator");
        userRoles.Should().Contain("Admin");
    }

    [Fact]
    public void BaseController_ShouldReturnEmptyRoles_WhenHeaderMissing()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>());

        // Act
        var userRoles = controller.GetUserRoles();

        // Assert
        userRoles.Should().BeEmpty();
    }

    [Fact]
    public void BaseController_ShouldDetectGatewayAuthorization()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-Gateway-Authorized"] = "true"
        });

        // Act
        var isAuthorized = controller.IsGatewayAuthorized();

        // Assert
        isAuthorized.Should().BeTrue();
    }

    [Fact]
    public void BaseController_ShouldReturnFalse_WhenGatewayAuthorizationMissing()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>());

        // Act
        var isAuthorized = controller.IsGatewayAuthorized();

        // Assert
        isAuthorized.Should().BeFalse();
    }

    #endregion

    #region Role Checking Tests

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("SuperAdmin", true)]
    [InlineData("Operator", false)]
    [InlineData("User", false)]
    public void BaseController_ShouldCheckAdminRole_Correctly(string role, bool expectedIsAdmin)
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = role
        });

        // Act
        var isAdmin = controller.IsAdmin();

        // Assert
        isAdmin.Should().Be(expectedIsAdmin);
    }

    [Theory]
    [InlineData("User", false)]
    [InlineData("Operator", true)]
    [InlineData("SiteManager", true)]
    [InlineData("Admin", true)]
    [InlineData("SuperAdmin", true)]
    public void BaseController_ShouldCheckOperatorOrHigher_Correctly(string role, bool expectedIsOperatorOrHigher)
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = role
        });

        // Act
        var isOperatorOrHigher = controller.IsOperatorOrHigher();

        // Assert
        isOperatorOrHigher.Should().Be(expectedIsOperatorOrHigher);
    }

    [Fact]
    public void BaseController_ShouldCheckMultipleRoles_Correctly()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = "User,Operator"
        });

        // Act
        var hasUserRole = controller.HasAnyRole("User");
        var hasOperatorRole = controller.HasAnyRole("Operator");
        var hasAdminRole = controller.HasAnyRole("Admin");
        var hasAnyRole = controller.HasAnyRole("User", "Admin");

        // Assert
        hasUserRole.Should().BeTrue();
        hasOperatorRole.Should().BeTrue();
        hasAdminRole.Should().BeFalse();
        hasAnyRole.Should().BeTrue();
    }

    #endregion

    #region Header Format Tests

    [Fact]
    public void BaseController_ShouldHandleWhitespace_InRolesHeader()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = " User , Operator , Admin "
        });

        // Act
        var userRoles = controller.GetUserRoles();

        // Assert
        userRoles.Should().HaveCount(3);
        userRoles.Should().Contain("User");
        userRoles.Should().Contain("Operator");
        userRoles.Should().Contain("Admin");
    }

    [Fact]
    public void BaseController_ShouldHandleEmptyRoles_Header()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = ""
        });

        // Act
        var userRoles = controller.GetUserRoles();

        // Assert
        userRoles.Should().BeEmpty();
    }

    [Fact]
    public void BaseController_ShouldHandleSingleRole_Header()
    {
        // Arrange
        var controller = CreateControllerWithHeaders(new Dictionary<string, string>
        {
            ["X-User-Roles"] = "User"
        });

        // Act
        var userRoles = controller.GetUserRoles();

        // Assert
        userRoles.Should().HaveCount(1);
        userRoles.Should().Contain("User");
    }

    #endregion

    #region Helper Methods

    private TestBaseController CreateControllerWithHeaders(Dictionary<string, string> headers)
    {
        var controller = new TestBaseController();
        var httpContext = new DefaultHttpContext();
        
        foreach (var header in headers)
        {
            httpContext.Request.Headers[header.Key] = header.Value;
        }
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
        
        return controller;
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
        _scope?.Dispose();
    }
}

/// <summary>
/// Test controller that exposes BaseController methods for testing
/// </summary>
public class TestBaseController : BaseController
{
    public new string GetUserId() => base.GetUserId();
    public new string GetUserName() => base.GetUserName();
    public new string GetUserEmail() => base.GetUserEmail();
    public new List<string> GetUserRoles() => base.GetUserRoles();
    public new bool HasAnyRole(params string[] roles) => base.HasAnyRole(roles);
    public new bool IsAdmin() => base.IsAdmin();
    public new bool IsOperatorOrHigher() => base.IsOperatorOrHigher();
    public new bool IsGatewayAuthorized() => base.IsGatewayAuthorized();
}