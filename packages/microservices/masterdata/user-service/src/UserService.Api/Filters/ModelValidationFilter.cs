using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using UserService.Core.DTOs;

namespace UserService.Api.Filters;

public class ModelValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var response = new ApiResponseDto<object>
            {
                Success = false,
                Message = "Validation failed",
                Data = errors
            };

            context.Result = new BadRequestObjectResult(response);
        }

        base.OnActionExecuting(context);
    }
}