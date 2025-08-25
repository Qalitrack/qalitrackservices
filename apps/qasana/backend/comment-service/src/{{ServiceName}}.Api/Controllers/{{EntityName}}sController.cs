using Microsoft.AspNetCore.Mvc;
using {{ServiceName}}.Core.DTOs;
using {{ServiceName}}.Core.Interfaces;

namespace {{ServiceName}}.Api.Controllers;

[Route("api/[controller]")]
public class {{EntityName}}sController : BaseController
{
    private readonly I{{EntityName}}Service _{{entityName}}Service;
    private readonly ILogger<{{EntityName}}sController> _logger;

    public {{EntityName}}sController(I{{EntityName}}Service {{entityName}}Service, ILogger<{{EntityName}}sController> logger)
    {
        _{{entityName}}Service = {{entityName}}Service;
        _logger = logger;
    }

    /// <summary>
    /// Get all {{entityName}}s
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var {{entityName}}s = await _{{entityName}}Service.GetAllAsync();
            return Ok({{entityName}}s);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {{entityName}}s");
            return InternalServerError("An error occurred while retrieving {{entityName}}s");
        }
    }

    /// <summary>
    /// Get {{entityName}} by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var {{entityName}} = await _{{entityName}}Service.GetByIdAsync(id);
            if ({{entityName}} == null)
            {
                return NotFound("{{EntityName}} not found");
            }

            return Ok({{entityName}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {{entityName}} with id {Id}", id);
            return InternalServerError("An error occurred while retrieving {{entityName}}");
        }
    }

    /// <summary>
    /// Create a new {{entityName}}
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Create{{EntityName}}Dto request)
    {
        try
        {
            var {{entityName}} = await _{{entityName}}Service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = {{entityName}}.Id }, {{entityName}});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {{entityName}}");
            return InternalServerError("An error occurred while creating {{entityName}}");
        }
    }

    /// <summary>
    /// Update an existing {{entityName}}
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Update{{EntityName}}Dto request)
    {
        try
        {
            var {{entityName}} = await _{{entityName}}Service.UpdateAsync(id, request);
            if ({{entityName}} == null)
            {
                return NotFound("{{EntityName}} not found");
            }

            return Ok({{entityName}}, "{{EntityName}} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {{entityName}} with id {Id}", id);
            return InternalServerError("An error occurred while updating {{entityName}}");
        }
    }

    /// <summary>
    /// Delete a {{entityName}}
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _{{entityName}}Service.DeleteAsync(id);
            if (!result)
            {
                return NotFound("{{EntityName}} not found");
            }

            return Ok<object?>(null, "{{EntityName}} deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {{entityName}} with id {Id}", id);
            return InternalServerError("An error occurred while deleting {{entityName}}");
        }
    }

    /// <summary>
    /// Check if {{entityName}} name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _{{entityName}}Service.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking {{entityName}} name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}