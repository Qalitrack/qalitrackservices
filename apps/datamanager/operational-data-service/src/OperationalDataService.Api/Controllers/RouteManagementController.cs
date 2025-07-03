using Microsoft.AspNetCore.Mvc;
using OperationalDataService.Core.DTOs;
using OperationalDataService.Core.Interfaces;

namespace OperationalDataService.Api.Controllers;

/// <summary>
/// Controller for managing route optimization and management operations
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/routes")]
[ApiVersion("1.0")]
public class RouteManagementController : BaseController
{
    private readonly IRouteOptimizationService _routeService;
    private readonly ILogger<RouteManagementController> _logger;

    public RouteManagementController(
        IRouteOptimizationService routeService,
        ILogger<RouteManagementController> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    /// <summary>
    /// Optimize route based on given criteria
    /// </summary>
    /// <param name="request">Route optimization request</param>
    /// <returns>Optimized route</returns>
    [HttpPost("optimize")]
    public async Task<IActionResult> OptimizeRoute([FromBody] RouteOptimizationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Origin) || string.IsNullOrEmpty(request.Destination))
            {
                return BadRequest("Origin and destination are required");
            }

            request.OrganizationId ??= GetOrganizationId();
            var optimizedRoute = await _routeService.OptimizeRouteAsync(request);
            
