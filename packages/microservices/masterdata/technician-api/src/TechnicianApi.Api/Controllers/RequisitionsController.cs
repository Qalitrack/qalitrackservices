using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Requisition;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[ApiController]
[Route("/[controller]")]
public class RequisitionsController : ControllerBase
{
    private readonly IRequisitionService _service;

    public RequisitionsController(IRequisitionService service)
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
    public async Task<IActionResult> Create([FromBody] CreateRequisitionDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRequisitionDto dto)
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

    [HttpPost("{id}/tm-approve")]
    public async Task<IActionResult> TmApprove(
        string id,
        [FromQuery] string tmId,
        [FromQuery] string? comments = null)
    {
        var result = await _service.TmApproveAsync(id, tmId, comments);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/tm-reject")]
    public async Task<IActionResult> TmReject(
        string id,
        [FromQuery] string tmId,
        [FromQuery] string rejectionReason)
    {
        var result = await _service.TmRejectAsync(id, tmId, rejectionReason);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/cfo-approve")]
    public async Task<IActionResult> CfoApprove(
        string id,
        [FromQuery] string cfoId,
        [FromQuery] string? comments = null)
    {
        var result = await _service.CfoApproveAsync(id, cfoId, comments);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/cfo-reject")]
    public async Task<IActionResult> CfoReject(
        string id,
        [FromQuery] string cfoId,
        [FromQuery] string rejectionReason)
    {
        var result = await _service.CfoRejectAsync(id, cfoId, rejectionReason);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost("{id}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(
        string id,
        [FromQuery] string voucherNumber,
        [FromQuery] string referenceNumber)
    {
        var result = await _service.MarkAsPaidAsync(id, voucherNumber, referenceNumber);
        return result == null ? NotFound() : Ok(result);
    }
}
