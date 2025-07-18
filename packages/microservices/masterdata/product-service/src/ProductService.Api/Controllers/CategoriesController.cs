using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;
using ProductService.Core.Interfaces;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(ApiResponseDto<IEnumerable<CategoryDto>>.Success(categories));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get category hierarchy (root categories with subcategories)
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetHierarchy()
    {
        try
        {
            var hierarchy = await _categoryService.GetHierarchyAsync();
            return Ok(ApiResponseDto<IEnumerable<CategoryDto>>.Success(hierarchy));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get subcategories for a parent category
    /// </summary>
    [HttpGet("{parentId}/subcategories")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<CategoryDto>>>> GetSubCategories(string parentId)
    {
        try
        {
            var subCategories = await _categoryService.GetSubCategoriesAsync(parentId);
            return Ok(ApiResponseDto<IEnumerable<CategoryDto>>.Success(subCategories));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<CategoryDto>>> GetById(string id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponseDto<CategoryDto>.Error("Category not found"));
            }

            return Ok(ApiResponseDto<CategoryDto>.Success(category));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<CategoryDto>>> Create([FromBody] CategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<CategoryDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, 
                ApiResponseDto<CategoryDto>.Success(category));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<CategoryDto>>> Update(string id, [FromBody] CategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponseDto<CategoryDto>.Error("Invalid model state", GetModelStateErrors()));
            }

            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null)
            {
                return NotFound(ApiResponseDto<CategoryDto>.Error("Category not found"));
            }

            return Ok(ApiResponseDto<CategoryDto>.Success(category));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Delete(string id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(ApiResponseDto<bool>.Error("Category not found"));
            }

            return Ok(ApiResponseDto<bool>.Success(true));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}