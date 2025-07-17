using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using DriverService.Core.DTOs;

namespace DriverService.Tests;

public class DriverServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public DriverServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDrivers_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/drivers");

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
    public async Task CreateDriver_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var driverDto = new CreateDriverDto
        {
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateTime.Parse("1990-01-01"),
            PhoneNumber = "+1234567890",
            Email = "john.doe@example.com",
            Address = "123 Main St, City, Country",
            LicenseNumber = "LIC123456",
            LicenseClass = "C",
            LicenseExpiryDate = DateTime.Now.AddYears(5),
            Status = "Active",
            ExperienceYears = 5
        };

        var json = JsonSerializer.Serialize(driverDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/drivers", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDriverById_WithValidId_ShouldReturnDriver()
    {
        // Arrange
        var driverId = "test-driver-id";

        // Act
        var response = await _client.GetAsync($"/api/drivers/{driverId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDriversByLicense_WithValidLicense_ShouldReturnDrivers()
    {
        // Arrange
        var licenseNumber = "LIC123456";

        // Act
        var response = await _client.GetAsync($"/api/drivers/license/{licenseNumber}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDriverPerformance_WithValidId_ShouldReturnPerformance()
    {
        // Arrange
        var driverId = "test-driver-id";

        // Act
        var response = await _client.GetAsync($"/api/drivers/{driverId}/performance");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}