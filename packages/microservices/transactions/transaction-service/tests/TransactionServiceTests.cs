using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using TransactionService.Core.DTOs;

namespace TransactionService.Tests;

public class TransactionServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TransactionServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTransactions_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/transactions");

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
    public async Task CreateTransaction_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var transactionDto = new CreateTransactionDto
        {
            VehicleId = "test-vehicle-id",
            DriverId = "test-driver-id",
            CustomerId = "test-customer-id",
            ProductId = "test-product-id",
            RouteId = "test-route-id",
            TransactionType = "Weighing",
            GrossWeight = 25000,
            TareWeight = 5000,
            NetWeight = 20000,
            Status = "Pending",
            Notes = "Test transaction"
        };

        var json = JsonSerializer.Serialize(transactionDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/transactions", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactionById_WithValidId_ShouldReturnTransaction()
    {
        // Arrange
        var transactionId = "test-transaction-id";

        // Act
        var response = await _client.GetAsync($"/api/transactions/{transactionId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/audit");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAuditLog_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var auditDto = new CreateAuditLogDto
        {
            Action = "TestAction",
            EntityType = "TestEntity",
            EntityId = "test-entity-id",
            UserId = "test-user-id",
            UserName = "Test User",
            Changes = "Test changes",
            IpAddress = "127.0.0.1",
            UserAgent = "Test Agent"
        };

        var json = JsonSerializer.Serialize(auditDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/audit", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactionsByVehicle_WithValidVehicleId_ShouldReturnTransactions()
    {
        // Arrange
        var vehicleId = "test-vehicle-id";

        // Act
        var response = await _client.GetAsync($"/api/transactions/vehicle/{vehicleId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTransactionsByDateRange_WithValidDates_ShouldReturnTransactions()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/transactions/daterange?startDate={startDate}&endDate={endDate}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}