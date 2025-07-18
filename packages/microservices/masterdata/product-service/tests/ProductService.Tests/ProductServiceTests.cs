using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Core.Mappings;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using Xunit;

namespace ProductService.Tests;

public class ProductServiceTests : IDisposable
{
    private readonly ProductServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProductRepository _productRepository;
    private readonly ProductService.Core.Services.ProductService _productService;

    public ProductServiceTests()
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
        _productRepository = new ProductRepository(_context);
        _productService = new ProductService.Core.Services.ProductService(_productRepository, _mapper);
    }

    [Fact]
    public async Task CreateProduct_ShouldCreateProductSuccessfully()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Code = "TEST001",
            SKU = "SKU001",
            Brand = "Test Brand"
        };

        // Act
        var result = await _productService.CreateAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Code, result.Code);
        Assert.NotNull(result.Id);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnAllProducts()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _productService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 2);
    }

    [Fact]
    public async Task GetProductById_ShouldReturnCorrectProduct()
    {
        // Arrange
        var product = await CreateTestProduct("Test Product", "TEST001");

        // Act
        var result = await _productService.GetByIdAsync(product.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Code, result.Code);
    }

    [Fact]
    public async Task UpdateProduct_ShouldUpdateProductSuccessfully()
    {
        // Arrange
        var product = await CreateTestProduct("Original Product", "ORIG001");
        var updateDto = new UpdateProductDto
        {
            Name = "Updated Product",
            Description = "Updated Description"
        };

        // Act
        var result = await _productService.UpdateAsync(product.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Description, result.Description);
    }

    [Fact]
    public async Task DeleteProduct_ShouldDeleteProductSuccessfully()
    {
        // Arrange
        var product = await CreateTestProduct("Product to Delete", "DEL001");

        // Act
        var result = await _productService.DeleteAsync(product.Id);

        // Assert
        Assert.True(result);
        
        // Verify product is deleted
        var deletedProduct = await _productService.GetByIdAsync(product.Id);
        Assert.Null(deletedProduct);
    }

    [Fact]
    public async Task IsNameAvailable_ShouldReturnCorrectAvailability()
    {
        // Arrange
        await CreateTestProduct("Existing Product", "EXIST001");

        // Act
        var existingNameAvailable = await _productService.IsNameAvailableAsync("Existing Product");
        var newNameAvailable = await _productService.IsNameAvailableAsync("New Product");

        // Assert
        Assert.False(existingNameAvailable);
        Assert.True(newNameAvailable);
    }

    private async Task<Product> CreateTestProduct(string name, string code)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Code = code,
            SKU = $"SKU-{code}",
            Description = $"Description for {name}",
            Brand = "Test Brand",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
        return product;
    }

    private async Task SeedTestData()
    {
        await CreateTestProduct("Product 1", "PROD001");
        await CreateTestProduct("Product 2", "PROD002");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}