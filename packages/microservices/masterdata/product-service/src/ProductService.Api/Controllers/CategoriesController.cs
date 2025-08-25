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
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
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
    public async Task<IActionResult> GetHierarchy()
    {
        try
        {
            var hierarchy = await _categoryService.GetRootCategoriesAsync();
            return Ok(hierarchy);
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
    public async Task<IActionResult> GetSubCategories(string parentId)
    {
        try
        {
            var subCategories = await _categoryService.GetSubCategoriesAsync(parentId);
            return Ok(subCategories);
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
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            return Ok(category);
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
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var category = await _categoryService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
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
    public async Task<IActionResult> Update(string id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid model state", GetModelStateErrors());
            }

            var category = await _categoryService.UpdateAsync(id, dto);
            if (category == null)
            {
                return NotFound("Category not found");
            }

            return Ok(category);
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
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _categoryService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Category not found");
            }

            return Ok(true);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}