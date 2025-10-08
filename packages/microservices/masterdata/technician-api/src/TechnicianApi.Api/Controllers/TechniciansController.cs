using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[Route("api/[controller]")]
public class TechniciansController : BaseController
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechniciansController> _logger;

    public TechniciansController(ITechnicianService technicianService, ILogger<TechniciansController> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    /// <summary>
    /// Get all technicians
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var technicians = await _technicianService.GetAllAsync();
            return Ok(technicians);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all technicians");
            return InternalServerError("An error occurred while retrieving technicians");
        }
    }

    /// <summary>
    /// Get technician by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        try
        {
            var technician = await _technicianService.GetByIdAsync(id);
            if (technician == null)
            {
                return NotFound("Technician not found");
            }

            return Ok(technician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting technician with id {Id}", id);
            return InternalServerError("An error occurred while retrieving technician");
        }
    }

    /// <summary>
    /// Create a new technician
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTechnicianDto request)
    {
        try
        {
            var technician = await _technicianService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = technician.Id }, technician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating technician");
            return InternalServerError("An error occurred while creating technician");
        }
    }

    /// <summary>
    /// Update an existing technician
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTechnicianDto request)
    {
        try
        {
            var technician = await _technicianService.UpdateAsync(id, request);
            if (technician == null)
            {
                return NotFound("Technician not found");
            }

            return Ok(technician, "Technician updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating technician with id {Id}", id);
            return InternalServerError("An error occurred while updating technician");
        }
    }

    /// <summary>
    /// Delete a technician
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var result = await _technicianService.DeleteAsync(id);
            if (!result)
            {
                return NotFound("Technician not found");
            }

            return Ok<object?>(null, "Technician deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting technician with id {Id}", id);
            return InternalServerError("An error occurred while deleting technician");
        }
    }

    /// <summary>
    /// Check if technician name is available
    /// </summary>
    [HttpGet("check-name/{name}")]
    public async Task<IActionResult> CheckName(string name)
    {
        try
        {
            var available = await _technicianService.IsNameAvailableAsync(name);
            return Ok(new { available }, available ? "Name is available" : "Name is not available");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking technician name availability");
            return InternalServerError("An error occurred while checking name");
        }
    }
}