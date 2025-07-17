using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StateController : BaseController
{
    private readonly IStateService _stateService;
    private readonly IAuditService _auditService;

    public StateController(IStateService stateService, IAuditService auditService)
    {
        _stateService = stateService;
        _auditService = auditService;
    }

    /// <summary>
    /// Get available triggers for current state
    /// </summary>
    [HttpGet("triggers/{currentState}")]
    public async Task<IActionResult> GetAvailableTriggers(string currentState)
    {
        try
        {
            var triggers = await _stateService.GetAvailableTriggersAsync(currentState);
            return HandleResult(triggers);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Validate state transition
    /// </summary>
    [HttpPost("validate-transition")]
    public async Task<IActionResult> ValidateTransition([FromBody] ValidateTransitionRequest request)
    {
        try
        {
            var isValid = await _stateService.ValidateTransitionAsync(request.FromState, request.ToState, request.Trigger);
            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isValid,
                Message = isValid ? "Transition is valid" : "Transition is not valid"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get audit trail for entity
    /// </summary>
    [HttpGet("audit/{entityType}/{entityId}")]
    public async Task<IActionResult> GetAuditTrail(string entityType, string entityId)
    {
        try
        {
            var auditTrail = await _auditService.GetAuditTrailAsync(entityType, entityId);
            return HandleResult(auditTrail);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transaction audit history
    /// </summary>
    [HttpGet("audit/transaction/{transactionId}")]
    public async Task<IActionResult> GetTransactionAuditHistory(string transactionId)
    {
        try
        {
            var auditHistory = await _auditService.GetTransactionAuditHistoryAsync(transactionId);
            return HandleResult(auditHistory);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get audits by user
    /// </summary>
    [HttpGet("audit/user/{userId}")]
    public async Task<IActionResult> GetAuditsByUser(string userId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var audits = await _auditService.GetAuditsByUserAsync(userId, fromDate, toDate);
            return HandleResult(audits);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get state transition statistics
    /// </summary>
    [HttpGet("statistics/transitions")]
    public async Task<IActionResult> GetTransitionStatistics([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var statistics = await _stateService.GetTransitionStatisticsAsync(fromDate, toDate);
            return HandleResult(statistics);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    public class ValidateTransitionRequest
    {
        public string FromState { get; set; } = string.Empty;
        public string ToState { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
    }
}