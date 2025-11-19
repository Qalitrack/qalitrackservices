using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceMetricsController : ControllerBase
{
    private readonly IPerformanceMetricsService _service;

    public PerformanceMetricsController(IPerformanceMetricsService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("technician/{technicianId}/latest")]
    public async Task<IActionResult> GetLatestByTechnicianId(string technicianId)
    {
        var result = await _service.GetLatestByTechnicianIdAsync(technicianId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? technicianId = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, technicianId);
        return Ok(result);
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> Calculate(
        [FromQuery] string technicianId,
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd)
    {
        var result = await _service.CalculateMetricsAsync(technicianId, periodStart, periodEnd);
        return Ok(result);
    }
}