            return Ok(optimizedRoute, "Route optimized successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Route optimization failed for {Origin} to {Destination}", 
                request.Origin, request.Destination);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing route from {Origin} to {Destination}", 
                request.Origin, request.Destination);
            return InternalServerError("Failed to optimize route");
        }
    }

    /// <summary>
    /// Get route recommendations
    /// </summary>
    /// <param name="origin">Origin location</param>
    /// <param name="destination">Destination location</param>
    /// <returns>List of route recommendations</returns>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRouteRecommendations(
        [FromQuery] string origin,
        [FromQuery] string destination)
    {
        try
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                return BadRequest("Origin and destination are required");
            }

            var organizationId = GetOrganizationId();
            var recommendations = await _routeService.GetRouteRecommendationsAsync(origin, destination, organizationId);
            
            return Ok(recommendations, "Route recommendations retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route recommendations from {Origin} to {Destination}", 
                origin, destination);
            return InternalServerError("Failed to get route recommendations");
        }
    }

    /// <summary>
    /// Get route by ID
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Route details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoute(string id)
    {
        try
        {
            var route = await _routeService.GetRouteByIdAsync(id);
            
            if (route == null)
            {
                return NotFound($"Route with ID {id} not found");
            }

            return Ok(route, "Route retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route {RouteId}", id);
            return InternalServerError("Failed to retrieve route");
        }
    }

    /// <summary>
    /// Analyze route performance
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <param name="startDate">Analysis start date</param>
    /// <param name="endDate">Analysis end date</param>
    /// <returns>Route performance metrics</returns>
    [HttpGet("{id}/performance")]
    public async Task<IActionResult> AnalyzeRoutePerformance(
        string id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var period = new TimeRange
            {
                StartDate = startDate ?? DateTime.UtcNow.AddDays(-30),
                EndDate = endDate ?? DateTime.UtcNow
            };

            var metrics = await _routeService.AnalyzeRoutePerformanceAsync(id, period);
            return Ok(metrics, "Route performance analysis completed");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Route performance analysis failed for {RouteId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing route performance for {RouteId}", id);
            return InternalServerError("Failed to analyze route performance");
        }
    }

    /// <summary>
    /// Update traffic conditions for a route
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <param name="trafficData">Traffic data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}/traffic")]
    public async Task<IActionResult> UpdateTrafficConditions(string id, [FromBody] TrafficData trafficData)
    {
        try
        {
            await _routeService.UpdateTrafficPatternsAsync(id, trafficData);
            return Ok("Traffic conditions updated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Traffic update failed for route {RouteId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating traffic conditions for route {RouteId}", id);
            return InternalServerError("Failed to update traffic conditions");
        }
    }

    /// <summary>
    /// Get alternative routes
    /// </summary>
    /// <param name="request">Route optimization request</param>
    /// <param name="maxAlternatives">Maximum number of alternatives to return</param>
    /// <returns>List of alternative routes</returns>
    [HttpPost("alternatives")]
    public async Task<IActionResult> GetAlternativeRoutes(
        [FromBody] RouteOptimizationRequest request,
        [FromQuery] int maxAlternatives = 3)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Origin) || string.IsNullOrEmpty(request.Destination))
            {
                return BadRequest("Origin and destination are required");
            }

            request.OrganizationId ??= GetOrganizationId();
            var alternatives = await _routeService.GetAlternativeRoutesAsync(request, maxAlternatives);
            
            return Ok(alternatives, "Alternative routes retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting alternative routes from {Origin} to {Destination}", 
                request.Origin, request.Destination);
            return InternalServerError("Failed to get alternative routes");
        }
    }

    /// <summary>
    /// Get active route issues
    /// </summary>
    /// <param name="routeId">Route ID (optional, gets issues for all routes if not provided)</param>
    /// <returns>List of active route issues</returns>
    [HttpGet("issues")]
    public async Task<IActionResult> GetActiveRouteIssues([FromQuery] string? routeId = null)
    {
        try
        {
            var issues = await _routeService.GetActiveRouteIssuesAsync(routeId);
            return Ok(issues, "Active route issues retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active route issues");
            return InternalServerError("Failed to retrieve active route issues");
        }
    }

    /// <summary>
    /// Get current traffic data for a route
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Current traffic data</returns>
    [HttpGet("{id}/traffic")]
    public async Task<IActionResult> GetCurrentTrafficData(string id)
    {
        try
        {
            var trafficData = await _routeService.GetCurrentTrafficDataAsync(id);
            return Ok(trafficData, "Current traffic data retrieved successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Traffic data not found for route {RouteId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving traffic data for route {RouteId}", id);
            return InternalServerError("Failed to retrieve traffic data");
        }
    }

    /// <summary>
    /// Get favorite routes
    /// </summary>
    /// <param name="origin">Origin location</param>
    /// <param name="destination">Destination location</param>
    /// <returns>List of favorite routes</returns>
    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavoriteRoutes(
        [FromQuery] string origin,
        [FromQuery] string destination)
    {
        try
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                return BadRequest("Origin and destination are required");
            }

            var organizationId = GetOrganizationId();
            var favorites = await _routeService.GetFavoriteRoutesAsync(origin, destination, organizationId);
            
            return Ok(favorites, "Favorite routes retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving favorite routes from {Origin} to {Destination}", 
                origin, destination);
            return InternalServerError("Failed to retrieve favorite routes");
        }
    }

    /// <summary>
    /// Save a route
    /// </summary>
    /// <param name="route">Route to save</param>
    /// <returns>Saved route</returns>
    [HttpPost]
    public async Task<IActionResult> SaveRoute([FromBody] OptimizedRoute route)
    {
        try
        {
            var savedRoute = await _routeService.SaveRouteAsync(route);
            return Ok(savedRoute, "Route saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving route {RouteId}", route.RouteId);
            return InternalServerError("Failed to save route");
        }
    }

    /// <summary>
    /// Delete a route
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRoute(string id)
    {
        try
        {
            var success = await _routeService.DeleteRouteAsync(id);
            
            if (!success)
            {
                return NotFound($"Route with ID {id} not found");
            }

            return Ok("Route deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route {RouteId}", id);
            return InternalServerError("Failed to delete route");
        }
    }

    /// <summary>
    /// Get route history
    /// </summary>
    /// <param name="origin">Origin location</param>
    /// <param name="destination">Destination location</param>
    /// <param name="fromDate">Start date for history</param>
    /// <param name="toDate">End date for history</param>
    /// <returns>List of historical routes</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetRouteHistory(
        [FromQuery] string origin,
        [FromQuery] string destination,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
            {
                return BadRequest("Origin and destination are required");
            }

            var startDate = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var endDate = toDate ?? DateTime.UtcNow;

            var history = await _routeService.GetRouteHistoryAsync(origin, destination, startDate, endDate);
            return Ok(history, "Route history retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route history from {Origin} to {Destination}", 
                origin, destination);
            return InternalServerError("Failed to retrieve route history");
        }
    }

    /// <summary>
    /// Calculate route quality
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <returns>Route quality metrics</returns>
    [HttpGet("{id}/quality")]
    public async Task<IActionResult> CalculateRouteQuality(string id)
    {
        try
        {
            var quality = await _routeService.CalculateRouteQualityAsync(id);
            return Ok(quality, "Route quality calculated successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Route quality calculation failed for {RouteId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating route quality for {RouteId}", id);
            return InternalServerError("Failed to calculate route quality");
        }
    }

    /// <summary>
    /// Report a route issue
    /// </summary>
    /// <param name="id">Route ID</param>
    /// <param name="issue">Route issue details</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/issues")]
    public async Task<IActionResult> ReportRouteIssue(string id, [FromBody] RouteIssue issue)
    {
        try
        {
            await _routeService.ReportRouteIssueAsync(id, issue);
            return Ok("Route issue reported successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Route issue reporting failed for {RouteId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting route issue for {RouteId}", id);
            return InternalServerError("Failed to report route issue");
        }
    }
}