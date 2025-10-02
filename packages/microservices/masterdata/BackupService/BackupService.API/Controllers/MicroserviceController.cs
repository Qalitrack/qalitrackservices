using BackupService.Core.Dtos;
using BackupService.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackupService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class MicroserviceController : ControllerBase
{
    private readonly IMicroService _microService;
    private readonly ILogger<MicroserviceController> _logger;

    public MicroserviceController(IMicroService microService, ILogger<MicroserviceController> logger)
    {
        _microService = microService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new microservice
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMicroservice([FromBody] MicroserviceRequest request, CancellationToken ct = default)
    {
        try
        {
            var microservice = await _microService.CreateMicroserviceAsync(request, ct);
            return CreatedAtAction(nameof(GetMicroservice), new { name = microservice.Name }, microservice);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for creating microservice");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Microservice already exists");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating microservice");
            return StatusCode(500, "An error occurred while creating the microservice");
        }
    }

    /// <summary>
    /// Get a microservice by name
    /// </summary>
    [HttpGet("{name}")]
    public async Task<IActionResult> GetMicroservice(string name, CancellationToken ct = default)
    {
        try
        {
            var microservice = await _microService.GetMicroserviceAsync(name, ct);
            return Ok(microservice);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Microservice not found: {Name}", name);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting microservice: {Name}", name);
            return StatusCode(500, "An error occurred while retrieving the microservice");
        }
    }

    /// <summary>
    /// Get all microservices
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllMicroservices(CancellationToken ct = default)
    {
        try
        {
            var microservices = await _microService.GetAllMicroservicesAsync(ct);
            return Ok(microservices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all microservices");
            return StatusCode(500, "An error occurred while retrieving microservices");
        }
    }

    /// <summary>
    /// Update a microservice
    /// </summary>
    [HttpPut("{name}")]
    public async Task<IActionResult> UpdateMicroservice(string name, [FromBody] MicroserviceRequest request, CancellationToken ct = default)
    {
        try
        {
            var microservice = await _microService.UpdateMicroserviceAsync(name, request, ct);
            return Ok(microservice);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request for updating microservice");
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Microservice not found: {Name}", name);
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating microservice");
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating microservice: {Name}", name);
            return StatusCode(500, "An error occurred while updating the microservice");
        }
    }

    /// <summary>
    /// Delete a microservice
    /// </summary>
    [HttpDelete("{name}")]
    public async Task<IActionResult> DeleteMicroservice(string name, CancellationToken ct = default)
    {
        try
        {
            await _microService.DeleteMicroserviceAsync(name, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Microservice not found: {Name}", name);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting microservice: {Name}", name);
            return StatusCode(500, "An error occurred while deleting the microservice");
        }
    }
}