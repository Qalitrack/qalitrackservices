using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.DTOs.Refund;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[Route("api/refunds")]
public class RefundsController : BaseController
{
    private readonly IRefundService _service;

    public RefundsController(IRefundService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var refund = await _service.GetByIdAsync(id);
        return refund == null ? NotFound("Refund not found") : Ok(refund);
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetByAssignmentId(string assignmentId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _service.GetByAssignmentIdAsync(assignmentId, pageNumber, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRefundDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return Created(created, "Refund created successfully");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateRefundDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated == null ? NotFound("Refund not found or cannot be updated") : Ok(updated, "Refund updated successfully");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? Ok("Refund deleted successfully") : NotFound("Refund not found");
    }

    [HttpPost("{id}/manager-approve")]
    public async Task<IActionResult> ManagerApprove(string id, [FromBody] ApproveRefundDto dto)
    {
        var approved = await _service.ManagerApproveAsync(id, dto);
        return approved == null ? NotFound("Refund not found or cannot be approved") : Ok(approved, "Refund approved by manager");
    }

    [HttpPost("{id}/manager-reject")]
    public async Task<IActionResult> ManagerReject(string id, [FromBody] RejectRefundDto dto)
    {
        var rejected = await _service.ManagerRejectAsync(id, dto);
        return rejected == null ? NotFound("Refund not found or cannot be rejected") : Ok(rejected, "Refund rejected by manager");
    }

    [HttpPost("{id}/cfo-confirm-received")]
    public async Task<IActionResult> CfoConfirmReceived(string id, [FromBody] ConfirmRefundReceivedDto dto)
    {
        var confirmed = await _service.CfoConfirmReceivedAsync(id, dto);
        return confirmed == null ? NotFound("Refund not found or cannot be confirmed") : Ok(confirmed, "Refund confirmed received by CFO");
    }

    [HttpPost("{id}/cfo-reject")]
    public async Task<IActionResult> CfoReject(string id, [FromBody] RejectRefundDto dto)
    {
        var rejected = await _service.CfoRejectAsync(id, dto);
        return rejected == null ? NotFound("Refund not found or cannot be rejected") : Ok(rejected, "Refund rejected by CFO");
    }
}
