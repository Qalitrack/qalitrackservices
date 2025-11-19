using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Photo;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotosController : ControllerBase
{
    private readonly IPhotoService _service;

    public PhotosController(IPhotoService service)
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
        return Ok(result);
    }

    [HttpGet("assignment/{assignmentId}/type/{type}")]
    public async Task<IActionResult> GetByAssignmentAndType(string assignmentId, string type)
    {
        var result = await _service.GetByAssignmentAndTypeAsync(assignmentId, type);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePhotoDto dto)
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
