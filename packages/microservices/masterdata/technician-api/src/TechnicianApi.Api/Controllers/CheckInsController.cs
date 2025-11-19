using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.CheckIn;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class CheckInsController : ControllerBase
{
    private readonly ICheckInService _service;

    public CheckInsController(ICheckInService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckInDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
