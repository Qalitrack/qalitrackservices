using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeightDataService.Infrastructure.Data;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Health check endpoints
/// </summary>
[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly WeightDataContext _context;

    public HealthController(WeightDataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Basic health check
    /// </summary>
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Weight Data Service",
            Version = "1.0.0"
        });
    }

    /// <summary>
    /// Database health check
    /// </summary>
    [HttpGet("database")]
    public async Task<IActionResult> GetDatabaseHealth()
    {
        try
        {
            await _context.Database.CanConnectAsync();
            return Ok(new
            {
                Status = "Healthy",
                Database = "Connected",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return ServiceUnavailable(new
            {
                Status = "Unhealthy",
                Database = "Disconnected",
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    private IActionResult ServiceUnavailable(object value)
    {
        Response.StatusCode = 503;
        return new JsonResult(value);
    }
}