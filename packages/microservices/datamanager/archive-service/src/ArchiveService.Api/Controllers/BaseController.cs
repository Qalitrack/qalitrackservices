using Microsoft.AspNetCore.Mvc;

namespace ArchiveService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(T result)
        {
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        protected IActionResult HandleException(Exception ex, string message = "An error occurred")
        {
            // In a real application, you would log the exception here
            return StatusCode(500, new { error = message, details = ex.Message });
        }

        protected string GetUserId()
        {
            // In a real application, extract from JWT token or authentication context
            return "system-user";
        }

        protected IActionResult BadRequestWithMessage(string message)
        {
            return BadRequest(new { error = message });
        }

        protected IActionResult ConflictWithMessage(string message)
        {
            return Conflict(new { error = message });
        }
    }
}