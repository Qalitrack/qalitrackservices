using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Infrastructure.Data;
using RouteEntity = QaliTrack.MasterData.Core.Modules.Route.Entities.Route;
using RouteWaypoint = QaliTrack.MasterData.Core.Modules.Route.Entities.RouteWaypoint;
using RouteSchedule = QaliTrack.MasterData.Core.Modules.Route.Entities.RouteSchedule;
using RouteHistory = QaliTrack.MasterData.Core.Modules.Route.Entities.RouteHistory;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Route Module")]
public class RouteController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public RouteController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get routes with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of routes</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RouteEntity>>>> GetRoutes(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Routes
                .Include(r => r.Waypoints)
                .Include(r => r.Schedules)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteEntity>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteEntity>>.ErrorResponse("Error retrieving routes", ex.Message));
        }
    }

    /// <summary>
    /// Get route by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RouteEntity>>> GetRoute(Guid id)
    {
        try
        {
            var route = await _context.Routes
                .Include(r => r.Waypoints.OrderBy(w => w.SequenceOrder))
                .Include(r => r.Schedules)
                .Include(r => r.History)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null)
            {
                return NotFound(ApiResponse<RouteEntity>.ErrorResponse("Route not found"));
            }

            return Ok(ApiResponse<RouteEntity>.SuccessResponse(route));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteEntity>.ErrorResponse("Error retrieving route", ex.Message));
        }
    }

    /// <summary>
    /// Create new route
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RouteEntity>>> CreateRoute(RouteEntity route)
    {
        try
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoute), 
                new { id = route.Id }, 
                ApiResponse<RouteEntity>.SuccessResponse(route, "Route created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteEntity>.ErrorResponse("Error creating route", ex.Message));
        }
    }

    /// <summary>
    /// Update route
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RouteEntity>>> UpdateRoute(Guid id, RouteEntity route)
    {
        if (id != route.Id)
        {
            return BadRequest(ApiResponse<RouteEntity>.ErrorResponse("ID mismatch"));
        }

        try
        {
            _context.Entry(route).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<RouteEntity>.SuccessResponse(route, "Route updated successfully"));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RouteExists(id))
            {
                return NotFound(ApiResponse<RouteEntity>.ErrorResponse("Route not found"));
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteEntity>.ErrorResponse("Error updating route", ex.Message));
        }
    }

    /// <summary>
    /// Delete route (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRoute(Guid id)
    {
        try
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound(ApiResponse.CreateError("Route not found"));
            }

            route.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Route deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting route", ex.Message));
        }
    }

    private async Task<bool> RouteExists(Guid id)
    {
        return await _context.Routes.AnyAsync(e => e.Id == id);
    }
}