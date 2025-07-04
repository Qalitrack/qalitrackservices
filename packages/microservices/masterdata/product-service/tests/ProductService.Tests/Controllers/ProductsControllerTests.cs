using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Api.Controllers;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;
using ProductService.Tests.Helpers;
using Xunit;

namespace ProductService.Tests.Controllers;

[Trait("Category", "Unit")]
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = MockFactories.CreateMockProductService();
        _loggerMock = MockFactories.CreateMockLogger<ProductsController>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    #region GetAllProducts Tests

    [Fact]
    public async Task GetAllProducts_ShouldReturnOkWithProducts_WhenProductsExist()
    {
        // Arrange
        var products = TestDataFactory.CreateTestProductList().Select(p => 
            TestDataFactory.CreateTestProductDto(p.Status)).ToList();
        _productServiceMock.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        result.Should().BeOfType<ActionResult<ApiResponseDto<List<ProductDto>>>>();
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(products.Count);
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnOkWithEmptyList_WhenNoProductsExist()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetAllProductsAsync())
            .ReturnsAsync(new List<ProductDto>());

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        
        var response = okResult!.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _productServiceMock.Setup(x => x.GetAllProductsAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Database error");
    }

    #endregion

    #region GetProduct Tests

    [Fact]
    public async Task GetProduct_ShouldReturnOkWithProduct_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var product = TestDataFactory.CreateTestProductDto();
        product.Id = productId;
        
        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(productId);
    }

    [Fact]
    public async Task GetProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
        
        var response = notFoundResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Product not found");
    }

    [Fact]
    public async Task GetProduct_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetProductByIdAsync(productId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region RegisterProduct Tests

    [Fact]
    public async Task RegisterProduct_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var request = TestDataFactory.CreateValidRegisterProductRequest();
        var createdProduct = TestDataFactory.CreateTestProductDto();
        createdProduct.Name = request.Name;
        createdProduct.Code = request.Code;
        
        _productServiceMock.Setup(x => x.RegisterProductAsync(request))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.RegisterProduct(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be("GetProduct");
        
        var response = createdResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Name.Should().Be(request.Name);
        response.Data.Code.Should().Be(request.Code);
    }

    [Fact]
    public async Task RegisterProduct_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var request = TestDataFactory.CreateValidRegisterProductRequest();
        _productServiceMock.Setup(x => x.RegisterProductAsync(request))
            .ThrowsAsync(new Exception("Validation error"));

        // Act
        var result = await _controller.RegisterProduct(request);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Validation error");
    }

    #endregion

    #region UpdateProduct Tests

    [Fact]
    public async Task UpdateProduct_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidUpdateProductRequest();
        var updatedProduct = TestDataFactory.CreateTestProductDto();
        updatedProduct.Id = productId;
        updatedProduct.Name = request.Name;
        
        _productServiceMock.Setup(x => x.UpdateProductAsync(productId, request))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(productId);
        response.Data.Name.Should().Be(request.Name);
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidUpdateProductRequest();
        _productServiceMock.Setup(x => x.UpdateProductAsync(productId, request))
            .ThrowsAsync(new ArgumentException("Product not found"));

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
        
        var response = notFoundResult.Value as ApiResponseDto<ProductDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Product not found");
    }

    [Fact]
    public async Task UpdateProduct_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidUpdateProductRequest();
        _productServiceMock.Setup(x => x.UpdateProductAsync(productId, request))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateProduct(productId, request);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region DeleteProduct Tests

    [Fact]
    public async Task DeleteProduct_ShouldReturnOk_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.DeleteProductAsync(productId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Product deleted successfully");
    }

    [Fact]
    public async Task DeleteProduct_ShouldReturnInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.DeleteProductAsync(productId))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _controller.DeleteProduct(productId);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        statusCodeResult.Should().NotBeNull();
        statusCodeResult!.StatusCode.Should().Be(500);
        
        var response = statusCodeResult.Value as ApiResponseDto;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Errors.Should().Contain("Delete failed");
    }

    #endregion

    #region GetProductsByCategory Tests

    [Fact]
    public async Task GetProductsByCategory_ShouldReturnOkWithProducts_WhenCategoryHasProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid().ToString();
        var products = TestDataFactory.CreateTestProductList().Select(p => 
            TestDataFactory.CreateTestProductDto(p.Status)).ToList();
        
        _productServiceMock.Setup(x => x.GetProductsByCategoryAsync(categoryId))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetProductsByCategory(categoryId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(products.Count);
    }

    #endregion

    #region GetHazardousProducts Tests

    [Fact]
    public async Task GetHazardousProducts_ShouldReturnOkWithHazardousProducts()
    {
        // Arrange
        var hazardousProducts = TestDataFactory.CreateTestProductList()
            .Where(p => p.IsHazardous)
            .Select(p => TestDataFactory.CreateTestProductDto(p.Status))
            .ToList();
        
        _productServiceMock.Setup(x => x.GetHazardousProductsAsync())
            .ReturnsAsync(hazardousProducts);

        // Act
        var result = await _controller.GetHazardousProducts();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(hazardousProducts.Count);
    }

    #endregion

    #region SearchProducts Tests

    [Fact]
    public async Task SearchProducts_ShouldReturnOkWithResults_WhenValidSearchTerm()
    {
        // Arrange
        var searchTerm = "test";
        var searchResults = TestDataFactory.CreateTestProductList()
            .Select(p => TestDataFactory.CreateTestProductDto(p.Status))
            .ToList();
        
        _productServiceMock.Setup(x => x.SearchProductsAsync(searchTerm))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(searchResults.Count);
    }

    [Fact]
    public async Task SearchProducts_ShouldReturnBadRequest_WhenSearchTermIsEmpty()
    {
        // Act
        var result = await _controller.SearchProducts("");

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
        
        var response = badRequestResult.Value as ApiResponseDto<List<ProductDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Search term is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchProducts_ShouldReturnBadRequest_WhenSearchTermIsInvalid(string searchTerm)
    {
        // Act
        var result = await _controller.SearchProducts(searchTerm);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    #endregion

    #region Product Specifications Tests

    [Fact]
    public async Task GetProductSpecifications_ShouldReturnOkWithSpecifications()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var specifications = new List<ProductSpecificationDto>
        {
            TestDataFactory.CreateTestProductSpecificationDto(productId)
        };
        
        _productServiceMock.Setup(x => x.GetProductSpecificationsAsync(productId))
            .ReturnsAsync(specifications);

        // Act
        var result = await _controller.GetProductSpecifications(productId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductSpecificationDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(specifications.Count);
    }

    [Fact]
    public async Task UpdateProductSpecifications_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidSpecificationsRequest();
        
        _productServiceMock.Setup(x => x.UpdateProductSpecificationsAsync(productId, request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProductSpecifications(productId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Product specifications updated successfully");
    }

    #endregion

    #region Product Pricing Tests

    [Fact]
    public async Task GetProductPricing_ShouldReturnOkWithPricing()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var pricing = new List<ProductPricingDto>
        {
            TestDataFactory.CreateTestProductPricingDto(productId)
        };
        
        _productServiceMock.Setup(x => x.GetProductPricingAsync(productId))
            .ReturnsAsync(pricing);

        // Act
        var result = await _controller.GetProductPricing(productId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<List<ProductPricingDto>>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().HaveCount(pricing.Count);
    }

    [Fact]
    public async Task UpdateProductPricing_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var request = TestDataFactory.CreateValidPricingRequest();
        
        _productServiceMock.Setup(x => x.UpdateProductPricingAsync(productId, request))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdateProductPricing(productId, request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Product pricing updated successfully");
    }

    #endregion

    #region Product Compliance Tests

    [Fact]
    public async Task GetProductCompliance_ShouldReturnOkWithCompliance()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var compliance = TestDataFactory.CreateTestProductComplianceDto(productId);
        
        _productServiceMock.Setup(x => x.GetProductComplianceAsync(productId))
            .ReturnsAsync(compliance);

        // Act
        var result = await _controller.GetProductCompliance(productId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        
        var response = okResult.Value as ApiResponseDto<ProductComplianceDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.ProductId.Should().Be(compliance.ProductId);
    }

    [Fact]
    public async Task GetProductCompliance_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        _productServiceMock.Setup(x => x.GetProductComplianceAsync(productId))
            .ThrowsAsync(new ArgumentException("Product not found"));

        // Act
        var result = await _controller.GetProductCompliance(productId);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
        
        var response = notFoundResult.Value as ApiResponseDto<ProductComplianceDto>;
        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Product not found");
    }

    #endregion
}