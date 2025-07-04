using Microsoft.AspNetCore.Mvc;
using ProductService.Core.DTOs;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the organization ID from gateway-forwarded headers
    /// </summary>
    protected string GetOrganizationId()
    {
        return HttpContext.Request.Headers["X-Organization-Id"].FirstOrDefault() ?? "default-org";
    }

    /// <summary>
    /// Gets the user ID from gateway-forwarded headers
    /// </summary>
    protected string GetUserId()
    {
        return HttpContext.Request.Headers["X-User-ID"].FirstOrDefault() ?? "system";
    }

    /// <summary>
    /// Gets the user name from gateway-forwarded headers
    /// </summary>
    protected string GetUserName()
    {
        return HttpContext.Request.Headers["X-User-Name"].FirstOrDefault() ?? "Unknown";
    }

    /// <summary>
    /// Gets the user email from gateway-forwarded headers
    /// </summary>
    protected string GetUserEmail()
    {
        return HttpContext.Request.Headers["X-User-Email"].FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Gets the user roles from gateway-forwarded headers
    /// </summary>
    protected List<string> GetUserRoles()
    {
        var rolesHeader = HttpContext.Request.Headers["X-User-Roles"].FirstOrDefault();
        if (string.IsNullOrEmpty(rolesHeader))
            return new List<string>();
        
        return rolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(r => r.Trim())
                         .ToList();
    }

    /// <summary>
    /// Checks if the current user has any of the specified roles
    /// </summary>
    protected bool HasAnyRole(params string[] roles)
    {
        var userRoles = GetUserRoles();
        return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if the current user has admin-level access
    /// </summary>
    protected bool IsAdmin()
    {
        return HasAnyRole("Admin", "SuperAdmin");
    }

    /// <summary>
    /// Checks if the current user has operator-level access or higher
    /// </summary>
    protected bool IsOperatorOrHigher()
    {
        return HasAnyRole("Operator", "SiteManager", "Admin", "SuperAdmin");
    }

    /// <summary>
    /// Checks if the request is authorized by the gateway
    /// </summary>
    protected bool IsGatewayAuthorized()
    {
        return HttpContext.Request.Headers["X-Gateway-Authorized"].FirstOrDefault() == "true";
    }

    /// <summary>
    /// Handles API response formatting
    /// </summary>
    protected IActionResult HandleResult<T>(ApiResponseDto<T> result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    /// <summary>
    /// Creates a successful API response
    /// </summary>
    protected ApiResponseDto<T> Success<T>(T data, string message = "")
    {
        return ApiResponseDto<T>.SuccessResponse(data, message);
    }

    /// <summary>
    /// Creates an error API response
    /// </summary>
    protected ApiResponseDto<T> Error<T>(string message, List<string>? errors = null)
    {
        return ApiResponseDto<T>.ErrorResponse(message, errors);
    }

    /// <summary>
    /// Creates a successful API response without data
    /// </summary>
    protected ApiResponseDto Success(string message = "")
    {
        return ApiResponseDto.SuccessResponse(message);
    }

    /// <summary>
    /// Creates an error API response without data
    /// </summary>
    protected ApiResponseDto Error(string message, List<string>? errors = null)
    {
        return ApiResponseDto.ErrorResponse(message, errors);
    }

    /// <summary>
    /// Returns a 403 Forbidden response for insufficient permissions
    /// </summary>
    protected IActionResult Forbidden(string message = "Insufficient permissions")
    {
        return StatusCode(403, Error(message));
    }
}