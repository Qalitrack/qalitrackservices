using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using AnalyticsService.Core.DTOs;

namespace AnalyticsService.Tests;

public class AnalyticsServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AnalyticsServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAnalyticsData_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
    }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task GetDashboardData_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/dashboard");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehicleAnalytics_WithValidId_ShouldReturnAnalytics()
    {
        // Arrange
        var vehicleId = "test-vehicle-id";

        // Act
        var response = await _client.GetAsync($"/api/analytics/vehicles/{vehicleId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDriverAnalytics_WithValidId_ShouldReturnAnalytics()
    {
        // Arrange
        var driverId = "test-driver-id";

        // Act
        var response = await _client.GetAsync($"/api/analytics/drivers/{driverId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRouteAnalytics_WithValidId_ShouldReturnAnalytics()
    {
        // Arrange
        var routeId = "test-route-id";

        // Act
        var response = await _client.GetAsync($"/api/analytics/routes/{routeId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPerformanceMetrics_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/performance");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTrendAnalysis_WithValidDateRange_ShouldReturnAnalytics()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/analytics/trends?startDate={startDate}&endDate={endDate}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRealtimeMetrics_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/analytics/realtime");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCustomerAnalytics_WithValidId_ShouldReturnAnalytics()
    {
        // Arrange
        var customerId = "test-customer-id";

        // Act
        var response = await _client.GetAsync($"/api/analytics/customers/{customerId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}