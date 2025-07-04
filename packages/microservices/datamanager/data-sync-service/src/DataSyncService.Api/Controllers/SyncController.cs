using Microsoft.AspNetCore.Mvc;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Interfaces;
using DataSyncService.Core.Enums;

namespace DataSyncService.Api.Controllers;

/// <summary>
/// Controller for managing data synchronization operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Synchronization")]
public class SyncController : BaseController
{
    private readonly ISyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ISyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Start a new synchronization session
    /// </summary>
    /// <param name="request">Sync session creation request</param>
    /// <returns>Created sync session</returns>
    [HttpPost("start")]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 409)]
    public async Task<IActionResult> StartSync([FromBody] CreateSyncSessionRequest request)
    {
        _logger.LogInformation("Starting sync session from {SourceSiteId} to {TargetSiteId}", 
            request.SourceSiteId, request.TargetSiteId);

        var response = await _syncService.StartSyncAsync(request);
        return HandleResponse(response);
    }

    /// <summary>
    /// Stop an active synchronization session
    /// </summary>
    /// <param name="sessionId">Session ID to stop</param>
    /// <returns>Updated sync session</returns>
    [HttpPost("{sessionId}/stop")]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 404)]
    public async Task<IActionResult> StopSync(string sessionId)
    {
        _logger.LogInformation("Stopping sync session {SessionId}", sessionId);

        var response = await _syncService.StopSyncAsync(sessionId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get the status of a synchronization session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Sync session status</returns>
    [HttpGet("{sessionId}/status")]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 404)]
    public async Task<IActionResult> GetSyncStatus(string sessionId)
    {
        var response = await _syncService.GetSyncStatusAsync(sessionId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get all active synchronization sessions
    /// </summary>
    /// <returns>List of active sync sessions</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncSessionDto>>), 200)]
    public async Task<IActionResult> GetActiveSessions()
    {
        var response = await _syncService.GetActiveSyncSessionsAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Get synchronization history
    /// </summary>
    /// <param name="siteId">Optional site ID filter</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Sync history</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncSessionDto>>), 200)]
    public async Task<IActionResult> GetSyncHistory(
        [FromQuery] string? siteId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _syncService.GetSyncHistoryAsync(siteId, pageNumber, pageSize);
        return HandleResponse(response);
    }

    /// <summary>
    /// Synchronize a specific table
    /// </summary>
    /// <param name="request">Table sync request</param>
    /// <returns>Sync session for the table sync</returns>
    [HttpPost("table")]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSessionDto>), 400)]
    public async Task<IActionResult> SyncTable([FromBody] SyncDataRequest request)
    {
        _logger.LogInformation("Starting table sync for {TableName}", request.TableName);

        var response = await _syncService.SyncTableAsync(request);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get synchronization status summary
    /// </summary>
    /// <returns>Sync status summary</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<SyncStatusSummary>), 200)]
    public async Task<IActionResult> GetSyncStatusSummary()
    {
        var response = await _syncService.GetSyncStatusSummaryAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Cancel a synchronization session
    /// </summary>
    /// <param name="sessionId">Session ID to cancel</param>
    /// <returns>Success status</returns>
    [HttpPost("{sessionId}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> CancelSync(string sessionId)
    {
        _logger.LogInformation("Cancelling sync session {SessionId}", sessionId);

        var response = await _syncService.CancelSyncAsync(sessionId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Retry a failed synchronization session
    /// </summary>
    /// <param name="sessionId">Session ID to retry</param>
    /// <returns>Success status</returns>
    [HttpPost("{sessionId}/retry")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> RetrySync(string sessionId)
    {
        _logger.LogInformation("Retrying sync session {SessionId}", sessionId);

        var response = await _syncService.RetrySyncAsync(sessionId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get paginated list of sync sessions
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>Paginated sync sessions</returns>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SyncSessionDto>>), 200)]
    public async Task<IActionResult> GetSyncSessions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SyncStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true)
    {
        var pagingRequest = new PagingRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var response = await _syncService.GetSyncSessionsAsync(pagingRequest, status);
        return HandleResponse(response);
    }
}