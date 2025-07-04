using Microsoft.AspNetCore.Mvc;
using CustomerService.Core.DTOs;

namespace CustomerService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected string GetOrganizationId()
    {
        return HttpContext.Request.Headers["X-Organization-Id"].FirstOrDefault() ?? "default-org";
    }

    protected string GetUserId()
    {
        return HttpContext.Request.Headers["X-User-Id"].FirstOrDefault() ?? "system";
    }

    protected IActionResult HandleResult<T>(ApiResponseDto<T> result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return BadRequest(result);
    }

    protected ApiResponseDto<T> Success<T>(T data, string message = "")
    {
        return ApiResponseDto<T>.SuccessResult(data, message);
    }

    protected ApiResponseDto<T> Error<T>(string message, List<string>? errors = null)
    {
        return ApiResponseDto<T>.ErrorResult(message, errors);
    }

    protected ApiResponseDto Success(string message = "")
    {
        return ApiResponseDto.SuccessResult(message);
    }

    protected ApiResponseDto Error(string message, List<string>? errors = null)
    {
        return ApiResponseDto.ErrorResult(message, errors);
    }
}