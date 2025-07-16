using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SupplierService.Core.DTOs;
using SupplierService.Core.Entities;
using SupplierService.Core.Interfaces;
using SupplierService.Core.Mappings;
using SupplierService.Infrastructure.Data;
using SupplierService.Infrastructure.Repositories;
using Xunit;

namespace SupplierService.Tests;

public class SupplierServiceTests : IDisposable
{
    private readonly SupplierDbContext _context;
    private readonly IMapper _mapper;
    private readonly ISupplierRepository _supplierRepository;
    private readonly SupplierService.Core.Services.SupplierService _supplierService;

    public SupplierServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<SupplierDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SupplierDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<SupplierProfile>());
        _mapper = config.CreateMapper();

        // Setup repositories
        _supplierRepository = new SupplierRepository(_context);

        // Setup service
        _supplierService = new SupplierService.Core.Services.SupplierService(
            _supplierRepository,
            _mapper);
    }

    [Fact]
    public async Task CreateSupplier_ShouldCreateSupplierSuccessfully()
    {
        // Arrange
        var request = new CreateSupplierDto
        {
            Name = "Test Supplier Ltd",
            Code = "TSL001",
            Email = "contact@testsupplier.com",
            ContactPerson = "John Doe",
            Phone = "+1234567890",
            Type = "Manufacturer",
            Description = "Test supplier description"
        };

        // Act
        var result = await _supplierService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Code, result.Code);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal("Active", result.Status);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public async Task CreateSupplier_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Duplicate Supplier", "DUP001");
        var request = new CreateSupplierDto
        {
            Name = "Duplicate Supplier",
            Code = "DUP002",
            Email = "duplicate@test.com",
            Type = "Manufacturer"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _supplierService.CreateAsync(request));
    }

    [Fact]
    public async Task CreateSupplier_WithDuplicateCode_ShouldThrowException()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Test Supplier", "DUP001");
        var request = new CreateSupplierDto
        {
            Name = "Another Supplier",
            Code = "DUP001",
            Email = "another@test.com",
            Type = "Manufacturer"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _supplierService.CreateAsync(request));
    }

    [Fact]
    public async Task GetAllSuppliers_ShouldReturnAllSuppliers()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _supplierService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task GetSupplierById_ShouldReturnCorrectSupplier()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Test Supplier", "TEST001");

        // Act
        var result = await _supplierService.GetByIdAsync(supplier.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplier.Name, result.Name);
        Assert.Equal(supplier.Code, result.Code);
        Assert.Equal(supplier.Email, result.Email);
    }

    [Fact]
    public async Task GetSupplierByCode_ShouldReturnCorrectSupplier()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Test Supplier", "TEST001");

        // Act
        var result = await _supplierService.GetByCodeAsync("TEST001");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplier.Name, result.Name);
        Assert.Equal(supplier.Code, result.Code);
    }

    [Fact]
    public async Task UpdateSupplier_ShouldUpdateSupplierSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Original Supplier", "ORIG001");
        var updateRequest = new UpdateSupplierDto
        {
            Name = "Updated Supplier",
            Email = "updated@supplier.com",
            ContactPerson = "Jane Doe",
            Phone = "+9876543210",
            Type = "Distributor",
            Status = "Active"
        };

        // Act
        var result = await _supplierService.UpdateAsync(supplier.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Name, result.Name);
        Assert.Equal(updateRequest.Email, result.Email);
        Assert.Equal(updateRequest.ContactPerson, result.ContactPerson);
        Assert.Equal(updateRequest.Type, result.Type);
    }

    [Fact]
    public async Task DeleteSupplier_ShouldDeleteSupplierSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Supplier to Delete", "DEL001");

        // Act
        var result = await _supplierService.DeleteAsync(supplier.Id);

        // Assert
        Assert.True(result);

        // Verify supplier is deleted
        var deletedSupplier = await _supplierService.GetByIdAsync(supplier.Id);
        Assert.Null(deletedSupplier);
    }

    [Fact]
    public async Task SearchSuppliers_ShouldReturnMatchingSuppliers()
    {
        // Arrange
        await CreateTestSupplier("ABC Manufacturing", "ABC001");
        await CreateTestSupplier("XYZ Distribution", "XYZ001");
        await CreateTestSupplier("ABC Logistics", "ABC002");

        // Act
        var result = await _supplierService.SearchSuppliersAsync("ABC");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, s => Assert.Contains("ABC", s.Name));
    }

    [Fact]
    public async Task GetSuppliersByStatus_ShouldReturnCorrectSuppliers()
    {
        // Arrange
        var activeSupplier = await CreateTestSupplier("Active Supplier", "ACT001");
        var inactiveSupplier = await CreateTestSupplier("Inactive Supplier", "INA001");
        
        // Update one supplier to inactive
        await _supplierService.UpdateAsync(inactiveSupplier.Id, new UpdateSupplierDto
        {
            Status = "Inactive"
        });

        // Act
        var activeSuppliers = await _supplierService.GetByStatusAsync(SupplierStatus.Active);
        var inactiveSuppliers = await _supplierService.GetByStatusAsync(SupplierStatus.Inactive);

        // Assert
        Assert.Contains(activeSuppliers, s => s.Id == activeSupplier.Id);
        Assert.Contains(inactiveSuppliers, s => s.Id == inactiveSupplier.Id);
    }

    [Fact]
    public async Task GetSuppliersByType_ShouldReturnCorrectSuppliers()
    {
        // Arrange
        await CreateTestSupplier("Manufacturer 1", "MAN001", "Manufacturer");
        await CreateTestSupplier("Distributor 1", "DIS001", "Distributor");
        await CreateTestSupplier("Manufacturer 2", "MAN002", "Manufacturer");

        // Act
        var manufacturers = await _supplierService.GetByTypeAsync(SupplierType.Manufacturer);
        var distributors = await _supplierService.GetByTypeAsync(SupplierType.Distributor);

        // Assert
        Assert.Equal(2, manufacturers.Count());
        Assert.Single(distributors);
        Assert.All(manufacturers, s => Assert.Equal("Manufacturer", s.Type));
        Assert.All(distributors, s => Assert.Equal("Distributor", s.Type));
    }

    [Fact]
    public async Task VerifySupplier_ShouldVerifySupplierSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Unverified Supplier", "UNV001");

        // Act
        var result = await _supplierService.VerifySupplierAsync(supplier.Id);

        // Assert
        Assert.True(result);

        // Verify supplier is verified
        var verifiedSupplier = await _supplierService.GetByIdAsync(supplier.Id);
        Assert.NotNull(verifiedSupplier);
        Assert.True(verifiedSupplier.IsVerified);
        Assert.NotNull(verifiedSupplier.VerificationDate);
    }

    [Fact]
    public async Task IsNameAvailable_ShouldReturnCorrectResult()
    {
        // Arrange
        await CreateTestSupplier("Existing Supplier", "EXI001");

        // Act
        var existingNameAvailable = await _supplierService.IsNameAvailableAsync("Existing Supplier");
        var newNameAvailable = await _supplierService.IsNameAvailableAsync("New Supplier");

        // Assert
        Assert.False(existingNameAvailable);
        Assert.True(newNameAvailable);
    }

    [Fact]
    public async Task IsCodeAvailable_ShouldReturnCorrectResult()
    {
        // Arrange
        await CreateTestSupplier("Test Supplier", "EXI001");

        // Act
        var existingCodeAvailable = await _supplierService.IsCodeAvailableAsync("EXI001");
        var newCodeAvailable = await _supplierService.IsCodeAvailableAsync("NEW001");

        // Assert
        Assert.False(existingCodeAvailable);
        Assert.True(newCodeAvailable);
    }

    private async Task<SupplierReadDto> CreateTestSupplier(string name, string code, string type = "Manufacturer")
    {
        var request = new CreateSupplierDto
        {
            Name = name,
            Code = code,
            Email = $"{name.Replace(" ", "").ToLower()}@test.com",
            ContactPerson = "Test Contact",
            Phone = "+1234567890",
            Type = type,
            Description = "Test supplier description"
        };

        return await _supplierService.CreateAsync(request);
    }

    private async Task SeedTestData()
    {
        await CreateTestSupplier("Supplier 1", "SUP001");
        await CreateTestSupplier("Supplier 2", "SUP002");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}