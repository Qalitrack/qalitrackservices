using Microsoft.AspNetCore.Mvc;
using DataSyncService.Core.DTOs;
using DataSyncService.Core.Interfaces;
using DataSyncService.Core.Enums;

namespace DataSyncService.Api.Controllers;

/// <summary>
/// Controller for managing sync sites
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Sites")]
public class SitesController : BaseController
{
    private readonly ISyncSiteService _syncSiteService;
    private readonly ILogger<SitesController> _logger;

    public SitesController(ISyncSiteService syncSiteService, ILogger<SitesController> logger)
    {
        _syncSiteService = syncSiteService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new sync site
    /// </summary>
    /// <param name="request">Site creation request</param>
    /// <returns>Created site</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 400)]
    public async Task<IActionResult> CreateSite([FromBody] CreateSyncSiteRequest request)
    {
        _logger.LogInformation("Creating new sync site {SiteId}", request.SiteId);

        var response = await _syncSiteService.CreateSiteAsync(request);
        
        if (response.Success)
        {
            return CreatedAtAction(nameof(GetSite), new { id = response.Data!.Id }, response);
        }

        return HandleResponse(response);
    }

    /// <summary>
    /// Update an existing sync site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <param name="request">Site update request</param>
    /// <returns>Updated site</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 400)]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 404)]
    public async Task<IActionResult> UpdateSite(int id, [FromBody] UpdateSyncSiteRequest request)
    {
        _logger.LogInformation("Updating sync site {SiteId}", id);

        var response = await _syncSiteService.UpdateSiteAsync(id, request);
        return HandleResponse(response);
    }

    /// <summary>
    /// Delete a sync site
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteSite(int id)
    {
        _logger.LogInformation("Deleting sync site {SiteId}", id);

        var response = await _syncSiteService.DeleteSiteAsync(id);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get a sync site by ID
    /// </summary>
    /// <param name="id">Site ID</param>
    /// <returns>Site details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 404)]
    public async Task<IActionResult> GetSite(int id)
    {
        var response = await _syncSiteService.GetSiteAsync(id);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get a sync site by site ID
    /// </summary>
    /// <param name="siteId">Site identifier</param>
    /// <returns>Site details</returns>
    [HttpGet("by-site-id/{siteId}")]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<SyncSiteDto>), 404)]
    public async Task<IActionResult> GetSiteBySiteId(string siteId)
    {
        var response = await _syncSiteService.GetSiteBySiteIdAsync(siteId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get all sync sites
    /// </summary>
    /// <returns>List of all sites</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncSiteDto>>), 200)]
    public async Task<IActionResult> GetAllSites()
    {
        var response = await _syncSiteService.GetAllSitesAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Get all active sync sites
    /// </summary>
    /// <returns>List of active sites</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncSiteDto>>), 200)]
    public async Task<IActionResult> GetActiveSites()
    {
        var response = await _syncSiteService.GetActiveSitesAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Get sites by status
    /// </summary>
    /// <param name="status">Site status filter</param>
    /// <returns>List of sites with specified status</returns>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SyncSiteDto>>), 200)]
    public async Task<IActionResult> GetSitesByStatus(SiteStatus status)
    {
        var response = await _syncSiteService.GetSitesByStatusAsync(status);
        return HandleResponse(response);
    }

    /// <summary>
    /// Check health of a specific site
    /// </summary>
    /// <param name="siteId">Site identifier</param>
    /// <returns>Health check result</returns>
    [HttpGet("{siteId}/health")]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<HealthCheckDto>), 404)]
    public async Task<IActionResult> CheckSiteHealth(string siteId)
    {
        _logger.LogInformation("Checking health for site {SiteId}", siteId);

        var response = await _syncSiteService.CheckSiteHealthAsync(siteId);
        return HandleResponse(response);
    }

    /// <summary>
    /// Check health of all sites
    /// </summary>
    /// <returns>Health check results for all sites</returns>
    [HttpGet("health/all")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<HealthCheckDto>>), 200)]
    public async Task<IActionResult> CheckAllSitesHealth()
    {
        _logger.LogInformation("Checking health for all sites");

        var response = await _syncSiteService.CheckAllSitesHealthAsync();
        return HandleResponse(response);
    }

    /// <summary>
    /// Update site status
    /// </summary>
    /// <param name="siteId">Site identifier</param>
    /// <param name="status">New status</param>
    /// <returns>Success status</returns>
    [HttpPut("{siteId}/status")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> UpdateSiteStatus(string siteId, [FromBody] SiteStatus status)
    {
        _logger.LogInformation("Updating status for site {SiteId} to {Status}", siteId, status);

        var response = await _syncSiteService.UpdateSiteStatusAsync(siteId, status);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get paginated list of sites
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="status">Optional status filter</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <returns>Paginated sites</returns>
    [HttpGet("paged")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SyncSiteDto>>), 200)]
    public async Task<IActionResult> GetSites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SiteStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var pagingRequest = new PagingRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var response = await _syncSiteService.GetSitesAsync(pagingRequest, status);
        return HandleResponse(response);
    }

    /// <summary>
    /// Get site statistics
    /// </summary>
    /// <returns>Site statistics by status</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<SiteStatus, int>>), 200)]
    public async Task<IActionResult> GetSiteStatistics()
    {
        var response = await _syncSiteService.GetSiteStatisticsAsync();
        return HandleResponse(response);
    }
}