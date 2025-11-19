using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailySummariesController : ControllerBase
{
    private readonly IDailySummaryService _service;

    public DailySummariesController(IDailySummaryService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("technician/{technicianId}/date/{date}")]
    public async Task<IActionResult> GetByTechnicianAndDate(string technicianId, DateTime date)
    {
        var result = await _service.GetByTechnicianAndDateAsync(technicianId, date);
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

    [HttpPost("generate")]
    public async Task<IActionResult> Generate(
        [FromQuery] string technicianId,
        [FromQuery] DateTime date)
    {
        var result = await _service.GenerateSummaryAsync(technicianId, date);
        return Ok(result);
    }
}
