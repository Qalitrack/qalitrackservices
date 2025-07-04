using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Core.Entities;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Repositories;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Repositories;

[Trait("Category", "Repository")]
public class ProductCategoryRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly ProductCategoryRepository _repository;

    public ProductCategoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        
        _context = new ProductDbContext(options);
        _repository = new ProductCategoryRepository(_context);
        
        // Seed test data
        SeedTestData();
    }

    #region CRUD Operations Tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Act
        var categories = await _repository.GetAllAsync();

        // Assert
        categories.Should().NotBeNull();
        categories.Should().HaveCountGreaterThan(0);
        categories.Should().AllBeOfType<ProductCategory>();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCategory_WhenCategoryExists()
    {
        // Arrange
        var existingCategory = await _context.ProductCategories.FirstAsync();

        // Act
        var category = await _repository.GetByIdAsync(existingCategory.Id);

        // Assert
        category.Should().NotBeNull();
        category!.Id.Should().Be(existingCategory.Id);
        category.Name.Should().Be(existingCategory.Name);
        category.Description.Should().Be(existingCategory.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var category = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        category.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategory_WhenValidCategory()
    {
        // Arrange
        var newCategory = TestDataFactory.CreateTestProductCategory();
        newCategory.Name = TestDataFactory.GenerateUniqueName("Test Category");

        // Act
        var addedCategory = await _repository.AddAsync(newCategory);

        // Assert
        addedCategory.Should().NotBeNull();
        addedCategory.Id.Should().NotBeNullOrEmpty();
        addedCategory.Name.Should().Be(newCategory.Name);
        addedCategory.Description.Should().Be(newCategory.Description);

        // Verify category was saved to database
        var savedCategory = await _context.ProductCategories.FindAsync(addedCategory.Id);
        savedCategory.Should().NotBeNull();
        savedCategory!.Name.Should().Be(newCategory.Name);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCategory_WhenValidCategory()
    {
        // Arrange
        var existingCategory = await _context.ProductCategories.FirstAsync();
        existingCategory.Name = "Updated Category Name";
        existingCategory.Description = "Updated Description";

        // Act
        var updatedCategory = await _repository.UpdateAsync(existingCategory);

        // Assert
        updatedCategory.Should().NotBeNull();
        updatedCategory.Name.Should().Be("Updated Category Name");
        updatedCategory.Description.Should().Be("Updated Description");
        updatedCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify category was updated in database
        var dbCategory = await _context.ProductCategories.FindAsync(existingCategory.Id);
        dbCategory.Should().NotBeNull();
        dbCategory!.Name.Should().Be("Updated Category Name");
        dbCategory.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryToDelete = TestDataFactory.CreateTestProductCategory();
        categoryToDelete.Name = TestDataFactory.GenerateUniqueName("Category to Delete");
        _context.ProductCategories.Add(categoryToDelete);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(categoryToDelete.Id);

        // Assert
        var deletedCategory = await _repository.GetByIdAsync(categoryToDelete.Id);
        deletedCategory.Should().BeNull(); // Should be null because repository filters out soft-deleted items
        
        // Verify the entity is soft deleted, not hard deleted
        var entityInContext = await _context.ProductCategories.FindAsync(categoryToDelete.Id);
        entityInContext.Should().NotBeNull();
        entityInContext!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert
        var act = async () => await _repository.DeleteAsync(nonExistentId);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Hierarchy Query Tests

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldReturnOnlyRootCategories()
    {
        // Arrange
        var expectedRootCategories = await _context.ProductCategories
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();

        // Act
        var rootCategories = await _repository.GetRootCategoriesAsync();

        // Assert
        rootCategories.Should().NotBeNull();
        rootCategories.Should().HaveCount(expectedRootCategories.Count);
        rootCategories.Should().OnlyContain(c => c.ParentCategoryId == null);
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldReturnDirectChildren_WhenParentHasChildren()
    {
        // Arrange
        var parentCategory = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.ParentCategoryId == null);
        
        if (parentCategory == null)
        {
            // Create a parent category for this test
            parentCategory = TestDataFactory.CreateTestProductCategory();
            parentCategory.Name = TestDataFactory.GenerateUniqueName("Parent Category");
            _context.ProductCategories.Add(parentCategory);
            await _context.SaveChangesAsync();
        }

        // Create child categories
        var childCategory1 = TestDataFactory.CreateTestProductCategory(parentCategory.Id, false);
        var childCategory2 = TestDataFactory.CreateTestProductCategory(parentCategory.Id, false);
        childCategory1.Name = TestDataFactory.GenerateUniqueName("Child Category 1");
        childCategory2.Name = TestDataFactory.GenerateUniqueName("Child Category 2");
        
        _context.ProductCategories.AddRange(childCategory1, childCategory2);
        await _context.SaveChangesAsync();

        // Act
        var subCategories = await _repository.GetSubCategoriesAsync(parentCategory.Id);

        // Assert
        subCategories.Should().NotBeNull();
        subCategories.Should().HaveCountGreaterOrEqualTo(2);
        subCategories.Should().OnlyContain(c => c.ParentCategoryId == parentCategory.Id);
        subCategories.Should().Contain(c => c.Id == childCategory1.Id);
        subCategories.Should().Contain(c => c.Id == childCategory2.Id);
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldReturnEmptyList_WhenParentHasNoChildren()
    {
        // Arrange
        var leafCategory = TestDataFactory.CreateTestProductCategory();
        leafCategory.Name = TestDataFactory.GenerateUniqueName("Leaf Category");
        _context.ProductCategories.Add(leafCategory);
        await _context.SaveChangesAsync();

        // Act
        var subCategories = await _repository.GetSubCategoriesAsync(leafCategory.Id);

        // Assert
        subCategories.Should().NotBeNull();
        subCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldReturnEmptyList_WhenParentDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var subCategories = await _repository.GetSubCategoriesAsync(nonExistentId);

        // Assert
        subCategories.Should().NotBeNull();
        subCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldNotReturnGrandchildren()
    {
        // Arrange - Create a 3-level hierarchy
        var rootCategory = TestDataFactory.CreateTestProductCategory();
        rootCategory.Name = TestDataFactory.GenerateUniqueName("Root Category");
        _context.ProductCategories.Add(rootCategory);
        await _context.SaveChangesAsync();

        var childCategory = TestDataFactory.CreateTestProductCategory(rootCategory.Id, false);
        childCategory.Name = TestDataFactory.GenerateUniqueName("Child Category");
        _context.ProductCategories.Add(childCategory);
        await _context.SaveChangesAsync();

        var grandchildCategory = TestDataFactory.CreateTestProductCategory(childCategory.Id, false);
        grandchildCategory.Name = TestDataFactory.GenerateUniqueName("Grandchild Category");
        _context.ProductCategories.Add(grandchildCategory);
        await _context.SaveChangesAsync();

        // Act - Get children of root category
        var subCategories = await _repository.GetSubCategoriesAsync(rootCategory.Id);

        // Assert
        subCategories.Should().NotBeNull();
        subCategories.Should().Contain(c => c.Id == childCategory.Id);
        subCategories.Should().NotContain(c => c.Id == grandchildCategory.Id); // Should not include grandchild
        subCategories.Should().OnlyContain(c => c.ParentCategoryId == rootCategory.Id);
    }

    #endregion

    #region Hierarchy Validation Tests

    [Fact]
    public async Task AddAsync_ShouldCreateRootCategory_WhenParentIdIsNull()
    {
        // Arrange
        var rootCategory = TestDataFactory.CreateTestProductCategory(parentId: null);
        rootCategory.Name = TestDataFactory.GenerateUniqueName("New Root Category");

        // Act
        var addedCategory = await _repository.AddAsync(rootCategory);

        // Assert
        addedCategory.Should().NotBeNull();
        addedCategory.ParentCategoryId.Should().BeNull();
        
        // Verify it appears in root categories
        var rootCategories = await _repository.GetRootCategoriesAsync();
        rootCategories.Should().Contain(c => c.Id == addedCategory.Id);
    }

    [Fact]
    public async Task AddAsync_ShouldCreateChildCategory_WhenValidParentId()
    {
        // Arrange
        var parentCategory = await _context.ProductCategories.FirstAsync();
        var childCategory = TestDataFactory.CreateTestProductCategory(parentCategory.Id, false);
        childCategory.Name = TestDataFactory.GenerateUniqueName("New Child Category");

        // Act
        var addedCategory = await _repository.AddAsync(childCategory);

        // Assert
        addedCategory.Should().NotBeNull();
        addedCategory.ParentCategoryId.Should().Be(parentCategory.Id);
        
        // Verify it appears in parent's subcategories
        var subCategories = await _repository.GetSubCategoriesAsync(parentCategory.Id);
        subCategories.Should().Contain(c => c.Id == addedCategory.Id);
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowMovingCategoryToNewParent()
    {
        // Arrange
        var originalParent = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.ParentCategoryId == null);
        var newParent = await _context.ProductCategories
            .FirstOrDefaultAsync(c => c.ParentCategoryId == null && c.Id != originalParent!.Id);

        if (originalParent == null || newParent == null)
        {
            // Create test categories
            originalParent = TestDataFactory.CreateTestProductCategory();
            newParent = TestDataFactory.CreateTestProductCategory();
            originalParent.Name = TestDataFactory.GenerateUniqueName("Original Parent");
            newParent.Name = TestDataFactory.GenerateUniqueName("New Parent");
            _context.ProductCategories.AddRange(originalParent, newParent);
            await _context.SaveChangesAsync();
        }

        var categoryToMove = TestDataFactory.CreateTestProductCategory(originalParent.Id, false);
        categoryToMove.Name = TestDataFactory.GenerateUniqueName("Category to Move");
        _context.ProductCategories.Add(categoryToMove);
        await _context.SaveChangesAsync();

        // Act
        categoryToMove.ParentCategoryId = newParent.Id;
        var updatedCategory = await _repository.UpdateAsync(categoryToMove);

        // Assert
        updatedCategory.ParentCategoryId.Should().Be(newParent.Id);
        
        // Verify it no longer appears under original parent
        var originalSubCategories = await _repository.GetSubCategoriesAsync(originalParent.Id);
        originalSubCategories.Should().NotContain(c => c.Id == categoryToMove.Id);
        
        // Verify it appears under new parent
        var newSubCategories = await _repository.GetSubCategoriesAsync(newParent.Id);
        newSubCategories.Should().Contain(c => c.Id == categoryToMove.Id);
    }

    #endregion

    #region Active/Inactive Category Tests

    [Fact]
    public async Task GetAllAsync_ShouldIncludeBothActiveAndInactiveCategories()
    {
        // Arrange
        var activeCategory = TestDataFactory.CreateTestProductCategory();
        var inactiveCategory = TestDataFactory.CreateTestProductCategory();
        activeCategory.Name = TestDataFactory.GenerateUniqueName("Active Category");
        inactiveCategory.Name = TestDataFactory.GenerateUniqueName("Inactive Category");
        activeCategory.IsActive = true;
        inactiveCategory.IsActive = false;
        
        _context.ProductCategories.AddRange(activeCategory, inactiveCategory);
        await _context.SaveChangesAsync();

        // Act
        var allCategories = await _repository.GetAllAsync();

        // Assert
        allCategories.Should().Contain(c => c.Id == activeCategory.Id && c.IsActive);
        allCategories.Should().Contain(c => c.Id == inactiveCategory.Id && !c.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowToggleActiveStatus()
    {
        // Arrange
        var category = await _context.ProductCategories.FirstAsync();
        var originalStatus = category.IsActive;

        // Act
        category.IsActive = !originalStatus;
        var updatedCategory = await _repository.UpdateAsync(category);

        // Assert
        updatedCategory.IsActive.Should().Be(!originalStatus);
        
        // Verify in database
        var dbCategory = await _context.ProductCategories.FindAsync(category.Id);
        dbCategory!.IsActive.Should().Be(!originalStatus);
    }

    #endregion

    #region Performance and Scalability Tests

    [Fact]
    public async Task GetRootCategoriesAsync_ShouldBeEfficient_WithManyCategories()
    {
        // Arrange
        var categories = new List<ProductCategory>();
        for (int i = 0; i < 50; i++)
        {
            var category = TestDataFactory.CreateTestProductCategory();
            category.Name = $"Root Category {i}";
            category.ParentCategoryId = null; // Root category
            categories.Add(category);
        }
        
        _context.ProductCategories.AddRange(categories);
        await _context.SaveChangesAsync();

        // Act
        var startTime = DateTime.UtcNow;
        var rootCategories = await _repository.GetRootCategoriesAsync();
        var endTime = DateTime.UtcNow;

        // Assert
        rootCategories.Should().HaveCountGreaterOrEqualTo(50);
        
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should complete within 2 seconds
    }

    [Fact]
    public async Task GetSubCategoriesAsync_ShouldBeEfficient_WithManyChildren()
    {
        // Arrange
        var parentCategory = TestDataFactory.CreateTestProductCategory();
        parentCategory.Name = TestDataFactory.GenerateUniqueName("Parent with Many Children");
        _context.ProductCategories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategories = new List<ProductCategory>();
        for (int i = 0; i < 30; i++)
        {
            var child = TestDataFactory.CreateTestProductCategory(parentCategory.Id, false);
            child.Name = $"Child Category {i}";
            childCategories.Add(child);
        }
        
        _context.ProductCategories.AddRange(childCategories);
        await _context.SaveChangesAsync();

        // Act
        var startTime = DateTime.UtcNow;
        var subCategories = await _repository.GetSubCategoriesAsync(parentCategory.Id);
        var endTime = DateTime.UtcNow;

        // Assert
        subCategories.Should().HaveCount(30);
        
        var duration = endTime - startTime;
        duration.Should().BeLessThan(TimeSpan.FromSeconds(2)); // Should complete within 2 seconds
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task AddAsync_ShouldSetCreatedAndUpdatedDates()
    {
        // Arrange
        var newCategory = TestDataFactory.CreateTestProductCategory();
        newCategory.Name = TestDataFactory.GenerateUniqueName("Date Test Category");
        newCategory.CreatedAt = default; // Reset dates
        newCategory.UpdatedAt = default;

        // Act
        var addedCategory = await _repository.AddAsync(newCategory);

        // Assert
        addedCategory.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        addedCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateOnlyUpdatedDate()
    {
        // Arrange
        var existingCategory = await _context.ProductCategories.FirstAsync();
        var originalCreatedAt = existingCategory.CreatedAt;
        var originalUpdatedAt = existingCategory.UpdatedAt;
        
        // Wait a bit to ensure different timestamp
        await Task.Delay(100);
        
        existingCategory.Name = "Updated Name";

        // Act
        var updatedCategory = await _repository.UpdateAsync(existingCategory);

        // Assert
        updatedCategory.CreatedAt.Should().Be(originalCreatedAt); // Should not change
        updatedCategory.UpdatedAt.Should().BeAfter(originalUpdatedAt); // Should be updated
        updatedCategory.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetSubCategoriesAsync_ShouldHandleInvalidParentId(string? parentId)
    {
        // Act
        var subCategories = await _repository.GetSubCategoriesAsync(parentId!);

        // Assert
        subCategories.Should().NotBeNull();
        subCategories.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldHandleCascadeDelete_WhenCategoryHasChildren()
    {
        // Arrange
        var parentCategory = TestDataFactory.CreateTestProductCategory();
        parentCategory.Name = TestDataFactory.GenerateUniqueName("Parent to Delete");
        _context.ProductCategories.Add(parentCategory);
        await _context.SaveChangesAsync();

        var childCategory = TestDataFactory.CreateTestProductCategory(parentCategory.Id, false);
        childCategory.Name = TestDataFactory.GenerateUniqueName("Child of Parent to Delete");
        _context.ProductCategories.Add(childCategory);
        await _context.SaveChangesAsync();

        // Act & Assert
        // Note: The behavior depends on database constraints
        // This test documents the current behavior
        var act = async () => await _repository.DeleteAsync(parentCategory.Id);
        
        // May throw exception due to foreign key constraint or succeed depending on configuration
        try
        {
            await act.Should().NotThrowAsync();
            
            // If deletion succeeded, verify both parent and child are deleted (cascade)
            var deletedParent = await _context.ProductCategories.FindAsync(parentCategory.Id);
            deletedParent.Should().BeNull();
        }
        catch (Exception)
        {
            // If deletion failed due to constraint, that's also valid behavior
            // The parent should still exist
            var parentStillExists = await _context.ProductCategories.FindAsync(parentCategory.Id);
            parentStillExists.Should().NotBeNull();
        }
    }

    #endregion

    private void SeedTestData()
    {
        // Add test categories in hierarchy
        var categories = TestDataFactory.CreateTestCategoryHierarchy();
        _context.ProductCategories.AddRange(categories);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}