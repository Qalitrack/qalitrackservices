using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Core.DTOs;
using TransactionService.Core.Entities;
using TransactionService.Core.Interfaces;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : BaseController
{
    private readonly ITransactionService _transactionService;
    private readonly IValidator<CreateTransactionRequest> _createValidator;
    private readonly IValidator<UpdateTransactionRequest> _updateValidator;

    public TransactionsController(
        ITransactionService transactionService,
        IValidator<CreateTransactionRequest> createValidator,
        IValidator<UpdateTransactionRequest> updateValidator)
    {
        _transactionService = transactionService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Create a new transaction
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        try
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var transaction = await _transactionService.CreateTransactionAsync(request);
            return Ok(new ApiResponse<TransactionDto>
            {
                Success = true,
                Message = "Transaction created successfully",
                Data = transaction
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transaction by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransaction(string id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionAsync(id);
            return HandleResult(transaction, "Transaction not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get complete transaction with all related data
    /// </summary>
    [HttpGet("{id}/complete")]
    public async Task<IActionResult> GetCompleteTransaction(string id)
    {
        try
        {
            var transaction = await _transactionService.GetCompleteTransactionAsync(id);
            return HandleResult(transaction, "Transaction not found");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get paginated list of transactions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsAsync(pageNumber, pageSize);
            return Ok(new PaginatedResponse<TransactionDto>
            {
                Success = true,
                Message = "Transactions retrieved successfully",
                Data = transactions,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transactions by vehicle ID
    /// </summary>
    [HttpGet("vehicle/{vehicleId}")]
    public async Task<IActionResult> GetTransactionsByVehicle(string vehicleId)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByVehicleAsync(vehicleId);
            return HandleResult(transactions);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transactions by organization ID
    /// </summary>
    [HttpGet("organization/{organizationId}")]
    public async Task<IActionResult> GetTransactionsByOrganization(string organizationId)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByOrganizationAsync(organizationId);
            return HandleResult(transactions);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transactions by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetTransactionsByStatus(TransactionStatus status)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByStatusAsync(status);
            return HandleResult(transactions);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Get transactions by date range
    /// </summary>
    [HttpGet("date-range")]
    public async Task<IActionResult> GetTransactionsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
            return HandleResult(transactions);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Update transaction
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTransaction(string id, [FromBody] UpdateTransactionRequest request)
    {
        try
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validation failed",
                    ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }

            var transaction = await _transactionService.UpdateTransactionAsync(id, request);
            return HandleResult(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiResponse<object>
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
    /// Cancel transaction
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelTransaction(string id, [FromBody] string reason)
    {
        try
        {
            var result = await _transactionService.CancelTransactionAsync(id, reason);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Transaction not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Transaction cancelled successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    /// <summary>
    /// Delete transaction
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(string id)
    {
        try
        {
            var result = await _transactionService.DeleteTransactionAsync(id);
            if (!result)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Transaction not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Transaction deleted successfully"
            });
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}