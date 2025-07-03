using Microsoft.AspNetCore.Mvc;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Interfaces;
using DataSyncService.Core.Enums;

namespace DataSyncService.Api.Controllers;

/// <summary>
/// Controller for managing sync conflicts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Conflicts")]
public class ConflictsController : BaseController
{
    private readonly IConflictResolutionService _conflictResolutionService;
    private readonly ILogger<ConflictsController> _logger;

    public ConflictsController(
        IConflictResolutionService conflictResolutionService, 
        ILogger<ConflictsController> logger)
    {
        _conflictResolutionService = conflictResolutionService;
        _logger = logger;
    }

    /// <summary>
    /// Get a specific conflict by ID
    /// </summary>
    /// <param name="id">Conflict ID</param>
    /// <returns>Conflict details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SyncConflictDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncConflictDto>), 404)]
    public async Task<IActionResult> GetConflict(int id)
    {
        var response = await _conflictResolutionService.GetConflictAsync(id);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get conflicts by session ID
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>List of conflicts for the session</returns>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncConflictDto>>), 200)]
    public async Task<IActionResult> GetConflictsBySession(string sessionId)
    {
        var response = await _conflictResolutionService.GetConflictsBySessionAsync(sessionId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get all unresolved conflicts
    /// </summary>
    /// <returns>List of unresolved conflicts</returns>
    [HttpGet("unresolved")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncConflictDto>>), 200)]
    public async Task<IActionResult> GetUnresolvedConflicts()
    {
        var response = await _conflictResolutionService.GetUnresolvedConflictsAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Get conflicts by table name
    /// </summary>
    /// <param name="tableName">Table name</param>
    /// <returns>List of conflicts for the table</returns>
    [HttpGet("table/{tableName}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncConflictDto>>), 200)]
    public async Task<IActionResult> GetConflictsByTable(string tableName)
    {
        var response = await _conflictResolutionService.GetConflictsByTableAsync(tableName);
        return HandleResponse(response);
    }

    /// <summary>
    /// Resolve a specific conflict
    /// </summary>
    /// <param name="id">Conflict ID</param>
    /// <param name="request">Resolution request</param>
    /// <returns>Resolution result</returns>
    [HttpPost("{id}/resolve")]
    [ProducesResponseType(typeof(ApiResponse<ConflictResolutionResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<ConflictResolutionResponse>), 400)]
    [ProducesResponseType(typeof(ApiResponse<ConflictResolutionResponse>), 404)]
    public async Task<IActionResult> ResolveConflict(int id, [FromBody] ResolveConflictRequest request)
    {
        _logger.LogInformation("Resolving conflict {ConflictId} with strategy {Strategy}", 
            id, request.ResolutionStrategy);

        var response = await _conflictResolutionService.ResolveConflictAsync(id, request);
        return HandleResponse(response);
    }

    /// <summary>
    /// Resolve multiple conflicts with the same strategy
    /// </summary>
    /// <param name="request">Bulk resolution request</param>
    /// <returns>Resolution results</returns>
    [HttpPost("resolve-multiple")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConflictResolutionResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ConflictResolutionResponse>>), 400)]
    public async Task<IActionResult> ResolveMultipleConflicts([FromBody] BulkResolveConflictRequest request)
    {
        _logger.LogInformation("Resolving {ConflictCount} conflicts with strategy {Strategy}", 
            request.ConflictIds.Count(), request.ResolutionRequest.ResolutionStrategy);

        var response = await _conflictResolutionService.ResolveMultipleConflictsAsync(
            request.ConflictIds, request.ResolutionRequest);
        return HandleResponse(response);
    }

    /// <summary>
    /// Escalate a conflict for manual review
    /// </summary>
    /// <param name="id">Conflict ID</param>
    /// <param name="reason">Escalation reason</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/escalate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> EscalateConflict(int id, [FromBody] string reason)
    {
        _logger.LogInformation("Escalating conflict {ConflictId} with reason: {Reason}", id, reason);

        var response = await _conflictResolutionService.EscalateConflictAsync(id, reason);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get paginated list of conflicts
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>Paginated conflicts</returns>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SyncConflictDto>>), 200)]
    public async Task<IActionResult> GetConflicts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ConflictStatus? status = null,
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

        var response = await _conflictResolutionService.GetConflictsAsync(pagingRequest, status);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get conflict statistics
    /// </summary>
    /// <returns>Conflict statistics by status</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<ConflictStatus, int>>), 200)]
    public async Task<IActionResult> GetConflictStatistics()
    {
        var response = await _conflictResolutionService.GetConflictStatisticsAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Get available resolution strategies
    /// </summary>
    /// <returns>List of available resolution strategies</returns>
    [HttpGet("strategies")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<string>>), 200)]
    public async Task<IActionResult> GetAvailableResolutionStrategies()
    {
        var response = await _conflictResolutionService.GetAvailableResolutionStrategiesAsync();
        return HandleResponse(response);
    }
}

/// <summary>
/// Request model for bulk conflict resolution
/// </summary>
public class BulkResolveConflictRequest
{
    public IEnumerable<int> ConflictIds { get; set; } = new List<int>();
    public ResolveConflictRequest ResolutionRequest { get; set; } = new();
}