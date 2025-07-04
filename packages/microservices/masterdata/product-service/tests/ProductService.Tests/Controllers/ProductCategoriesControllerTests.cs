using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Api.Controllers;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Controllers;

[Trait("Category", "Unit")]
public class ProductCategoriesControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductCategoriesController _controller;

    public ProductCategoriesControllerTests()
    {
        _productServiceMock = MockFactories.CreateMockProductService();
        _controller = new ProductCategoriesController(_productServiceMock.Object);
    }

    #region GetAllCategories Tests

    [Fact]
    public async Task GetAllCategories_ShouldReturnOkWithCategories_WhenCategoriesExist()
    {
        // Arrange
        var categories = TestDataFactory.CreateTestCategoryHierarchy()
            .Select(c => TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId))
            .ToList();
        
        _productServiceMock.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        result.Should().BeOfType<ActionResult<ApiResponseDto<List<ProductCategoryDto>>>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(categories.Count);
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnOkWithEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        
        var response = okResult!.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetAllCategoriesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Database error");
    }

    #endregion

    #region GetRootCategories Tests

    [Fact]
    public async Task GetRootCategories_ShouldReturnOkWithRootCategories_WhenRootCategoriesExist()
    {
        // Arrange
        var rootCategories = TestDataFactory.CreateTestCategoryHierarchy()
            .Where(c => c.ParentCategoryId == null)
            .Select(c => TestDataFactory.CreateTestProductCategoryDto(c.ParentCategoryId))
            .ToList();
        
        _productServiceMock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(rootCategories);

        // Act
        var result = await _controller.GetRootCategories();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(rootCategories.Count);
        response.Data.Should().OnlyContain(c => c.ParentCategoryId == null);
    }

    [Fact]
    public async Task GetRootCategories_ShouldReturnOkWithEmptyList_WhenNoRootCategoriesExist()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        var result = await _controller.GetRootCategories();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        
        var response = okResult!.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRootCategories_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetRootCategoriesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetRootCategories();

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetSubCategories Tests

    [Fact]
    public async Task GetSubCategories_ShouldReturnOkWithSubCategories_WhenSubCategoriesExist()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid().ToString();
        
        // Create specific subcategories for the test parent
        var subCategories = new List<ProductCategoryDto>
        {
            TestDataFactory.CreateTestProductCategoryDto(parentCategoryId),
            TestDataFactory.CreateTestProductCategoryDto(parentCategoryId)
        };
        
        // Ensure they have unique names
        subCategories[0].Name = "Sub Category 1";
        subCategories[1].Name = "Sub Category 2";
        
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(parentCategoryId))
            .ReturnsAsync(subCategories);

        // Act
        var result = await _controller.GetSubCategories(parentCategoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(subCategories.Count);
        response.Data.Should().OnlyContain(c => c.ParentCategoryId == parentCategoryId);
    }

    [Fact]
    public async Task GetSubCategories_ShouldReturnOkWithEmptyList_WhenNoSubCategoriesExist()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(parentCategoryId))
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        var result = await _controller.GetSubCategories(parentCategoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        
        var response = okResult!.Value as ApiResponseDto<List<ProductCategoryDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSubCategories_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(parentCategoryId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetSubCategories(parentCategoryId);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-guid")]
    [InlineData("12345")]
    public async Task GetSubCategories_ShouldHandleVariousParentCategoryIds(string parentCategoryId)
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(parentCategoryId))
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        var result = await _controller.GetSubCategories(parentCategoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    #endregion

    #region CreateCategory Tests

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var request = TestDataFactory.CreateValidCategoryRequest();
        var createdCategory = TestDataFactory.CreateTestProductCategoryDto(request.ParentCategoryId);
        createdCategory.Name = request.Name;
        createdCategory.Description = request.Description;
        
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be("GetAllCategories");
        
        var response = createdResult.Value as ApiResponseDto<ProductCategoryDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be(request.Name);
        response.Data.Description.Should().Be(request.Description);
        response.Message.Should().Be("Category created successfully");
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenValidRootCategoryRequest()
    {
        // Arrange
        var request = TestDataFactory.CreateValidCategoryRequest(parentId: null);
        var createdCategory = TestDataFactory.CreateTestProductCategoryDto(null);
        createdCategory.Name = request.Name;
        createdCategory.Description = request.Description;
        
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        
        var response = createdResult.Value as ApiResponseDto<ProductCategoryDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ParentCategoryId.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnCreated_WhenValidSubCategoryRequest()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidCategoryRequest(parentId: parentCategoryId);
        var createdCategory = TestDataFactory.CreateTestProductCategoryDto(parentCategoryId);
        createdCategory.Name = request.Name;
        createdCategory.Description = request.Description;
        
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ReturnsAsync(createdCategory);

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        
        var response = createdResult.Value as ApiResponseDto<ProductCategoryDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ParentCategoryId.Should().Be(parentCategoryId);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var request = TestDataFactory.CreateValidCategoryRequest();
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ThrowsAsync(new Exception("Validation error"));

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto<ProductCategoryDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Validation error");
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnInternalServerError_WhenDatabaseConstraintViolation()
    {
        // Arrange
        var request = TestDataFactory.CreateValidCategoryRequest();
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ThrowsAsync(new InvalidOperationException("Category name already exists"));

        // Act
        var result = await _controller.CreateCategory(request);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto<ProductCategoryDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Category name already exists");
    }

    #endregion

    #region Edge Cases and Error Handling Tests

    [Fact]
    public async Task CreateCategory_ShouldHandleNullRequest()
    {
        // Arrange
        CreateProductCategoryRequest? request = null;
        
        _productServiceMock.Setup(x => x.CreateCategoryAsync(It.IsAny<CreateProductCategoryRequest>()))
            .ThrowsAsync(new ArgumentNullException(nameof(request)));

        // Act
        var result = await _controller.CreateCategory(request!);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetSubCategories_ShouldHandleNullParentCategoryId()
    {
        // Arrange
        string? parentCategoryId = null;
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        var result = await _controller.GetSubCategories(parentCategoryId!);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    #endregion

    #region Service Method Verification Tests

    [Fact]
    public async Task GetAllCategories_ShouldCallServiceMethod()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        await _controller.GetAllCategories();

        // Assert
        _productServiceMock.Verify(x => x.GetAllCategoriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRootCategories_ShouldCallServiceMethod()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetRootCategoriesAsync())
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        await _controller.GetRootCategories();

        // Assert
        _productServiceMock.Verify(x => x.GetRootCategoriesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetSubCategories_ShouldCallServiceMethodWithCorrectParameter()
    {
        // Arrange
        var parentCategoryId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetSubCategoriesAsync(parentCategoryId))
            .ReturnsAsync(new List<ProductCategoryDto>());

        // Act
        await _controller.GetSubCategories(parentCategoryId);

        // Assert
        _productServiceMock.Verify(x => x.GetSubCategoriesAsync(parentCategoryId), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ShouldCallServiceMethodWithCorrectParameter()
    {
        // Arrange
        var request = TestDataFactory.CreateValidCategoryRequest();
        var createdCategory = TestDataFactory.CreateTestProductCategoryDto();
        
        _productServiceMock.Setup(x => x.CreateCategoryAsync(request))
            .ReturnsAsync(createdCategory);

        // Act
        await _controller.CreateCategory(request);

        // Assert
        _productServiceMock.Verify(x => x.CreateCategoryAsync(request), Times.Once);
    }

    #endregion
}