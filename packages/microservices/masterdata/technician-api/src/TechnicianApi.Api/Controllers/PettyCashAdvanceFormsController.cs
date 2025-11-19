using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.PettyCash;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[Route("api/petty-cash-advance-forms")]
public class PettyCashAdvanceFormsController : BaseController
{
    private readonly IPettyCashAdvanceFormService _service;

    public PettyCashAdvanceFormsController(IPettyCashAdvanceFormService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var form = await _service.GetByIdAsync(id);
        return form == null ? NotFound("Petty cash advance form not found") : Ok(form);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePettyCashAdvanceFormDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Created(created, "Petty cash advance form created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePettyCashAdvanceFormDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound("Petty cash advance form not found") : Ok(updated, "Petty cash advance form updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok("Petty cash advance form deleted successfully") : NotFound("Petty cash advance form not found");
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(string id, [FromQuery] string approvedBy, [FromQuery] string? comments = null)
    {
        var approved = await _service.ApproveAsync(id, approvedBy, comments);
        return approved == null ? NotFound("Petty cash advance form not found or cannot be approved") : Ok(approved, "Petty cash advance form approved successfully");
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(string id, [FromQuery] string rejectedBy, [FromQuery] string rejectionReason)
    {
        var rejected = await _service.RejectAsync(id, rejectedBy, rejectionReason);
        return rejected == null ? NotFound("Petty cash advance form not found or cannot be rejected") : Ok(rejected, "Petty cash advance form rejected");
    }

    [HttpPost("{id}/disburse")]
    public async Task<IActionResult> Disburse(string id, [FromQuery] string disbursedBy, [FromQuery] string voucherNumber, [FromQuery] string? referenceNumber = null)
    {
        var disbursed = await _service.DisburseAsync(id, disbursedBy, voucherNumber, referenceNumber);
        return disbursed == null ? NotFound("Petty cash advance form not found or cannot be disbursed") : Ok(disbursed, "Petty cash advance form disbursed successfully");
    }
}
