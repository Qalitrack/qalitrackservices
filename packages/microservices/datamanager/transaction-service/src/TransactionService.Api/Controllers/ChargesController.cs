using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChargesController : BaseController
{
    private readonly IChargeService _chargeService;
    private readonly IValidator<CreateChargeRequest> _validator;

    public ChargesController(
        IChargeService chargeService,
        IValidator<CreateChargeRequest> validator)
    {
        _chargeService = chargeService;
        _validator = validator;
    }

    /// <summary>
    /// Get charges for a transaction
    /// </summary>
    [HttpGet("transaction/{transactionId}")]
    public async Task<IActionResult> GetCharges(string transactionId)
    {
        try
        {
            var charges = await _chargeService.GetChargesAsync(transactionId);
            return HandleResult(charges);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Create a new charge
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCharge([FromBody] CreateChargeRequest request)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var charge = await _chargeService.CreateChargeAsync(request);
            return Ok(new ApiResponse<TransactionChargeDto>
            {
                Success = true,
                Message = "Charge created successfully",
                Data = charge
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update a charge
    /// </summary>
    [HttpPut("{chargeId}")]
    public async Task<IActionResult> UpdateCharge(string chargeId, [FromBody] CreateChargeRequest request)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var charge = await _chargeService.UpdateChargeAsync(chargeId, request);
            return HandleResult(charge);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete a charge
    /// </summary>
    [HttpDelete("{chargeId}")]
    public async Task<IActionResult> DeleteCharge(string chargeId)
    {
        try
        {
            var result = await _chargeService.DeleteChargeAsync(chargeId);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Charge not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Charge deleted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Approve a charge
    /// </summary>
    [HttpPost("{chargeId}/approve")]
    public async Task<IActionResult> ApproveCharge(string chargeId, [FromBody] ApproveChargeRequest request)
    {
        try
        {
            var result = await _chargeService.ApproveChargeAsync(chargeId, request.ApprovedBy);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Charge not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Charge approved successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Process payment for a charge
    /// </summary>
    [HttpPost("{chargeId}/pay")]
    public async Task<IActionResult> ProcessPayment(string chargeId, [FromBody] ProcessPaymentRequest request)
    {
        try
        {
            var result = await _chargeService.ProcessPaymentAsync(chargeId, request.PaymentReference);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Charge not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Payment processed successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get total amount for a transaction
    /// </summary>
    [HttpGet("transaction/{transactionId}/total")]
    public async Task<IActionResult> GetTotalAmount(string transactionId)
    {
        try
        {
            var total = await _chargeService.GetTotalAmountAsync(transactionId);
            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Message = "Total amount retrieved successfully",
                Data = total
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get unpaid amount for a transaction
    /// </summary>
    [HttpGet("transaction/{transactionId}/unpaid")]
    public async Task<IActionResult> GetUnpaidAmount(string transactionId)
    {
        try
        {
            var unpaid = await _chargeService.GetUnpaidAmountAsync(transactionId);
            return Ok(new ApiResponse<decimal>
            {
                Success = true,
                Message = "Unpaid amount retrieved successfully",
                Data = unpaid
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get all unapproved charges
    /// </summary>
    [HttpGet("unapproved")]
    public async Task<IActionResult> GetUnapprovedCharges()
    {
        try
        {
            var charges = await _chargeService.GetUnapprovedChargesAsync();
            return HandleResult(charges);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Recalculate charges for a transaction
    /// </summary>
    [HttpPost("transaction/{transactionId}/recalculate")]
    public async Task<IActionResult> RecalculateCharges(string transactionId)
    {
        try
        {
            await _chargeService.CalculateChargesAsync(transactionId);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Charges recalculated successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}

public class ApproveChargeRequest
{
    public string ApprovedBy { get; set; } = string.Empty;
}

public class ProcessPaymentRequest
{
    public string PaymentReference { get; set; } = string.Empty;
}