using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VehicleService.Core.DTOs;
using VehicleService.Core.Entities;
using VehicleService.Core.Interfaces;
using VehicleService.Core.Mappings;
using VehicleService.Infrastructure.Data;
using VehicleService.Infrastructure.Repositories;
using Xunit;

namespace VehicleService.Tests;

public class VehicleServiceTests : IDisposable
{
    private readonly VehicleDbContext _context;
    private readonly IMapper _mapper;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleTypeRepository _vehicleTypeRepository;
    private readonly VehicleService.Core.Services.VehicleService _vehicleService;

    public VehicleServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<VehicleDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new VehicleDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<VehicleProfile>());
        _mapper = config.CreateMapper();

        // Setup repositories
        _vehicleRepository = new VehicleRepository(_context);
        _vehicleTypeRepository = new VehicleTypeRepository(_context);
        var registrationRepository = new VehicleRegistrationRepository(_context);
        var specificationRepository = new VehicleSpecificationRepository(_context);
        var documentRepository = new VehicleDocumentRepository(_context);
        var inspectionRepository = new VehicleInspectionRepository(_context);
        var insuranceRepository = new VehicleInsuranceRepository(_context);

        // Setup service
        _vehicleService = new VehicleService.Core.Services.VehicleService(
            _vehicleRepository,
            _vehicleTypeRepository,
            registrationRepository,
            specificationRepository,
            documentRepository,
            inspectionRepository,
            insuranceRepository,
            _mapper);
    }

    [Fact]
    public async Task RegisterVehicle_ShouldCreateVehicleSuccessfully()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var request = new RegisterVehicleRequest
        {
            RegistrationNumber = "KCA-123A",
            Make = "Toyota",
            Model = "Hilux",
            Year = 2022,
            Color = "White",
            VIN = "JTMHY05V0X4123456",
            EngineNumber = "2KD-FTV-123456",
            FuelType = "Diesel",
            MaxWeight = 3500m,
            TareWeight = 1800m,
            OwnerName = "John Doe Transport",
            OwnerContactInfo = "+254700123456",
            VehicleTypeId = vehicleType.Id,
            CurrentMileage = 15000m
        };

        // Act
        var result = await _vehicleService.RegisterVehicleAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.RegistrationNumber, result.RegistrationNumber);
        Assert.Equal(request.Make, result.Make);
        Assert.Equal(request.Model, result.Model);
        Assert.Equal(VehicleStatus.Active, result.Status);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public async Task RegisterVehicle_WithDuplicateRegistrationNumber_ShouldThrowException()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var vehicle = await CreateTestVehicle("KCA-123A", vehicleType.Id);
        
        var request = new RegisterVehicleRequest
        {
            RegistrationNumber = "KCA-123A", // Duplicate
            Make = "Nissan",
            Model = "Navara",
            Year = 2023,
            VehicleTypeId = vehicleType.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _vehicleService.RegisterVehicleAsync(request));
    }

    [Fact]
    public async Task GetVehicleByRegistrationNumber_ShouldReturnCorrectVehicle()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var vehicle = await CreateTestVehicle("KCA-456B", vehicleType.Id);

        // Act
        var result = await _vehicleService.GetVehicleByRegistrationNumberAsync("KCA-456B");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(vehicle.RegistrationNumber, result.RegistrationNumber);
        Assert.Equal(vehicle.Make, result.Make);
    }

    [Fact]
    public async Task GetVehicleByVIN_ShouldReturnCorrectVehicle()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var vehicle = await CreateTestVehicle("KCA-789C", vehicleType.Id, "JTMHY05V0X4789012");

        // Act
        var result = await _vehicleService.GetVehicleByVINAsync("JTMHY05V0X4789012");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(vehicle.VIN, result.VIN);
        Assert.Equal(vehicle.RegistrationNumber, result.RegistrationNumber);
    }

    [Fact]
    public async Task GetVehiclesByStatus_ShouldReturnCorrectVehicles()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var activeVehicle = await CreateTestVehicle("KCA-111A", vehicleType.Id);
        var inactiveVehicle = await CreateTestVehicle("KCA-222B", vehicleType.Id);
        
        // Update one vehicle to inactive
        await _vehicleService.UpdateVehicleAsync(inactiveVehicle.Id, new UpdateVehicleRequest
        {
            Make = inactiveVehicle.Make,
            Model = inactiveVehicle.Model,
            Year = inactiveVehicle.Year,
            Color = inactiveVehicle.Color,
            Status = VehicleStatus.Inactive
        });

        // Act
        var activeVehicles = await _vehicleService.GetVehiclesByStatusAsync(VehicleStatus.Active);
        var inactiveVehicles = await _vehicleService.GetVehiclesByStatusAsync(VehicleStatus.Inactive);

        // Assert
        Assert.Contains(activeVehicles, v => v.Id == activeVehicle.Id);
        Assert.Contains(inactiveVehicles, v => v.Id == inactiveVehicle.Id);
    }

    [Fact]
    public async Task GetVehiclesByOwner_ShouldReturnCorrectVehicles()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var owner1Vehicle = await CreateTestVehicle("KCA-333C", vehicleType.Id, ownerName: "Owner One");
        var owner2Vehicle = await CreateTestVehicle("KCA-444D", vehicleType.Id, ownerName: "Owner Two");

        // Act
        var owner1Vehicles = await _vehicleService.GetVehiclesByOwnerAsync("Owner One");
        var owner2Vehicles = await _vehicleService.GetVehiclesByOwnerAsync("Owner Two");

        // Assert
        Assert.Single(owner1Vehicles);
        Assert.Single(owner2Vehicles);
        Assert.Equal(owner1Vehicle.Id, owner1Vehicles.First().Id);
        Assert.Equal(owner2Vehicle.Id, owner2Vehicles.First().Id);
    }

    [Fact]
    public async Task UpdateVehicle_ShouldUpdateVehicleSuccessfully()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var vehicle = await CreateTestVehicle("KCA-555E", vehicleType.Id);
        
        var updateRequest = new UpdateVehicleRequest
        {
            Make = "Updated Make",
            Model = "Updated Model",
            Year = 2024,
            Color = "Blue",
            Status = VehicleStatus.Active,
            CurrentMileage = 20000m
        };

        // Act
        var result = await _vehicleService.UpdateVehicleAsync(vehicle.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Make, result.Make);
        Assert.Equal(updateRequest.Model, result.Model);
        Assert.Equal(updateRequest.Year, result.Year);
        Assert.Equal(updateRequest.Color, result.Color);
        Assert.Equal(updateRequest.CurrentMileage, result.CurrentMileage);
    }

    [Fact]
    public async Task ValidateRegistrationNumber_ShouldReturnCorrectValidation()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        await CreateTestVehicle("KCA-666F", vehicleType.Id);

        // Act
        var existingNumberValid = await _vehicleService.ValidateRegistrationNumberAsync("KCA-666F");
        var newNumberValid = await _vehicleService.ValidateRegistrationNumberAsync("KCA-777G");

        // Assert
        Assert.False(existingNumberValid); // Should be false because it exists
        Assert.True(newNumberValid); // Should be true because it doesn't exist
    }

    [Fact]
    public async Task ValidateVIN_ShouldReturnCorrectValidation()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        await CreateTestVehicle("KCA-888H", vehicleType.Id, "EXISTING-VIN-123456");

        // Act
        var existingVinValid = await _vehicleService.ValidateVINAsync("EXISTING-VIN-123456");
        var newVinValid = await _vehicleService.ValidateVINAsync("NEW-VIN-789012");

        // Assert
        Assert.False(existingVinValid); // Should be false because it exists
        Assert.True(newVinValid); // Should be true because it doesn't exist
    }

    [Fact]
    public async Task CreateVehicleType_ShouldCreateVehicleTypeSuccessfully()
    {
        // Arrange
        var request = new CreateVehicleTypeRequest
        {
            Name = "Heavy Truck",
            Description = "Heavy duty trucks for construction",
            Category = "Commercial",
            MaxWeightLimit = 15000m
        };

        // Act
        var result = await _vehicleService.CreateVehicleTypeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Category, result.Category);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreateVehicleType_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        await CreateTestVehicleType("Duplicate Type");
        
        var request = new CreateVehicleTypeRequest
        {
            Name = "Duplicate Type",
            Description = "Another type with same name"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _vehicleService.CreateVehicleTypeAsync(request));
    }

    [Fact]
    public async Task GetAllVehicleTypes_ShouldReturnAllTypes()
    {
        // Arrange
        await CreateTestVehicleType("Type 1");
        await CreateTestVehicleType("Type 2");

        // Act
        var result = await _vehicleService.GetAllVehicleTypesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public async Task DeleteVehicle_ShouldDeleteVehicleSuccessfully()
    {
        // Arrange
        var vehicleType = await CreateTestVehicleType();
        var vehicle = await CreateTestVehicle("KCA-999Z", vehicleType.Id);

        // Act
        await _vehicleService.DeleteVehicleAsync(vehicle.Id);

        // Assert
        var deletedVehicle = await _vehicleService.GetVehicleByIdAsync(vehicle.Id);
        Assert.Null(deletedVehicle);
    }

    private async Task<VehicleDto> CreateTestVehicle(string registrationNumber, string vehicleTypeId, string? vin = null, string? ownerName = null)
    {
        var request = new RegisterVehicleRequest
        {
            RegistrationNumber = registrationNumber,
            Make = "Toyota",
            Model = "Hilux",
            Year = 2022,
            Color = "White",
            VIN = vin ?? $"VIN-{Guid.NewGuid().ToString()[..10]}",
            EngineNumber = $"ENG-{Guid.NewGuid().ToString()[..10]}",
            FuelType = "Diesel",
            MaxWeight = 3500m,
            TareWeight = 1800m,
            OwnerName = ownerName ?? "Test Owner",
            OwnerContactInfo = "+254700123456",
            VehicleTypeId = vehicleTypeId,
            CurrentMileage = 15000m
        };

        return await _vehicleService.RegisterVehicleAsync(request);
    }

    private async Task<VehicleTypeDto> CreateTestVehicleType(string? name = null)
    {
        var request = new CreateVehicleTypeRequest
        {
            Name = name ?? "Test Vehicle Type",
            Description = "Test vehicle type description",
            Category = "Commercial",
            MaxWeightLimit = 10000m
        };

        return await _vehicleService.CreateVehicleTypeAsync(request);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}