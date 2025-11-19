using Microsoft.AspNetCore.Mvc;
using TechnicianApi.Core.Interfaces;

namespace TechnicianApi.Api.Controllers;

[Route("/assignment-balances")]
public class AssignmentBalancesController : BaseController
{
    private readonly IAssignmentBalanceService _service;

    public AssignmentBalancesController(IAssignmentBalanceService service)
    {
        _service = service;
    }

    [HttpGet("assignment/{assignmentId}")]
    public async Task<IActionResult> GetBalanceSummary(string assignmentId)
    {
        var summary = await _service.GetBalanceSummaryAsync(assignmentId);
        return summary == null ? NotFound("Balance summary not found") : Ok(summary);
    }

    [HttpPost("assignment/{assignmentId}/calculate")]
    public async Task<IActionResult> CalculateBalance(string assignmentId)
    {
        var summary = await _service.CalculateAndUpdateBalanceAsync(assignmentId);
        return Ok(summary, "Balance calculated successfully");
    }

    [HttpPost("assignment/{assignmentId}/recalculate")]
    public async Task<IActionResult> RecalculateBalance(string assignmentId)
    {
        await _service.RecalculateOnFormChangeAsync(assignmentId);
        var summary = await _service.GetBalanceSummaryAsync(assignmentId);
        return Ok(summary, "Balance recalculated successfully");
    }
}
