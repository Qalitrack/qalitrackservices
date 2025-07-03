using Microsoft.AspNetCore.Mvc;
using SupplierService.Core.DTOs;

namespace SupplierService.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T? data, string? message = null)
    {
        if (data == null)
        {
            return NotFound(ApiResponseDto<T>.ErrorResponse("Resource not found"));
        }

        return Ok(ApiResponseDto<T>.SuccessResponse(data, message));
    }

    protected IActionResult HandleException(Exception ex)
    {
        var errors = new List<string> { ex.Message };
        
        if (ex.InnerException != null)
        {
            errors.Add(ex.InnerException.Message);
        }

        return ex switch
        {
            KeyNotFoundException => NotFound(ApiResponseDto<object>.ErrorResponse("Resource not found", errors)),
            InvalidOperationException => BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid operation", errors)),
            ArgumentException => BadRequest(ApiResponseDto<object>.ErrorResponse("Invalid arguments", errors)),
            _ => StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred", errors))
        };
    }
}