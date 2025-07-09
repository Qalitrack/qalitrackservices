using Microsoft.AspNetCore.Mvc;
using Abuso.Core.DTOs;
using Abuso.Core.Interfaces;

namespace Abuso.Api.Controllers;

[Route("api/[controller]")]
public class AbusosController : BaseController
{
    private readonly IAbusoService _abusoService;
    private readonly ILogger<AbusosController> _logger;

    public AbusosController(IAbusoService abusoService, ILogger<AbusosController> logger)
    {
        _abusoService = abusoService;
        _logger = logger;
    }

    /// <summary>
    /// Get all abusos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var abusos = await _abusoService.GetAllAsync();
            return Ok(abusos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all abusos");
            return InternalServerError("An error occurred while retrieving abusos");
        }
    }

    /// <summary>
    /// Get abuso by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var abuso = await _abusoService.GetByIdAsync(id);
            if (abuso == null)
            {
                return NotFound("Abuso not found");
            }

            return Ok(abuso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting abuso with id {Id}", id);
            return InternalServerError("An error occurred while retrieving abuso");
        }
    }

    /// <summary>
    /// Create a new abuso
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAbusoDto request)
    {
        try
        {
            var abuso = await _abusoService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = abuso.Id }, abuso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating abuso");
            return InternalServerError("An error occurred while creating abuso");
        }
    }

    /// <summary>
    /// Update an existing abuso
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateAbusoDto request)
    {
        try
        {
            var abuso = await _abusoService.UpdateAsync(id, request);
            if (abuso == null)
            {
                return NotFound("Abuso not found");
            }

            return Ok(abuso, "Abuso updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating abuso with id {Id}", id);
            return InternalServerError("An error occurred while updating abuso");
        }
    }

    /// <summary>
    /// Delete a abuso
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _abusoService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Abuso not found");
            }

            return Ok<object?>(null, "Abuso deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting abuso with id {Id}", id);
            return InternalServerError("An error occurred while deleting abuso");
        }
    }

    /// <summary>
    /// Check if abuso name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _abusoService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking abuso name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}