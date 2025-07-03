using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products/categories")]
public class ProductCategoriesController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductCategoriesController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Get all product categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<ProductCategoryDto>>>> GetAllCategories()
    {
        try
        {
            var categories = await _productService.GetAllCategoriesAsync();
            return Ok(ApiResponseDto<List<ProductCategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductCategoryDto>>.ErrorResponse("An error occurred while retrieving categories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get root categories (categories without parent)
    /// </summary>
    [HttpGet("root")]
    public async Task<ActionResult<ApiResponseDto<List<ProductCategoryDto>>>> GetRootCategories()
    {
        try
        {
            var categories = await _productService.GetRootCategoriesAsync();
            return Ok(ApiResponseDto<List<ProductCategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductCategoryDto>>.ErrorResponse("An error occurred while retrieving root categories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get subcategories for a parent category
    /// </summary>
    [HttpGet("{parentCategoryId}/subcategories")]
    public async Task<ActionResult<ApiResponseDto<List<ProductCategoryDto>>>> GetSubCategories(string parentCategoryId)
    {
        try
        {
            var categories = await _productService.GetSubCategoriesAsync(parentCategoryId);
            return Ok(ApiResponseDto<List<ProductCategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<List<ProductCategoryDto>>.ErrorResponse("An error occurred while retrieving subcategories", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Create a new product category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProductCategoryDto>>> CreateCategory([FromBody] CreateProductCategoryRequest request)
    {
        try
        {
            var category = await _productService.CreateCategoryAsync(request);
            return CreatedAtAction(nameof(GetAllCategories), null, 
                ApiResponseDto<ProductCategoryDto>.SuccessResponse(category, "Category created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponseDto<ProductCategoryDto>.ErrorResponse("An error occurred while creating the category", new List<string> { ex.Message }));
        }
    }
}