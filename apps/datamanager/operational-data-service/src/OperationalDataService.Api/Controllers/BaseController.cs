using Microsoft.AspNetCore.Mvc;
using OperationalDataService.Core.DTOs;

namespace OperationalDataService.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Ok<T>(T data, string? message = null)
    {
        return base.Ok(new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation completed successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    protected IActionResult BadRequest(string message, object? errors = null)
    {
        return base.BadRequest(new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Message = message,
            Errors = errors,
            Timestamp = DateTime.UtcNow
        });
    }

    protected IActionResult NotFound(string message)
    {
        return base.NotFound(new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    protected IActionResult InternalServerError(string message)
    {
        return StatusCode(500, new ApiResponse<object>
        {
            Success = false,
            Data = null,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    protected string GetOrganizationId()
    {
        // In a real implementation, this would extract organization ID from JWT token or headers
        return HttpContext.Request.Headers["Organization-Id"].FirstOrDefault() ?? "default";
    }

    protected string? GetUserId()
    {
        // In a real implementation, this would extract user ID from JWT token
        return HttpContext.Request.Headers["User-Id"].FirstOrDefault();
    }
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; }
}