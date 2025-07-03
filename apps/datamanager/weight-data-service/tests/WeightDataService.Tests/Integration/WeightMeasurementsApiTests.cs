using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WeightDataService.Core.DTOs;
using WeightDataService.Infrastructure.Data;
using WeightDataService.Tests.Helpers;
using Xunit;

namespace WeightDataService.Tests.Integration;

public class WeightMeasurementsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WeightMeasurementsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<WeightDataContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database for testing
                services.AddDbContext<WeightDataContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Organization-Id", "test-org");
        _client.DefaultRequestHeaders.Add("X-User-Id", "test-user");
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task PostMeasurement_ValidData_ReturnsCreated()
    {
        // Arrange
        await SeedTestData();
        var createDto = TestDataFactory.CreateWeightMeasurementDto();

        // Act
        var response = await _client.PostAsJsonAsync("/api/measurements", createDto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<WeightMeasurementDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(createDto.WeighbridgeId, apiResponse.Data.WeighbridgeId);
    }

    [Fact]
    public async Task GetMeasurements_ReturnsPagedResults()
    {
        // Arrange
        await SeedTestData();

        // Act
        var response = await _client.GetAsync("/api/measurements?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<PagedResultDto<WeightMeasurementDto>>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    [Fact]
    public async Task GetMeasurement_ExistingId_ReturnsOk()
    {
        // Arrange
        var measurementId = await SeedSingleMeasurement();

        // Act
        var response = await _client.GetAsync($"/api/measurements/{measurementId}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<WeightMeasurementDto>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
        Assert.Equal(measurementId, apiResponse.Data.Id);
    }

    [Fact]
    public async Task GetMeasurement_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/measurements/{nonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMeasurement_ExistingId_ReturnsOk()
    {
        // Arrange
        var measurementId = await SeedSingleMeasurement();

        // Act
        var response = await _client.DeleteAsync($"/api/measurements/{measurementId}");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<bool>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.True(apiResponse.Data);
    }

    [Fact]
    public async Task GetWeighbridges_ReturnsOk()
    {
        // Arrange
        await SeedTestData();

        // Act
        var response = await _client.GetAsync("/api/weighbridges");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var apiResponse = JsonSerializer.Deserialize<ApiResponseDto<List<WeighbridgeStatusDto>>>(content, options);
        
        Assert.NotNull(apiResponse);
        Assert.True(apiResponse.Success);
        Assert.NotNull(apiResponse.Data);
    }

    private async Task SeedTestData()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WeightDataContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Add test weighbridge
        var weighbridge = TestDataFactory.CreateWeighbridgeStatus("WB001", "test-org");
        context.WeighbridgeStatuses.Add(weighbridge);

        await context.SaveChangesAsync();
    }

    private async Task<Guid> SeedSingleMeasurement()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WeightDataContext>();
        
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Add test weighbridge
        var weighbridge = TestDataFactory.CreateWeighbridgeStatus("WB001", "test-org");
        context.WeighbridgeStatuses.Add(weighbridge);

        // Add test measurement
        var measurement = TestDataFactory.CreateWeightMeasurement("WB001", "ABC123", 1000, "test-org");
        context.WeightMeasurements.Add(measurement);

        await context.SaveChangesAsync();

        return measurement.Id;
    }
}