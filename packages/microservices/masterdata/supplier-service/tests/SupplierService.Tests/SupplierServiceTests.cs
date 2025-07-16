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
        var contactRepository = new SupplierContactRepository(_context);
        var contractRepository = new SupplierContractRepository(_context);
        var productRepository = new SupplierProductRepository(_context);
        var performanceRepository = new SupplierPerformanceRepository(_context);
        var financialRepository = new SupplierFinancialRepository(_context);

        // Setup service
        _supplierService = new SupplierService.Core.Services.SupplierService(
            _supplierRepository,
            contactRepository,
            contractRepository,
            productRepository,
            performanceRepository,
            financialRepository,
            _mapper);
    }

    [Fact]
    public async Task RegisterSupplier_ShouldCreateSupplierSuccessfully()
    {
        // Arrange
        var request = new RegisterSupplierRequest
        {
            Name = "Test Supplier Ltd",
            ContactEmail = "contact@testsupplier.com",
            Address = "123 Test Street",
            SupplierType = SupplierType.Manufacturer,
            TaxNumber = "TAX123456",
            RegistrationNumber = "REG123456"
        };

        // Act
        var result = await _supplierService.RegisterSupplierAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.ContactEmail, result.ContactEmail);
        Assert.Equal(SupplierStatus.Active, result.Status);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public async Task RegisterSupplier_WithDuplicateName_ShouldThrowException()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Duplicate Supplier");
        var request = new RegisterSupplierRequest
        {
            Name = "Duplicate Supplier",
            ContactEmail = "duplicate@test.com",
            Address = "123 Test Street",
            SupplierType = SupplierType.Manufacturer
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _supplierService.RegisterSupplierAsync(request));
    }

    [Fact]
    public async Task GetAllSuppliers_ShouldReturnAllSuppliers()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _supplierService.GetAllSuppliersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task GetSupplierById_ShouldReturnCorrectSupplier()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Test Supplier");

        // Act
        var result = await _supplierService.GetSupplierByIdAsync(supplier.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(supplier.Name, result.Name);
        Assert.Equal(supplier.ContactEmail, result.ContactEmail);
    }

    [Fact]
    public async Task UpdateSupplier_ShouldUpdateSupplierSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Original Supplier");
        var updateRequest = new UpdateSupplierRequest
        {
            Name = "Updated Supplier",
            ContactEmail = supplier.ContactEmail,
            Address = "Updated Address",
            SupplierType = SupplierType.Distributor,
            Status = SupplierStatus.Active
        };

        // Act
        var result = await _supplierService.UpdateSupplierAsync(supplier.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.Name, result.Name);
        Assert.Equal(updateRequest.Address, result.Address);
        Assert.Equal(updateRequest.SupplierType, result.SupplierType);
    }

    [Fact]
    public async Task CreateContact_ShouldCreateContactSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Supplier with Contact");
        var contactRequest = new CreateSupplierContactRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@supplier.com",
            Phone = "+1234567890",
            JobTitle = "Sales Manager",
            ContactType = ContactType.Sales,
            IsPrimary = true
        };

        // Act
        var result = await _supplierService.CreateContactAsync(supplier.Id, contactRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contactRequest.FirstName, result.FirstName);
        Assert.Equal(contactRequest.LastName, result.LastName);
        Assert.Equal(contactRequest.Email, result.Email);
        Assert.True(result.IsPrimary);
    }

    [Fact]
    public async Task CreateContract_ShouldCreateContractSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Supplier with Contract");
        var contractRequest = new CreateSupplierContractRequest
        {
            ContractNumber = "CNT-001",
            Title = "Test Contract",
            Description = "Test contract description",
            ContractType = ContractType.Supply,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            ContractValue = 100000m,
            Currency = "USD",
            PaymentTerms = PaymentTerms.Net30
        };

        // Act
        var result = await _supplierService.CreateContractAsync(supplier.Id, contractRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contractRequest.ContractNumber, result.ContractNumber);
        Assert.Equal(contractRequest.Title, result.Title);
        Assert.Equal(contractRequest.ContractValue, result.ContractValue);
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Supplier with Product");
        var productRequest = new CreateSupplierProductRequest
        {
            ProductId = "PROD-001",
            SupplierPrice = 850m,
            Currency = "KES",
            MinimumOrderQuantity = 10,
            LeadTimeDays = 5,
            IsPreferred = true
        };

        // Act
        var result = await _supplierService.CreateProductAsync(supplier.Id, productRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productRequest.ProductId, result.ProductId);
        Assert.Equal(productRequest.SupplierPrice, result.UnitPrice);
        Assert.Equal(productRequest.MinimumOrderQuantity, result.MinimumOrderQuantity);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task CreatePerformance_ShouldCreatePerformanceSuccessfully()
    {
        // Arrange
        var supplier = await CreateTestSupplier("Supplier with Performance");
        var performanceRequest = new CreateSupplierPerformanceRequest
        {
            EvaluationDate = DateTime.UtcNow,
            QualityScore = 85,
            DeliveryScore = 90,
            ServiceScore = 88,
            OverallScore = 87.67m,
            Comments = "Good performance overall",
            EvaluatedBy = "Test Evaluator"
        };

        // Act
        var result = await _supplierService.CreatePerformanceAsync(supplier.Id, performanceRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(85m, result.QualityRating);
        Assert.Equal(90m, result.DeliveryRating);
        Assert.Equal(88m, result.ServiceRating);
        Assert.Equal(87.67m, result.OverallRating);
    }

    [Fact]
    public async Task SearchSuppliers_ShouldReturnMatchingSuppliers()
    {
        // Arrange
        await CreateTestSupplier("ABC Manufacturing");
        await CreateTestSupplier("XYZ Distribution");
        await CreateTestSupplier("ABC Logistics");

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
        var activeSupplier = await CreateTestSupplier("Active Supplier");
        var inactiveSupplier = await CreateTestSupplier("Inactive Supplier");
        
        // Update one supplier to inactive
        await _supplierService.UpdateSupplierAsync(inactiveSupplier.Id, new UpdateSupplierRequest
        {
            Name = inactiveSupplier.Name,
            ContactEmail = inactiveSupplier.ContactEmail,
            Address = inactiveSupplier.Address,
            SupplierType = inactiveSupplier.SupplierType,
            Status = SupplierStatus.Inactive
        });

        // Act
        var activeSuppliers = await _supplierService.GetSuppliersByStatusAsync(SupplierStatus.Active);
        var inactiveSuppliers = await _supplierService.GetSuppliersByStatusAsync(SupplierStatus.Inactive);

        // Assert
        Assert.Contains(activeSuppliers, s => s.Id == activeSupplier.Id);
        Assert.Contains(inactiveSuppliers, s => s.Id == inactiveSupplier.Id);
    }

    private async Task<SupplierDto> CreateTestSupplier(string name)
    {
        var request = new RegisterSupplierRequest
        {
            Name = name,
            ContactEmail = $"{name.Replace(" ", "").ToLower()}@test.com",
            Address = "Test Address",
            SupplierType = SupplierType.Manufacturer,
            TaxNumber = $"TAX{Guid.NewGuid().ToString()[..8]}",
            RegistrationNumber = $"REG{Guid.NewGuid().ToString()[..8]}"
        };

        return await _supplierService.RegisterSupplierAsync(request);
    }

    private async Task SeedTestData()
    {
        await CreateTestSupplier("Supplier 1");
        await CreateTestSupplier("Supplier 2");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}