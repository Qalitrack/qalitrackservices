using Microsoft.AspNetCore.Mvc;
using DataSyncService.Core.DTOs;

namespace DataSyncService.Api.Controllers;

/// <summary>
/// Controller for health checks and monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Health")]
public class HealthController : BaseController
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Check service health
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckHealth()
    {
        try
        {
            var healthStatus = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "DataSyncService",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Service is healthy",
                Data = healthStatus
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Service is unhealthy",
                ErrorCode = "HEALTH_CHECK_FAILED",
                Data = new
                {
                    Status = "Unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                }
            };

            return StatusCode(503, response);
        }
    }

    /// <summary>
    /// Check readiness (ready to serve requests)
    /// </summary>
    /// <returns>Readiness status</returns>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckReadiness()
    {
        try
        {
            // In a real implementation, you would check:
            // - Database connectivity
            // - Required services availability
            // - Configuration validity

            var readinessStatus = new
            {
                Status = "Ready",
                Timestamp = DateTime.UtcNow,
                Database = "Connected",
                Services = "Available"
            };

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Service is ready",
                Data = readinessStatus
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness check failed");

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Service is not ready",
                ErrorCode = "READINESS_CHECK_FAILED",
                Data = new
                {
                    Status = "NotReady",
                    Timestamp = DateTime.UtcNow,
                    Error = ex.Message
                }
            };

            return StatusCode(503, response);
        }
    }

    /// <summary>
    /// Check liveness (service is running)
    /// </summary>
    /// <returns>Liveness status</returns>
    [HttpGet("live")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> CheckLiveness()
    {
        var livenessStatus = new
        {
            Status = "Alive",
            Timestamp = DateTime.UtcNow,
            UptimeMs = Environment.TickCount64
        };

        var response = new ApiResponse<object>
        {
            Success = true,
            Message = "Service is alive",
            Data = livenessStatus
        };

        return Ok(response);
    }
}