using Microsoft.AspNetCore.Mvc;
using Masterdata.Core.DTOs;
using Masterdata.Core.Interfaces;

namespace Masterdata.Api.Controllers;

[Route("api/[controller]")]
public class BasesController : BaseController
{
    private readonly IBaseService _baseService;
    private readonly ILogger<BasesController> _logger;

    public BasesController(IBaseService baseService, ILogger<BasesController> logger)
    {
        _baseService = baseService;
        _logger = logger;
    }

    /// <summary>
    /// Get all bases
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var bases = await _baseService.GetAllAsync();
            return Ok(bases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bases");
            return InternalServerError("An error occurred while retrieving bases");
        }
    }

    /// <summary>
    /// Get base by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var base = await _baseService.GetByIdAsync(id);
            if (base == null)
            {
                return NotFound("Base not found");
            }

            return Ok(base);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting base with id {Id}", id);
            return InternalServerError("An error occurred while retrieving base");
        }
    }

    /// <summary>
    /// Create a new base
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBaseDto request)
    {
        try
        {
            var base = await _baseService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = base.Id }, base);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating base");
            return InternalServerError("An error occurred while creating base");
        }
    }

    /// <summary>
    /// Update an existing base
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateBaseDto request)
    {
        try
        {
            var base = await _baseService.UpdateAsync(id, request);
            if (base == null)
            {
                return NotFound("Base not found");
            }

            return Ok(base, "Base updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating base with id {Id}", id);
            return InternalServerError("An error occurred while updating base");
        }
    }

    /// <summary>
    /// Delete a base
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _baseService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Base not found");
            }

            return Ok<object?>(null, "Base deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting base with id {Id}", id);
            return InternalServerError("An error occurred while deleting base");
        }
    }

    /// <summary>
    /// Check if base name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _baseService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking base name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}