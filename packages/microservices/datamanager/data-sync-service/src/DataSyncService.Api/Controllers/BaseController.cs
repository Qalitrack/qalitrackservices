using Microsoft.AspNetCore.Mvc;
using DataSyncService.Core.DTOs;

namespace DataSyncService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleResponse<T>(ApiResponse<T> response)
    {
        if (response.Success)
        {
            return Ok(response);
        }

        return response.ErrorCode switch
        {
            "NOT_FOUND" or "SESSION_NOT_FOUND" or "SITE_NOT_FOUND" or "CONFLICT_NOT_FOUND" => NotFound(response),
            "VALIDATION_ERROR" or "INVALID_REQUEST" => BadRequest(response),
            "UNAUTHORIZED" => Unauthorized(response),
            "FORBIDDEN" => Forbid(),
            "CONFLICT" or "ACTIVE_SESSION_EXISTS" => Conflict(response),
            _ => StatusCode(500, response)
        };
    }

    protected IActionResult HandleResponse(ApiResponse<bool> response)
    {
        if (response.Success)
        {
            return Ok(response);
        }

        return response.ErrorCode switch
        {
            "NOT_FOUND" or "SESSION_NOT_FOUND" or "SITE_NOT_FOUND" => NotFound(response),
            "VALIDATION_ERROR" or "INVALID_REQUEST" => BadRequest(response),
            "UNAUTHORIZED" => Unauthorized(response),
            "FORBIDDEN" => Forbid(),
            "CONFLICT" => Conflict(response),
            _ => StatusCode(500, response)
        };
    }
}