using Microsoft.AspNetCore.Mvc;
using TransporterService.Core.DTOs;

namespace TransporterService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResult<T>(T data, string message = "Success")
    {
        if (data == null)
        {
            return NotFound(ApiResponseDto<T>.ErrorResponse("Resource not found"));
        }

        return Ok(ApiResponseDto<T>.SuccessResponse(data, message));
    }

    protected IActionResult HandleListResult<T>(IEnumerable<T> data, string message = "Success")
    {
        var list = data.ToList();
        return Ok(ApiResponseDto<List<T>>.SuccessResponse(list, $"{message}. Found {list.Count} item(s)"));
    }

    protected IActionResult HandleCreated<T>(T data, string message = "Created successfully")
    {
        return CreatedAtAction(null, null, ApiResponseDto<T>.SuccessResponse(data, message));
    }

    protected IActionResult HandleError(string message, List<string>? errors = null)
    {
        return BadRequest(ApiResponseDto<object>.ErrorResponse(message, errors));
    }

    protected IActionResult HandleException(Exception ex)
    {
        return StatusCode(500, ApiResponseDto<object>.ErrorResponse("An error occurred", new List<string> { ex.Message }));
    }
}