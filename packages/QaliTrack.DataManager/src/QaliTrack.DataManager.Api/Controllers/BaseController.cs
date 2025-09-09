using Microsoft.AspNetCore.Mvc;
using QaliTrack.DataManager.Core.Common;

namespace QaliTrack.DataManager.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> SuccessResponse<T>(T data, string message = "Operation successful")
    {
        return Ok(ApiResponse<T>.SuccessResponse(data, message));
    }

    protected ActionResult<ApiResponse<T>> ErrorResponse<T>(string message, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(ApiResponse<T>.ErrorResponse(message, error));
        }
        return BadRequest(ApiResponse<T>.ErrorResponse(message));
    }

    protected ActionResult<ApiResponse<object>> ErrorResponse(string message, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest(ApiResponse.CreateError(message, error));
        }
        return BadRequest(ApiResponse.CreateError(message));
    }

    protected ActionResult<ApiResponse<IEnumerable<T>>> PaginatedResponse<T>(
        IEnumerable<T> items, 
        int page, 
        int pageSize, 
        int totalCount, 
        string message = "Operation successful")
    {
        var response = new ApiResponse<IEnumerable<T>>
        {
            Success = true,
            Message = message,
            Data = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return Ok(response);
    }
}