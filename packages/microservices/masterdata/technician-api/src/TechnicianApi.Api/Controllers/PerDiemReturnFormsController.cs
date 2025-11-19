using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.PerDiemReturn;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[Route("/per-diem-return-forms")]
public class PerDiemReturnFormsController : BaseController
{
    private readonly IPerDiemReturnFormService _service;

    public PerDiemReturnFormsController(IPerDiemReturnFormService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var form = await _service.GetByIdAsync(id);
        return form == null ? NotFound("Per diem return form not found") : Ok(form);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePerDiemReturnFormDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Created(created, "Per diem return form created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePerDiemReturnFormDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound("Per diem return form not found") : Ok(updated, "Per diem return form updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok("Per diem return form deleted successfully") : NotFound("Per diem return form not found");
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromQuery] string approvedBy, [FromQuery] string? comments = null)
    {
        var approved = await _service.ApproveAsync(id, approvedBy, comments);
        return approved == null ? NotFound("Per diem return form not found or cannot be approved") : Ok(approved, "Per diem return form approved successfully");
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromQuery] string rejectedBy, [FromQuery] string rejectionReason)
    {
        var rejected = await _service.RejectAsync(id, rejectedBy, rejectionReason);
        return rejected == null ? NotFound("Per diem return form not found or cannot be rejected") : Ok(rejected, "Per diem return form rejected");
    }
}
