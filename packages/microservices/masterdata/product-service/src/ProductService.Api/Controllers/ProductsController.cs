using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[Route("api/[controller]")]
public class ProductsController : BaseController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<ProductDto>>>> GetAllProducts()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponseDto<List<ProductDto>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductDto>>.ErrorResponse("An error occurred while retrieving products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> GetProduct(string id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponseDto<ProductDto>.ErrorResponse("Product not found"));
            }
            return Ok(ApiResponseDto<ProductDto>.SuccessResponse(product));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<ProductDto>.ErrorResponse("An error occurred while retrieving the product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Register a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> RegisterProduct([FromBody] RegisterProductRequest request)
    {
        try
        {
            var product = await _productService.RegisterProductAsync(request);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                ApiResponseDto<ProductDto>.SuccessResponse(product, "Product registered successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<ProductDto>.ErrorResponse("An error occurred while registering the product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update a product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<ProductDto>>> UpdateProduct(string id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            var product = await _productService.UpdateProductAsync(id, request);
            return Ok(ApiResponseDto<ProductDto>.SuccessResponse(product, "Product updated successfully"));
        }
        catch (ArgumentException)
        {
            return NotFound(ApiResponseDto<ProductDto>.ErrorResponse("Product not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<ProductDto>.ErrorResponse("An error occurred while updating the product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteProduct(string id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return Ok(ApiResponseDto.SuccessResponse("Product deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.ErrorResponse("An error occurred while deleting the product", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<ApiResponseDto<List<ProductDto>>>> GetProductsByCategory(string categoryId)
    {
        try
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(ApiResponseDto<List<ProductDto>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductDto>>.ErrorResponse("An error occurred while retrieving products by category", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get hazardous products
    /// </summary>
    [HttpGet("hazmat")]
    public async Task<ActionResult<ApiResponseDto<List<ProductDto>>>> GetHazardousProducts()
    {
        try
        {
            var products = await _productService.GetHazardousProductsAsync();
            return Ok(ApiResponseDto<List<ProductDto>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductDto>>.ErrorResponse("An error occurred while retrieving hazardous products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Search products
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponseDto<List<ProductDto>>>> SearchProducts([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(ApiResponseDto<List<ProductDto>>.ErrorResponse("Search term is required"));
            }

            var products = await _productService.SearchProductsAsync(searchTerm);
            return Ok(ApiResponseDto<List<ProductDto>>.SuccessResponse(products));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductDto>>.ErrorResponse("An error occurred while searching products", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get product specifications
    /// </summary>
    [HttpGet("{id}/specifications")]
    public async Task<ActionResult<ApiResponseDto<List<ProductSpecificationDto>>>> GetProductSpecifications(string id)
    {
        try
        {
            var specifications = await _productService.GetProductSpecificationsAsync(id);
            return Ok(ApiResponseDto<List<ProductSpecificationDto>>.SuccessResponse(specifications));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductSpecificationDto>>.ErrorResponse("An error occurred while retrieving product specifications", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update product specifications
    /// </summary>
    [HttpPut("{id}/specifications")]
    public async Task<ActionResult<ApiResponseDto>> UpdateProductSpecifications(string id, [FromBody] UpdateProductSpecificationsRequest request)
    {
        try
        {
            await _productService.UpdateProductSpecificationsAsync(id, request);
            return Ok(ApiResponseDto.SuccessResponse("Product specifications updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.ErrorResponse("An error occurred while updating product specifications", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get product pricing
    /// </summary>
    [HttpGet("{id}/pricing")]
    public async Task<ActionResult<ApiResponseDto<List<ProductPricingDto>>>> GetProductPricing(string id)
    {
        try
        {
            var pricing = await _productService.GetProductPricingAsync(id);
            return Ok(ApiResponseDto<List<ProductPricingDto>>.SuccessResponse(pricing));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductPricingDto>>.ErrorResponse("An error occurred while retrieving product pricing", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Update product pricing
    /// </summary>
    [HttpPut("{id}/pricing")]
    public async Task<ActionResult<ApiResponseDto>> UpdateProductPricing(string id, [FromBody] UpdateProductPricingRequest request)
    {
        try
        {
            await _productService.UpdateProductPricingAsync(id, request);
            return Ok(ApiResponseDto.SuccessResponse("Product pricing updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto.ErrorResponse("An error occurred while updating product pricing", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get product compliance information
    /// </summary>
    [HttpGet("{id}/compliance")]
    public async Task<ActionResult<ApiResponseDto<ProductComplianceDto>>> GetProductCompliance(string id)
    {
        try
        {
            var compliance = await _productService.GetProductComplianceAsync(id);
            return Ok(ApiResponseDto<ProductComplianceDto>.SuccessResponse(compliance));
        }
        catch (ArgumentException)
        {
            return NotFound(ApiResponseDto<ProductComplianceDto>.ErrorResponse("Product not found"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<ProductComplianceDto>.ErrorResponse("An error occurred while retrieving product compliance", new List<string> { ex.Message }));
        }
    }
}