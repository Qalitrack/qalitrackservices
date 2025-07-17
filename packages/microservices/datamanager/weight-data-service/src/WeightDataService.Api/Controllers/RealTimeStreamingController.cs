using Microsoft.AspNetCore.Mvc;
using WeightDataService.Core.DTOs;
using WeightDataService.Core.Interfaces;

namespace WeightDataService.Api.Controllers;

/// <summary>
/// Real-time weight data streaming management
/// </summary>
[ApiController]
[Route("api/streaming")]
public class RealTimeStreamingController : BaseController
{
    private readonly IRealTimeStreamingService _streamingService;

    public RealTimeStreamingController(IRealTimeStreamingService streamingService)
    {
        _streamingService = streamingService;
    }

    /// <summary>
    /// Start a new real-time streaming session
    /// </summary>
    [HttpPost("sessions")]
    public async Task<IActionResult> StartStreamingSession([FromBody] CreateRealTimeSessionDto createDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var session = await _streamingService.StartStreamingSessionAsync(createDto, organizationId, userId);
            
            return CreatedAtAction(nameof(GetStreamingSession), new { sessionId = session.SessionId }, 
                Success(session, "Streaming session started successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<RealTimeSessionDto>(ex.Message));
        }
    }

    /// <summary>
    /// Get streaming session details
    /// </summary>
    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetStreamingSession(string sessionId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var session = await _streamingService.GetStreamingSessionAsync(sessionId, organizationId);
            
            if (session == null)
            {
                return NotFound(Error<RealTimeSessionDto>("Streaming session not found"));
            }

            return HandleResult(Success(session, "Streaming session retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<RealTimeSessionDto>(ex.Message));
        }
    }

    /// <summary>
    /// Update streaming session with new weight data
    /// </summary>
    [HttpPut("sessions/{sessionId}")]
    public async Task<IActionResult> UpdateStreamingSession(string sessionId, [FromBody] UpdateRealTimeSessionDto updateDto)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var updated = await _streamingService.UpdateStreamingSessionAsync(sessionId, updateDto, organizationId);
            
            if (!updated)
            {
                return NotFound(Error<bool>("Streaming session not found"));
            }

            return HandleResult(Success(true, "Streaming session updated successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// End streaming session
    /// </summary>
    [HttpPost("sessions/{sessionId}/end")]
    public async Task<IActionResult> EndStreamingSession(string sessionId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var userId = GetUserId();
            var ended = await _streamingService.EndStreamingSessionAsync(sessionId, organizationId, userId);
            
            if (!ended)
            {
                return NotFound(Error<bool>("Streaming session not found"));
            }

            return HandleResult(Success(true, "Streaming session ended successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Get active streaming sessions
    /// </summary>
    [HttpGet("sessions/active")]
    public async Task<IActionResult> GetActiveSessions()
    {
        try
        {
            var organizationId = GetOrganizationId();
            var sessions = await _streamingService.GetActiveSessionsAsync(organizationId);
            
            return HandleResult(Success(sessions, "Active sessions retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<RealTimeSessionDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Get streaming weight data
    /// </summary>
    [HttpGet("sessions/{sessionId}/data")]
    public async Task<IActionResult> GetStreamingData(string sessionId, [FromQuery] DateTime? fromTime = null)
    {
        try
        {
            var data = await _streamingService.GetStreamingDataAsync(sessionId, fromTime);
            
            return HandleResult(Success(data, "Streaming data retrieved successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<List<StreamingWeightDataDto>>(ex.Message));
        }
    }

    /// <summary>
    /// Pause streaming session
    /// </summary>
    [HttpPost("sessions/{sessionId}/pause")]
    public async Task<IActionResult> PauseStreaming(string sessionId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var paused = await _streamingService.PauseStreamingAsync(sessionId, organizationId);
            
            if (!paused)
            {
                return NotFound(Error<bool>("Streaming session not found"));
            }

            return HandleResult(Success(true, "Streaming session paused successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }

    /// <summary>
    /// Resume streaming session
    /// </summary>
    [HttpPost("sessions/{sessionId}/resume")]
    public async Task<IActionResult> ResumeStreaming(string sessionId)
    {
        try
        {
            var organizationId = GetOrganizationId();
            var resumed = await _streamingService.ResumeStreamingAsync(sessionId, organizationId);
            
            if (!resumed)
            {
                return NotFound(Error<bool>("Streaming session not found"));
            }

            return HandleResult(Success(true, "Streaming session resumed successfully"));
        }
        catch (Exception ex)
        {
            return HandleResult(Error<bool>(ex.Message));
        }
    }
}