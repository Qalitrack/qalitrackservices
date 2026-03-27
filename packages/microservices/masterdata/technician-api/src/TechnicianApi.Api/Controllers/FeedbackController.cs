using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Feedback;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _service;

    public FeedbackController(IFeedbackService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? userId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? feedbackType = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, userId, status, feedbackType);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateFeedbackDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/respond")]
    public async Task<IActionResult> Respond(string id, [FromBody] RespondToFeedbackDto dto)
    {
        // TODO: Get respondedBy from authenticated user context
        var respondedBy = "admin"; // Placeholder
        var result = await _service.RespondToFeedbackAsync(id, dto, respondedBy);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/resolve")]
    public async Task<IActionResult> Resolve(string id)
    {
        var result = await _service.ResolveAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id)
    {
        var result = await _service.RejectAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }
}
