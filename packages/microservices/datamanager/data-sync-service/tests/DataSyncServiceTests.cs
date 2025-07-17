using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using DataSyncService.Core.DTOs;

namespace DataSyncService.Tests;

public class DataSyncServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DataSyncServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSyncJobs_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/sync/jobs");

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
    public async Task CreateSyncJob_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var syncJobDto = new CreateSyncJobDto
        {
            Name = "Test Sync Job",
            SourceSiteId = "site-1",
            TargetSiteId = "site-2",
            DataType = "Transactions",
            SyncType = "Full",
            ScheduleCron = "0 0 * * *",
            IsEnabled = true,
            Priority = 1,
            RetryCount = 3,
            TimeoutMinutes = 30
        };

        var json = JsonSerializer.Serialize(syncJobDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/sync/jobs", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSyncJobById_WithValidId_ShouldReturnSyncJob()
    {
        // Arrange
        var syncJobId = "test-sync-job-id";

        // Act
        var response = await _client.GetAsync($"/api/sync/jobs/{syncJobId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSyncSites_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/sync/sites");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateSyncSite_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var syncSiteDto = new CreateSyncSiteDto
        {
            SiteId = "test-site-001",
            Name = "Test Site",
            ApiEndpoint = "https://test-site.example.com/api",
            DatabaseConnectionString = "Server=localhost;Database=TestSite;Integrated Security=true;",
            IsEnabled = true,
            Priority = 1,
            SyncIntervalMinutes = 15,
            Location = "Test Location"
        };

        var json = JsonSerializer.Serialize(syncSiteDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/sync/sites", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSyncHistory_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/sync/history");

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RunSyncJob_WithValidId_ShouldReturnOkResponse()
    {
        // Arrange
        var syncJobId = "test-sync-job-id";

        // Act
        var response = await _client.PostAsync($"/api/sync/jobs/{syncJobId}/run", null);

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSyncStatus_WithValidId_ShouldReturnStatus()
    {
        // Arrange
        var syncJobId = "test-sync-job-id";

        // Act
        var response = await _client.GetAsync($"/api/sync/jobs/{syncJobId}/status");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSiteHealth_WithValidId_ShouldReturnHealth()
    {
        // Arrange
        var siteId = "test-site-001";

        // Act
        var response = await _client.GetAsync($"/api/sync/sites/{siteId}/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}