using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Core.Mappings;
using ProductService.Core.Services;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using Xunit;

namespace ProductService.Tests;

public class PricingServiceTests : IDisposable
{
    private readonly ProductServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPricingRepository _pricingRepository;
    private readonly PricingService _pricingService;

    public PricingServiceTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ProductServiceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductServiceDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ProductProfile>());
        _mapper = config.CreateMapper();

        // Setup repository and service
        _pricingRepository = new PricingRepository(_context);
        _pricingService = new PricingService(_pricingRepository, _mapper);
    }

    [Fact]
    public async Task CreatePricing_ShouldCreatePricingSuccessfully()
    {
        // Arrange
        var product = await CreateTestProduct();
        var pricingDto = new CreatePricingDto
        {
            ProductId = product.Id,
            Type = PricingType.Standard,
            BasePrice = 99.99m,
            Currency = "USD",
            ValidFrom = DateTime.UtcNow,
            IsActive = true
        };

        // Act
        var result = await _pricingService.CreateAsync(pricingDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pricingDto.ProductId, result.ProductId);
        Assert.Equal(pricingDto.BasePrice, result.BasePrice);
        Assert.Equal(pricingDto.Currency, result.Currency);
    }

    [Fact]
    public async Task GetByProductId_ShouldReturnProductPricing()
    {
        // Arrange
        var product = await CreateTestProduct();
        await CreateTestPricing(product.Id, PricingType.Standard, 99.99m);
        await CreateTestPricing(product.Id, PricingType.Tiered, 89.99m, minQuantity: 10);

        // Act
        var result = await _pricingService.GetByProductIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, p => Assert.Equal(product.Id, p.ProductId));
    }

    [Fact]
    public async Task GetCustomerPricing_ShouldReturnCustomerSpecificPricing()
    {
        // Arrange
        var product = await CreateTestProduct();
        var customerId = "customer123";
        await CreateTestPricing(product.Id, PricingType.Standard, 99.99m);
        var customerPricing = await CreateTestPricing(product.Id, PricingType.CustomerSpecific, 89.99m, customerId: customerId);

        // Act
        var result = await _pricingService.GetEffectivePricingAsync(product.Id, customerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(customerPricing.Id, result.Id);
        Assert.Equal(89.99m, result.BasePrice);
        Assert.Equal(customerId, result.CustomerId);
    }

    [Fact]
    public async Task CalculateEffectivePrice_StandardPricing_ShouldReturnBasePrice()
    {
        // Arrange
        var product = await CreateTestProduct();
        await CreateTestPricing(product.Id, PricingType.Standard, 99.99m);

        // Act
        var result = await _pricingService.CalculateEffectivePriceAsync(product.Id);

        // Assert
        Assert.Equal(99.99m, result);
    }

    [Fact]
    public async Task CalculateEffectivePrice_CustomerSpecific_ShouldReturnCustomerPrice()
    {
        // Arrange
        var product = await CreateTestProduct();
        var customerId = "customer123";
        await CreateTestPricing(product.Id, PricingType.Standard, 99.99m);
        await CreateTestPricing(product.Id, PricingType.CustomerSpecific, 79.99m, customerId: customerId);

        // Act
        var result = await _pricingService.CalculateEffectivePriceAsync(product.Id, 1, customerId);

        // Assert
        Assert.Equal(79.99m, result);
    }

    [Fact]
    public async Task CalculateEffectivePrice_TieredPricing_ShouldReturnTieredPrice()
    {
        // Arrange
        var product = await CreateTestProduct();
        await CreateTestPricing(product.Id, PricingType.Standard, 99.99m);
        await CreateTestPricing(product.Id, PricingType.Tiered, 89.99m, minQuantity: 10, maxQuantity: 50);
        await CreateTestPricing(product.Id, PricingType.Tiered, 79.99m, minQuantity: 51);

        // Act
        var result1 = await _pricingService.CalculateEffectivePriceAsync(product.Id, quantity: 5);
        var result2 = await _pricingService.CalculateEffectivePriceAsync(product.Id, quantity: 25);
        var result3 = await _pricingService.CalculateEffectivePriceAsync(product.Id, quantity: 100);

        // Assert
        Assert.Equal(99.99m, result1); // Standard pricing for low quantity
        Assert.Equal(89.99m, result2); // First tier
        Assert.Equal(79.99m, result3); // Second tier
    }

    [Fact]
    public async Task CalculateEffectivePrice_WithDiscount_ShouldApplyDiscount()
    {
        // Arrange
        var product = await CreateTestProduct();
        await CreateTestPricing(product.Id, PricingType.Standard, 100.00m, discountPercentage: 10);

        // Act
        var result = await _pricingService.CalculateEffectivePriceAsync(product.Id);

        // Assert
        Assert.Equal(90.00m, result); // 10% discount applied
    }

    private async Task<Product> CreateTestProduct()
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Test Product",
            Code = "TEST001",
            SKU = "SKU001",
            Description = "Test Product Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private async Task<Pricing> CreateTestPricing(
        string productId, 
        PricingType type, 
        decimal basePrice, 
        string? customerId = null,
        int minQuantity = 1,
        int? maxQuantity = null,
        decimal? discountPercentage = null)
    {
        var pricing = new Pricing
        {
            Id = Guid.NewGuid().ToString(),
            ProductId = productId,
            Type = type,
            BasePrice = basePrice,
            Currency = "USD",
            CustomerId = customerId,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            DiscountPercentage = discountPercentage,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            Priority = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Pricings.AddAsync(pricing);
        await _context.SaveChangesAsync();
        return pricing;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}