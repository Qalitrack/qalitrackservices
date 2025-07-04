using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TransactionService.Core.DTOs;

namespace TransactionService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T result, string? errorMessage = null)
    {
        if (result == null)
        {
            return NotFound(new ApiResponse<T>
            {
                Success = false,
                Message = errorMessage ?? "Resource not found",
                Data = default
            });
        }

        return Ok(new ApiResponse<T>
        {
            Success = true,
            Message = "Operation completed successfully",
            Data = result
        });
    }

    protected IActionResult HandleException(Exception ex)
    {
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Message = "An error occurred while processing the request",
            Data = null,
            Error = ex.Message
        });
    }

    protected IActionResult HandleValidationErrors(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        return BadRequest(new ApiResponse<object>
        {
            Success = false,
            Message = "Validation failed",
            Data = null,
            ValidationErrors = errors
        });
    }
}