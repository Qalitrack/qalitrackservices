using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.ServiceReport;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceReportsController : ControllerBase
{
    private readonly IServiceReportService _service;

    public ServiceReportsController(IServiceReportService service)
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

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? technicianId = null,
        [FromQuery] string? status = null)
    {
        var result = await _service.GetPagedAsync(pageNumber, pageSize, technicianId, status);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceReportDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateServiceReportDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(string id)
    {
        var result = await _service.SubmitReportAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromQuery] string approvedBy)
    {
        var result = await _service.ApproveReportAsync(id, approvedBy);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromQuery] string rejectionReason)
    {
        var result = await _service.RejectReportAsync(id, rejectionReason);
        return result == null ? NotFound() : Ok(result);
    }
}
