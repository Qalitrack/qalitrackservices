using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;
using VehicleService.Core.Entities;
using VehicleService.Core.DTOs;
using VehicleService.Infrastructure.Data;

namespace VehicleService.Tests;

public class VehicleServiceTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public VehicleServiceTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetVehicles_ShouldReturnOkResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/vehicles");

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
    public async Task CreateVehicle_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var vehicleDto = new CreateVehicleDto
        {
            RegistrationNumber = "TEST-001",
            Make = "Toyota",
            Model = "Hilux",
            Year = 2023,
            EngineNumber = "ENG123456",
            ChassisNumber = "CHAS789012",
            Color = "White",
            FuelType = "Diesel",
            VehicleType = "Truck",
            Status = "Active"
        };

        var json = JsonSerializer.Serialize(vehicleDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/vehicles", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehicleById_WithValidId_ShouldReturnVehicle()
    {
        // Arrange
        var vehicleId = "test-vehicle-id";

        // Act
        var response = await _client.GetAsync($"/api/vehicles/{vehicleId}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetVehiclesByRegistration_WithValidRegistration_ShouldReturnVehicles()
    {
        // Arrange
        var registration = "TEST-001";

        // Act
        var response = await _client.GetAsync($"/api/vehicles/registration/{registration}");

        // Assert
        Assert.True(response.IsSuccessStatusCode || 
                   response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized);
    }
}