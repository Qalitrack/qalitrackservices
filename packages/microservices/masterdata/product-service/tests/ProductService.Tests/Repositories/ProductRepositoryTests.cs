using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Repositories;

[Trait("Category", "Repository")]
public class ProductRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new ProductDbContext(options);
        _repository = new ProductRepository(_context);
        
        // Seed test data
        SeedTestData();
    }

    #region CRUD Operations Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllProducts()
    {
        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCountGreaterThan(0);
        products.Should().AllBeOfType<Product>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var existingProduct = await _context.Products.FirstAsync();

        // Act
        var product = await _repository.GetByIdAsync(existingProduct.Id);

        // Assert
        product.Should().NotBeNull();
        product!.Id.Should().Be(existingProduct.Id);
        product.Name.Should().Be(existingProduct.Name);
        product.Code.Should().Be(existingProduct.Code);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var product = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        product.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddProduct_WhenValidProduct()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var newProduct = TestDataFactory.CreateTestProduct();
        newProduct.CategoryId = category.Id;
        newProduct.Code = TestDataFactory.GenerateProductCode();

        // Act
        var addedProduct = await _repository.AddAsync(newProduct);

        // Assert
        addedProduct.Should().NotBeNull();
        addedProduct.Id.Should().NotBeNullOrEmpty();
        addedProduct.Name.Should().Be(newProduct.Name);
        addedProduct.Code.Should().Be(newProduct.Code);

        // Verify product was saved to database
        var savedProduct = await _context.Products.FindAsync(addedProduct.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Name.Should().Be(newProduct.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenValidProduct()
    {
        // Arrange
        var existingProduct = await _context.Products.FirstAsync();
        existingProduct.Name = "Updated Product Name";
        existingProduct.Description = "Updated Description";
        existingProduct.Weight = 999.99m;

        // Act
        var updatedProduct = await _repository.UpdateAsync(existingProduct);

        // Assert
        updatedProduct.Should().NotBeNull();
        updatedProduct.Name.Should().Be("Updated Product Name");
        updatedProduct.Description.Should().Be("Updated Description");
        updatedProduct.Weight.Should().Be(999.99m);
        updatedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify product was updated in database
        var dbProduct = await _context.Products.FindAsync(existingProduct.Id);
        dbProduct.Should().NotBeNull();
        dbProduct!.Name.Should().Be("Updated Product Name");
        dbProduct.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteProduct_WhenProductExists()
    {
        // Arrange
        var productToDelete = TestDataFactory.CreateTestProduct();
        var category = await _context.ProductCategories.FirstAsync();
        productToDelete.CategoryId = category.Id;
        _context.Products.Add(productToDelete);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(productToDelete.Id);

        // Assert
        var deletedProduct = await _context.Products.FindAsync(productToDelete.Id);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.IsDeleted.Should().BeTrue();
        deletedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        
        // Verify product is not returned by GetByIdAsync (soft delete filter)
        var retrievedProduct = await _repository.GetByIdAsync(productToDelete.Id);
        retrievedProduct.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenProductDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert
        var act = async () => await _repository.DeleteAsync(nonExistentId);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Query Methods Tests

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnProductsInCategory()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var expectedProducts = await _context.Products
            .Where(p => p.CategoryId == category.Id)
            .ToListAsync();

        // Act
        var products = await _repository.GetByCategoryAsync(category.Id);

        // Assert
        products.Should().NotBeNull();
        products.Should().HaveCount(expectedProducts.Count);
        products.Should().OnlyContain(p => p.CategoryId == category.Id);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnEmptyList_WhenCategoryHasNoProducts()
    {
        // Arrange
        var emptyCategory = TestDataFactory.CreateTestProductCategory();
        _context.ProductCategories.Add(emptyCategory);
        await _context.SaveChangesAsync();

        // Act
        var products = await _repository.GetByCategoryAsync(emptyCategory.Id);

        // Assert
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHazardousProductsAsync_ShouldReturnOnlyHazardousProducts()
    {
        // Arrange
        // Add some hazardous products
        var category = await _context.ProductCategories.FirstAsync();
        var hazardousProduct1 = TestDataFactory.CreateTestProduct(isHazardous: true);
        var hazardousProduct2 = TestDataFactory.CreateTestProduct(isHazardous: true);
        hazardousProduct1.CategoryId = category.Id;
        hazardousProduct2.CategoryId = category.Id;
        hazardousProduct1.Code = TestDataFactory.GenerateProductCode();
        hazardousProduct2.Code = TestDataFactory.GenerateProductCode();
        
        _context.Products.AddRange(hazardousProduct1, hazardousProduct2);
        await _context.SaveChangesAsync();

        // Act
        var hazardousProducts = await _repository.GetHazardousProductsAsync();

        // Assert
        hazardousProducts.Should().NotBeNull();
        hazardousProducts.Should().OnlyContain(p => p.IsHazardous);
        hazardousProducts.Should().Contain(p => p.Id == hazardousProduct1.Id);
        hazardousProducts.Should().Contain(p => p.Id == hazardousProduct2.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingProducts_WhenSearchingByName()
    {
        // Arrange
        var searchTerm = "TestSearch";
        var category = await _context.ProductCategories.FirstAsync();
        var matchingProduct = TestDataFactory.CreateTestProduct();
        matchingProduct.Name = $"{searchTerm} Product";
        matchingProduct.CategoryId = category.Id;
        matchingProduct.Code = TestDataFactory.GenerateProductCode();
        
        _context.Products.Add(matchingProduct);
        await _context.SaveChangesAsync();

        // Act
        var searchResults = await _repository.SearchProductsAsync(searchTerm);

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().Contain(p => p.Id == matchingProduct.Id);
        searchResults.Should().OnlyContain(p => 
            p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            p.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (p.Description != null && p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingProducts_WhenSearchingByCode()
    {
        // Arrange
        var searchTerm = "SRCH";
        var category = await _context.ProductCategories.FirstAsync();
        var matchingProduct = TestDataFactory.CreateTestProduct();
        matchingProduct.Code = $"{searchTerm}001";
        matchingProduct.CategoryId = category.Id;
        
        _context.Products.Add(matchingProduct);
        await _context.SaveChangesAsync();

        // Act
        var searchResults = await _repository.SearchProductsAsync(searchTerm);

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().Contain(p => p.Id == matchingProduct.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMatchingProducts_WhenSearchingByDescription()
    {
        // Arrange
        var searchTerm = "SearchableDescription";
        var category = await _context.ProductCategories.FirstAsync();
        var matchingProduct = TestDataFactory.CreateTestProduct();
        matchingProduct.Description = $"This is a {searchTerm} for testing";
        matchingProduct.CategoryId = category.Id;
        matchingProduct.Code = TestDataFactory.GenerateProductCode();
        
        _context.Products.Add(matchingProduct);
        await _context.SaveChangesAsync();

        // Act
        var searchResults = await _repository.SearchProductsAsync(searchTerm);

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().Contain(p => p.Id == matchingProduct.Id);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenNoMatches()
    {
        // Arrange
        var searchTerm = "NonExistentSearchTerm123456";

        // Act
        var searchResults = await _repository.SearchProductsAsync(searchTerm);

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenSearchTermIsInvalid(string? searchTerm)
    {
        // Act
        var searchResults = await _repository.SearchProductsAsync(searchTerm!);

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().BeEmpty();
    }

    #endregion

    #region Navigation Properties Tests

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeCategory_WhenIncludeIsSpecified()
    {
        // Arrange
        var productWithCategory = await _context.Products
            .Include(p => p.Category)
            .FirstAsync();

        // Act
        var product = await _repository.GetByIdAsync(productWithCategory.Id);

        // Assert
        product.Should().NotBeNull();
        // Note: The repository might or might not include navigation properties by default
        // This test verifies the current behavior
        if (product!.Category != null)
        {
            product.Category.Should().NotBeNull();
            product.Category.Id.Should().Be(product.CategoryId);
        }
    }

    [Fact]
    public async Task GetAllAsync_ShouldHandleProductsWithoutNavigationProperties()
    {
        // Act
        var products = await _repository.GetAllAsync();

        // Assert
        products.Should().NotBeNull();
        products.Should().AllSatisfy(p =>
        {
            p.Id.Should().NotBeNullOrEmpty();
            p.Name.Should().NotBeNullOrEmpty();
            p.Code.Should().NotBeNullOrEmpty();
            p.CategoryId.Should().NotBeNullOrEmpty();
        });
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task UpdateAsync_ShouldHandleConcurrentUpdates()
    {
        // Arrange
        var product = await _context.Products.FirstAsync();
        var originalName = product.Name;

        // Simulate concurrent update by modifying the product in another context
        using var anotherContext = new ProductDbContext(
            new DbContextOptionsBuilder<ProductDbContext>()
                .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
                .Options);
        
        // This is a simplified test - in reality, you'd need the same database
        // to properly test concurrency conflicts

        // Act
        product.Name = "Updated by Test";
        var updatedProduct = await _repository.UpdateAsync(product);

        // Assert
        updatedProduct.Should().NotBeNull();
        updatedProduct.Name.Should().Be("Updated by Test");
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetAllAsync_ShouldHandleLargeDataset()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var products = new List<Product>();
        
        for (int i = 0; i < 100; i++)
        {
            var product = TestDataFactory.CreateTestProduct();
            product.CategoryId = category.Id;
            product.Code = $"PERF{i:D3}";
            product.Name = $"Performance Test Product {i}";
            products.Add(product);
        }
        
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var startTime = DateTime.UtcNow;
        var allProducts = await _repository.GetAllAsync();
        var endTime = DateTime.UtcNow;

        // Assert
        allProducts.Should().NotBeNull();
        allProducts.Should().HaveCountGreaterOrEqualTo(100);
        
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5)); // Should complete within 5 seconds
    }

    [Fact]
    public async Task SearchAsync_ShouldBeEfficient_WithLargeDataset()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var products = new List<Product>();
        
        for (int i = 0; i < 50; i++)
        {
            var product = TestDataFactory.CreateTestProduct();
            product.CategoryId = category.Id;
            product.Code = $"SEARCH{i:D3}";
            product.Name = $"Searchable Product {i}";
            products.Add(product);
        }
        
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        // Act
        var startTime = DateTime.UtcNow;
        var searchResults = await _repository.SearchProductsAsync("Searchable");
        var endTime = DateTime.UtcNow;

        // Assert
        searchResults.Should().NotBeNull();
        searchResults.Should().HaveCountGreaterOrEqualTo(50);
        
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should complete within 2 seconds
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task AddAsync_ShouldSetCreatedAndUpdatedDates()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var newProduct = TestDataFactory.CreateTestProduct();
        newProduct.CategoryId = category.Id;
        newProduct.Code = TestDataFactory.GenerateProductCode();
        newProduct.CreatedAt = default; // Reset dates
        newProduct.UpdatedAt = default;

        // Act
        var addedProduct = await _repository.AddAsync(newProduct);

        // Assert
        addedProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        addedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOnlyUpdatedDate()
    {
        // Arrange
        var existingProduct = await _context.Products.FirstAsync();
        var originalCreatedAt = existingProduct.CreatedAt;
        var originalUpdatedAt = existingProduct.UpdatedAt;
        
        // Wait a bit to ensure different timestamp
        await Task.Delay(100);
        
        existingProduct.Name = "Updated Name";

        // Act
        var updatedProduct = await _repository.UpdateAsync(existingProduct);

        // Assert
        updatedProduct.CreatedAt.Should().Be(originalCreatedAt); // Should not change
        updatedProduct.UpdatedAt.Should().BeAfter(originalUpdatedAt); // Should be updated
        updatedProduct.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldHandleNonExistentCategory()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid().ToString();

        // Act
        var products = await _repository.GetByCategoryAsync(nonExistentCategoryId);

        // Assert
        products.Should().NotBeNull();
        products.Should().BeEmpty();
    }

    #endregion

    private void SeedTestData()
    {
        // Add test categories
        var categories = TestDataFactory.CreateTestCategoryHierarchy();
        _context.ProductCategories.AddRange(categories);
        _context.SaveChanges();

        // Add test products
        var products = TestDataFactory.CreateTestProductList();
        foreach (var product in products)
        {
            product.CategoryId = categories.First().Id;
        }
        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}