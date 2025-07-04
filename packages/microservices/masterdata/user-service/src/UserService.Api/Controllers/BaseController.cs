using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs;

namespace UserService.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Ok<T>(T data, string? message = null)
    {
        return base.Ok(new ApiResponseDto<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 200
        });
    }

    protected IActionResult Created<T>(T data, string? message = null)
    {
        return StatusCode(201, new ApiResponseDto<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = 201
        });
    }

    protected IActionResult BadRequest(string message, List<string>? errors = null)
    {
        return base.BadRequest(new ApiResponseDto
        {
            Success = false,
            Message = message,
            Errors = errors,
            StatusCode = 400
        });
    }

    protected IActionResult Unauthorized(string message = "Unauthorized")
    {
        return base.Unauthorized(new ApiResponseDto
        {
            Success = false,
            Message = message,
            StatusCode = 401
        });
    }

    protected IActionResult Forbidden(string message = "Forbidden")
    {
        return StatusCode(403, new ApiResponseDto
        {
            Success = false,
            Message = message,
            StatusCode = 403
        });
    }

    protected IActionResult NotFound(string message = "Not found")
    {
        return base.NotFound(new ApiResponseDto
        {
            Success = false,
            Message = message,
            StatusCode = 404
        });
    }

    protected IActionResult InternalServerError(string message = "Internal server error")
    {
        return StatusCode(500, new ApiResponseDto
        {
            Success = false,
            Message = message,
            StatusCode = 500
        });
    }

    protected string? GetCurrentUserId()
    {
        return User?.FindFirst("user_id")?.Value;
    }

    protected string? GetCurrentUserName()
    {
        return User?.FindFirst("username")?.Value;
    }

    protected List<string> GetCurrentUserRoles()
    {
        return User?.FindAll("role")?.Select(c => c.Value).ToList() ?? new List<string>();
    }

    protected List<string> GetCurrentUserOrganizations()
    {
        return User?.FindAll("organization")?.Select(c => c.Value).ToList() ?? new List<string>();
    }

    protected string? GetCurrentOrganization()
    {
        return User?.FindFirst("current_organization")?.Value;
    }
}