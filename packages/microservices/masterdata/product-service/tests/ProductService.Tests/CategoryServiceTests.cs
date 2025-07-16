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

public class CategoryServiceTests : IDisposable
{
    private readonly ProductServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
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
        _categoryRepository = new CategoryRepository(_context);
        _categoryService = new CategoryService(_categoryRepository, _mapper);
    }

    [Fact]
    public async Task CreateCategory_ShouldCreateCategorySuccessfully()
    {
        // Arrange
        var categoryDto = new CategoryDto
        {
            Name = "Electronics",
            Description = "Electronic products",
            Code = "ELEC",
            IsVisible = true,
            AllowProducts = true
        };

        // Act
        var result = await _categoryService.CreateAsync(categoryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryDto.Name, result.Name);
        Assert.Equal(categoryDto.Code, result.Code);
        Assert.Equal(0, result.Level); // Root category
        Assert.Equal("Electronics", result.Path);
    }

    [Fact]
    public async Task CreateSubCategory_ShouldSetCorrectHierarchy()
    {
        // Arrange
        var parentCategory = await CreateTestCategory("Electronics", "ELEC", null);
        var subCategoryDto = new CategoryDto
        {
            Name = "Computers",
            Description = "Computer products",
            Code = "COMP",
            ParentCategoryId = parentCategory.Id,
            IsVisible = true,
            AllowProducts = true
        };

        // Act
        var result = await _categoryService.CreateAsync(subCategoryDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Level); // Sub category
        Assert.Equal("Electronics/Computers", result.Path);
        Assert.Equal(parentCategory.Id, result.ParentCategoryId);
    }

    [Fact]
    public async Task GetHierarchy_ShouldReturnRootCategories()
    {
        // Arrange
        var rootCategory1 = await CreateTestCategory("Electronics", "ELEC", null);
        var rootCategory2 = await CreateTestCategory("Clothing", "CLOTH", null);
        await CreateTestCategory("Computers", "COMP", rootCategory1.Id);

        // Act
        var result = await _categoryService.GetHierarchyAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count()); // Only root categories
        Assert.Contains(result, c => c.Name == "Electronics");
        Assert.Contains(result, c => c.Name == "Clothing");
    }

    [Fact]
    public async Task GetSubCategories_ShouldReturnCorrectSubCategories()
    {
        // Arrange
        var parentCategory = await CreateTestCategory("Electronics", "ELEC", null);
        await CreateTestCategory("Computers", "COMP", parentCategory.Id);
        await CreateTestCategory("Phones", "PHONE", parentCategory.Id);

        // Act
        var result = await _categoryService.GetSubCategoriesAsync(parentCategory.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.Equal(parentCategory.Id, c.ParentCategoryId));
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnAllCategories()
    {
        // Arrange
        await SeedTestData();

        // Act
        var result = await _categoryService.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Count() >= 3);
    }

    [Fact]
    public async Task UpdateCategory_ShouldUpdateCategorySuccessfully()
    {
        // Arrange
        var category = await CreateTestCategory("Original Category", "ORIG", null);
        var updateDto = new CategoryDto
        {
            Id = category.Id,
            Name = "Updated Category",
            Description = "Updated Description",
            Code = category.Code,
            IsVisible = false
        };

        // Act
        var result = await _categoryService.UpdateAsync(category.Id, updateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateDto.Name, result.Name);
        Assert.Equal(updateDto.Description, result.Description);
        Assert.False(result.IsVisible);
    }

    [Fact]
    public async Task DeleteCategory_ShouldDeleteCategorySuccessfully()
    {
        // Arrange
        var category = await CreateTestCategory("Category to Delete", "DEL", null);

        // Act
        var result = await _categoryService.DeleteAsync(category.Id);

        // Assert
        Assert.True(result);
        
        // Verify category is deleted
        var deletedCategory = await _categoryService.GetByIdAsync(category.Id);
        Assert.Null(deletedCategory);
    }

    private async Task<Category> CreateTestCategory(string name, string code, string? parentId)
    {
        var category = new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Code = code,
            Description = $"Description for {name}",
            ParentCategoryId = parentId,
            Level = parentId == null ? 0 : 1,
            Path = parentId == null ? name : $"Parent/{name}",
            IsVisible = true,
            AllowProducts = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    private async Task SeedTestData()
    {
        var electronics = await CreateTestCategory("Electronics", "ELEC", null);
        await CreateTestCategory("Computers", "COMP", electronics.Id);
        await CreateTestCategory("Clothing", "CLOTH", null);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}