using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/transactions/{transactionId}/[controller]")]
public class WorkflowController : BaseController
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    /// <summary>
    /// Get workflow steps for a transaction
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkflowSteps(string transactionId)
    {
        try
        {
            var steps = await _workflowService.GetWorkflowStepsAsync(transactionId);
            return HandleResult(steps);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get current workflow step
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentStep(string transactionId)
    {
        try
        {
            var step = await _workflowService.GetCurrentStepAsync(transactionId);
            return HandleResult(step, "No current step found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Advance workflow to next step
    /// </summary>
    [HttpPost("advance")]
    public async Task<IActionResult> AdvanceWorkflow(
        string transactionId,
        [FromBody] AdvanceWorkflowRequest? request = null)
    {
        try
        {
            var result = await _workflowService.AdvanceWorkflowAsync(
                transactionId,
                request?.ProcessedBy,
                request?.Notes);

            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Cannot advance workflow. Current step may not be ready for completion."
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Workflow advanced successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update specific workflow step
    /// </summary>
    [HttpPut("step/{step}")]
    public async Task<IActionResult> UpdateStep(
        string transactionId,
        WorkflowStep step,
        [FromBody] UpdateStepRequest request)
    {
        try
        {
            var result = await _workflowService.UpdateStepAsync(
                transactionId,
                step,
                request.Status,
                request.ProcessedBy,
                request.Notes);

            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Workflow step not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Workflow step updated successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Skip a workflow step
    /// </summary>
    [HttpPost("step/{step}/skip")]
    public async Task<IActionResult> SkipStep(
        string transactionId,
        WorkflowStep step,
        [FromBody] SkipStepRequest request)
    {
        try
        {
            var result = await _workflowService.SkipStepAsync(transactionId, step, request.Reason);
            
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Workflow step not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Workflow step skipped successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Reset workflow to beginning
    /// </summary>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetWorkflow(
        string transactionId,
        [FromBody] ResetWorkflowRequest? request = null)
    {
        try
        {
            var result = await _workflowService.ResetWorkflowAsync(transactionId, request?.Reason);
            
            if (!result)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Failed to reset workflow"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Workflow reset successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

/// <summary>
/// Get all pending workflow steps across all transactions
/// </summary>
[Route("api/workflow/pending")]
[ApiController]
public class PendingWorkflowController : BaseController
{
    private readonly IWorkflowService _workflowService;

    public PendingWorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPendingSteps()
    {
        try
        {
            var steps = await _workflowService.GetPendingStepsAsync();
            return HandleResult(steps);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

public class AdvanceWorkflowRequest
{
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStepRequest
{
    public StepStatus Status { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }
}

public class SkipStepRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ResetWorkflowRequest
{
    public string? Reason { get; set; }
}