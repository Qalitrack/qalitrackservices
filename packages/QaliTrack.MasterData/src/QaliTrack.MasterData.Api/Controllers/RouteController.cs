using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Route.DTOs;
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
    public async Task<ActionResult<ApiResponse<PagedResult<RouteSummaryDto>>>> GetRoutes(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Routes
                .Select(r => new RouteSummaryDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    StartLocation = r.StartLocation,
                    EndLocation = r.EndLocation,
                    Distance = r.Distance,
                    EstimatedDurationMinutes = r.EstimatedDurationMinutes,
                    Status = r.Status,
                    RouteType = r.RouteType,
                    IsActive = r.IsActive,
                    WaypointCount = r.Waypoints.Count,
                    ScheduleCount = r.Schedules.Count,
                    CreatedAt = r.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteSummaryDto>>.ErrorResponse("Error retrieving routes", ex.Message));
        }
    }

    /// <summary>
    /// Get route by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> GetRoute(Guid id)
    {
        try
        {
            var route = await _context.Routes
                .Where(r => r.Id == id)
                .Select(r => new RouteDetailDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    StartLocation = r.StartLocation,
                    EndLocation = r.EndLocation,
                    Distance = r.Distance,
                    EstimatedDurationMinutes = r.EstimatedDurationMinutes,
                    Status = r.Status,
                    RouteType = r.RouteType,
                    RoadType = r.RoadType,
                    Coordinates = r.Coordinates,
                    TrafficConditions = r.TrafficConditions,
                    WeatherRestrictions = r.WeatherRestrictions,
                    VehicleRestrictions = r.VehicleRestrictions,
                    TollFee = r.TollFee,
                    FuelStations = r.FuelStations,
                    RestAreas = r.RestAreas,
                    IsActive = r.IsActive,
                    OrganizationId = r.OrganizationId,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasWaypoints = r.Waypoints.Any(),
                    HasSchedules = r.Schedules.Any(),
                    HasHistory = r.History.Any()
                })
                .FirstOrDefaultAsync();

            if (route == null)
            {
                return NotFound(ApiResponse<RouteDetailDto>.ErrorResponse("Route not found"));
            }

            return Ok(ApiResponse<RouteDetailDto>.SuccessResponse(route));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteDetailDto>.ErrorResponse("Error retrieving route", ex.Message));
        }
    }

    /// <summary>
    /// Create new route
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> CreateRoute(CreateRouteDto dto)
    {
        try
        {
            var route = new RouteEntity
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description ?? string.Empty,
                StartLocation = dto.StartLocation,
                EndLocation = dto.EndLocation,
                Distance = dto.Distance,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                Status = dto.Status,
                RouteType = dto.RouteType,
                RoadType = dto.RoadType,
                Coordinates = dto.Coordinates,
                TrafficConditions = dto.TrafficConditions,
                WeatherRestrictions = dto.WeatherRestrictions,
                VehicleRestrictions = dto.VehicleRestrictions,
                TollFee = dto.TollFee,
                FuelStations = dto.FuelStations,
                RestAreas = dto.RestAreas,
                OrganizationId = dto.OrganizationId,
                Notes = dto.Notes
            };

            _context.Routes.Add(route);
            await _context.SaveChangesAsync();

            var responseDto = new RouteDetailDto
            {
                Id = route.Id,
                Name = route.Name,
                Code = route.Code,
                Description = route.Description,
                StartLocation = route.StartLocation,
                EndLocation = route.EndLocation,
                Distance = route.Distance,
                EstimatedDurationMinutes = route.EstimatedDurationMinutes,
                Status = route.Status,
                RouteType = route.RouteType,
                RoadType = route.RoadType,
                Coordinates = route.Coordinates,
                TrafficConditions = route.TrafficConditions,
                WeatherRestrictions = route.WeatherRestrictions,
                VehicleRestrictions = route.VehicleRestrictions,
                TollFee = route.TollFee,
                FuelStations = route.FuelStations,
                RestAreas = route.RestAreas,
                IsActive = route.IsActive,
                OrganizationId = route.OrganizationId,
                Notes = route.Notes,
                CreatedAt = route.CreatedAt,
                UpdatedAt = route.UpdatedAt,
                HasWaypoints = false,
                HasSchedules = false,
                HasHistory = false
            };

            return CreatedAtAction(nameof(GetRoute), 
                new { id = route.Id }, 
                ApiResponse<RouteDetailDto>.SuccessResponse(responseDto, "Route created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteDetailDto>.ErrorResponse("Error creating route", ex.Message));
        }
    }

    /// <summary>
    /// Update route
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> UpdateRoute(Guid id, UpdateRouteDto dto)
    {
        try
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound(ApiResponse<RouteDetailDto>.ErrorResponse("Route not found"));
            }

            route.Name = dto.Name;
            route.StartLocation = dto.StartLocation;
            route.EndLocation = dto.EndLocation;
            route.Distance = dto.Distance;
            route.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            route.Status = dto.Status;
            route.RouteType = dto.RouteType;
            route.RoadType = dto.RoadType;
            route.Description = dto.Description ?? string.Empty;
            route.Coordinates = dto.Coordinates;
            route.TrafficConditions = dto.TrafficConditions;
            route.WeatherRestrictions = dto.WeatherRestrictions;
            route.VehicleRestrictions = dto.VehicleRestrictions;
            route.TollFee = dto.TollFee;
            route.FuelStations = dto.FuelStations;
            route.RestAreas = dto.RestAreas;
            route.IsActive = dto.IsActive;
            route.Notes = dto.Notes;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Routes
                .Where(r => r.Id == id)
                .Select(r => new RouteDetailDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    StartLocation = r.StartLocation,
                    EndLocation = r.EndLocation,
                    Distance = r.Distance,
                    EstimatedDurationMinutes = r.EstimatedDurationMinutes,
                    Status = r.Status,
                    RouteType = r.RouteType,
                    RoadType = r.RoadType,
                    Coordinates = r.Coordinates,
                    TrafficConditions = r.TrafficConditions,
                    WeatherRestrictions = r.WeatherRestrictions,
                    VehicleRestrictions = r.VehicleRestrictions,
                    TollFee = r.TollFee,
                    FuelStations = r.FuelStations,
                    RestAreas = r.RestAreas,
                    IsActive = r.IsActive,
                    OrganizationId = r.OrganizationId,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasWaypoints = r.Waypoints.Any(),
                    HasSchedules = r.Schedules.Any(),
                    HasHistory = r.History.Any()
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<RouteDetailDto>.SuccessResponse(responseDto!, "Route updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteDetailDto>.ErrorResponse("Error updating route", ex.Message));
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

    /// <summary>
    /// Partially update route
    /// </summary>
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<RouteDetailDto>>> PatchRoute(Guid id, PatchRouteDto dto)
    {
        try
        {
            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound(ApiResponse<RouteDetailDto>.ErrorResponse("Route not found"));
            }

            if (dto.Name != null) route.Name = dto.Name;
            if (dto.StartLocation != null) route.StartLocation = dto.StartLocation;
            if (dto.EndLocation != null) route.EndLocation = dto.EndLocation;
            if (dto.Distance.HasValue) route.Distance = dto.Distance.Value;
            if (dto.EstimatedDurationMinutes.HasValue) route.EstimatedDurationMinutes = dto.EstimatedDurationMinutes.Value;
            if (dto.Status != null) route.Status = dto.Status;
            if (dto.RouteType != null) route.RouteType = dto.RouteType;
            if (dto.RoadType != null) route.RoadType = dto.RoadType;
            if (dto.Description != null) route.Description = dto.Description;
            if (dto.Coordinates != null) route.Coordinates = dto.Coordinates;
            if (dto.TrafficConditions != null) route.TrafficConditions = dto.TrafficConditions;
            if (dto.WeatherRestrictions != null) route.WeatherRestrictions = dto.WeatherRestrictions;
            if (dto.VehicleRestrictions != null) route.VehicleRestrictions = dto.VehicleRestrictions;
            if (dto.TollFee.HasValue) route.TollFee = dto.TollFee;
            if (dto.FuelStations != null) route.FuelStations = dto.FuelStations;
            if (dto.RestAreas != null) route.RestAreas = dto.RestAreas;
            if (dto.IsActive.HasValue) route.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) route.Notes = dto.Notes;
            route.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Routes
                .Where(r => r.Id == id)
                .Select(r => new RouteDetailDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    StartLocation = r.StartLocation,
                    EndLocation = r.EndLocation,
                    Distance = r.Distance,
                    EstimatedDurationMinutes = r.EstimatedDurationMinutes,
                    Status = r.Status,
                    RouteType = r.RouteType,
                    RoadType = r.RoadType,
                    Coordinates = r.Coordinates,
                    TrafficConditions = r.TrafficConditions,
                    WeatherRestrictions = r.WeatherRestrictions,
                    VehicleRestrictions = r.VehicleRestrictions,
                    TollFee = r.TollFee,
                    FuelStations = r.FuelStations,
                    RestAreas = r.RestAreas,
                    IsActive = r.IsActive,
                    OrganizationId = r.OrganizationId,
                    Notes = r.Notes,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    HasWaypoints = r.Waypoints.Any(),
                    HasSchedules = r.Schedules.Any(),
                    HasHistory = r.History.Any()
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<RouteDetailDto>.SuccessResponse(responseDto!, "Route updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteDetailDto>.ErrorResponse("Error updating route", ex.Message));
        }
    }

    /// <summary>
    /// Get route waypoints
    /// </summary>
    [HttpGet("{id}/waypoints")]
    public async Task<ActionResult<ApiResponse<PagedResult<RouteWaypointDto>>>> GetRouteWaypoints(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.RouteWaypoints
                .Where(w => w.RouteId == id)
                .Select(w => new RouteWaypointDto
                {
                    Id = w.Id,
                    RouteId = w.RouteId,
                    Name = w.Name,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    SequenceOrder = w.SequenceOrder,
                    WaypointType = w.WaypointType,
                    IsMandatory = w.IsMandatory,
                    EstimatedDurationMinutes = w.EstimatedDurationMinutes,
                    DistanceFromPrevious = w.DistanceFromPrevious,
                    Instructions = w.Instructions,
                    Restrictions = w.Restrictions,
                    Services = w.Services,
                    IsActive = w.IsActive,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt
                })
                .OrderBy(w => w.SequenceOrder)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteWaypointDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteWaypointDto>>.ErrorResponse("Error retrieving route waypoints", ex.Message));
        }
    }

    /// <summary>
    /// Add waypoint to route
    /// </summary>
    [HttpPost("{id}/waypoints")]
    public async Task<ActionResult<ApiResponse<RouteWaypointDto>>> CreateRouteWaypoint(Guid id, CreateRouteWaypointDto dto)
    {
        try
        {
            if (!await RouteExists(id))
            {
                return NotFound(ApiResponse<RouteWaypointDto>.ErrorResponse("Route not found"));
            }

            var waypoint = new RouteWaypoint
            {
                Id = Guid.NewGuid(),
                RouteId = id,
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SequenceOrder = dto.SequenceOrder,
                WaypointType = dto.WaypointType,
                IsMandatory = dto.IsMandatory,
                EstimatedDurationMinutes = dto.EstimatedDurationMinutes,
                DistanceFromPrevious = dto.DistanceFromPrevious,
                Instructions = dto.Instructions,
                Restrictions = dto.Restrictions,
                Services = dto.Services,
                Notes = dto.Notes
            };

            _context.RouteWaypoints.Add(waypoint);
            await _context.SaveChangesAsync();

            var responseDto = new RouteWaypointDto
            {
                Id = waypoint.Id,
                RouteId = waypoint.RouteId,
                Name = waypoint.Name,
                Latitude = waypoint.Latitude,
                Longitude = waypoint.Longitude,
                SequenceOrder = waypoint.SequenceOrder,
                WaypointType = waypoint.WaypointType,
                IsMandatory = waypoint.IsMandatory,
                EstimatedDurationMinutes = waypoint.EstimatedDurationMinutes,
                DistanceFromPrevious = waypoint.DistanceFromPrevious,
                Instructions = waypoint.Instructions,
                Restrictions = waypoint.Restrictions,
                Services = waypoint.Services,
                IsActive = waypoint.IsActive,
                Notes = waypoint.Notes,
                CreatedAt = waypoint.CreatedAt
            };

            return Ok(ApiResponse<RouteWaypointDto>.SuccessResponse(responseDto, "Route waypoint created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteWaypointDto>.ErrorResponse("Error creating route waypoint", ex.Message));
        }
    }

    /// <summary>
    /// Get single route waypoint
    /// </summary>
    [HttpGet("{id}/waypoints/{waypointId}")]
    public async Task<ActionResult<ApiResponse<RouteWaypointDto>>> GetRouteWaypoint(Guid id, Guid waypointId)
    {
        try
        {
            var waypoint = await _context.RouteWaypoints
                .Where(w => w.Id == waypointId && w.RouteId == id)
                .Select(w => new RouteWaypointDto
                {
                    Id = w.Id,
                    RouteId = w.RouteId,
                    Name = w.Name,
                    Latitude = w.Latitude,
                    Longitude = w.Longitude,
                    SequenceOrder = w.SequenceOrder,
                    WaypointType = w.WaypointType,
                    IsMandatory = w.IsMandatory,
                    EstimatedDurationMinutes = w.EstimatedDurationMinutes,
                    DistanceFromPrevious = w.DistanceFromPrevious,
                    Instructions = w.Instructions,
                    Restrictions = w.Restrictions,
                    Services = w.Services,
                    IsActive = w.IsActive,
                    Notes = w.Notes,
                    CreatedAt = w.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (waypoint == null)
            {
                return NotFound(ApiResponse<RouteWaypointDto>.ErrorResponse("Waypoint not found"));
            }

            return Ok(ApiResponse<RouteWaypointDto>.SuccessResponse(waypoint));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteWaypointDto>.ErrorResponse("Error retrieving waypoint", ex.Message));
        }
    }

    /// <summary>
    /// Update route waypoint
    /// </summary>
    [HttpPut("{id}/waypoints/{waypointId}")]
    public async Task<ActionResult<ApiResponse<RouteWaypointDto>>> UpdateRouteWaypoint(Guid id, Guid waypointId, UpdateRouteWaypointDto dto)
    {
        try
        {
            var waypoint = await _context.RouteWaypoints
                .FirstOrDefaultAsync(w => w.Id == waypointId && w.RouteId == id);

            if (waypoint == null)
            {
                return NotFound(ApiResponse<RouteWaypointDto>.ErrorResponse("Waypoint not found"));
            }

            waypoint.Name = dto.Name;
            waypoint.Latitude = dto.Latitude;
            waypoint.Longitude = dto.Longitude;
            waypoint.SequenceOrder = dto.SequenceOrder;
            waypoint.WaypointType = dto.WaypointType;
            waypoint.IsMandatory = dto.IsMandatory;
            waypoint.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            waypoint.DistanceFromPrevious = dto.DistanceFromPrevious;
            waypoint.Instructions = dto.Instructions;
            waypoint.Restrictions = dto.Restrictions;
            waypoint.Services = dto.Services;
            waypoint.IsActive = dto.IsActive;
            waypoint.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteWaypointDto
            {
                Id = waypoint.Id,
                RouteId = waypoint.RouteId,
                Name = waypoint.Name,
                Latitude = waypoint.Latitude,
                Longitude = waypoint.Longitude,
                SequenceOrder = waypoint.SequenceOrder,
                WaypointType = waypoint.WaypointType,
                IsMandatory = waypoint.IsMandatory,
                EstimatedDurationMinutes = waypoint.EstimatedDurationMinutes,
                DistanceFromPrevious = waypoint.DistanceFromPrevious,
                Instructions = waypoint.Instructions,
                Restrictions = waypoint.Restrictions,
                Services = waypoint.Services,
                IsActive = waypoint.IsActive,
                Notes = waypoint.Notes,
                CreatedAt = waypoint.CreatedAt
            };

            return Ok(ApiResponse<RouteWaypointDto>.SuccessResponse(responseDto, "Waypoint updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteWaypointDto>.ErrorResponse("Error updating waypoint", ex.Message));
        }
    }

    /// <summary>
    /// Partially update route waypoint
    /// </summary>
    [HttpPatch("{id}/waypoints/{waypointId}")]
    public async Task<ActionResult<ApiResponse<RouteWaypointDto>>> PatchRouteWaypoint(Guid id, Guid waypointId, PatchRouteWaypointDto dto)
    {
        try
        {
            var waypoint = await _context.RouteWaypoints
                .FirstOrDefaultAsync(w => w.Id == waypointId && w.RouteId == id);

            if (waypoint == null)
            {
                return NotFound(ApiResponse<RouteWaypointDto>.ErrorResponse("Waypoint not found"));
            }

            if (dto.Name != null) waypoint.Name = dto.Name;
            if (dto.Latitude.HasValue) waypoint.Latitude = dto.Latitude.Value;
            if (dto.Longitude.HasValue) waypoint.Longitude = dto.Longitude.Value;
            if (dto.SequenceOrder.HasValue) waypoint.SequenceOrder = dto.SequenceOrder.Value;
            if (dto.WaypointType != null) waypoint.WaypointType = dto.WaypointType;
            if (dto.IsMandatory.HasValue) waypoint.IsMandatory = dto.IsMandatory.Value;
            if (dto.EstimatedDurationMinutes.HasValue) waypoint.EstimatedDurationMinutes = dto.EstimatedDurationMinutes;
            if (dto.DistanceFromPrevious.HasValue) waypoint.DistanceFromPrevious = dto.DistanceFromPrevious;
            if (dto.Instructions != null) waypoint.Instructions = dto.Instructions;
            if (dto.Restrictions != null) waypoint.Restrictions = dto.Restrictions;
            if (dto.Services != null) waypoint.Services = dto.Services;
            if (dto.IsActive.HasValue) waypoint.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) waypoint.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteWaypointDto
            {
                Id = waypoint.Id,
                RouteId = waypoint.RouteId,
                Name = waypoint.Name,
                Latitude = waypoint.Latitude,
                Longitude = waypoint.Longitude,
                SequenceOrder = waypoint.SequenceOrder,
                WaypointType = waypoint.WaypointType,
                IsMandatory = waypoint.IsMandatory,
                EstimatedDurationMinutes = waypoint.EstimatedDurationMinutes,
                DistanceFromPrevious = waypoint.DistanceFromPrevious,
                Instructions = waypoint.Instructions,
                Restrictions = waypoint.Restrictions,
                Services = waypoint.Services,
                IsActive = waypoint.IsActive,
                Notes = waypoint.Notes,
                CreatedAt = waypoint.CreatedAt
            };

            return Ok(ApiResponse<RouteWaypointDto>.SuccessResponse(responseDto, "Waypoint updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteWaypointDto>.ErrorResponse("Error updating waypoint", ex.Message));
        }
    }

    /// <summary>
    /// Delete route waypoint
    /// </summary>
    [HttpDelete("{id}/waypoints/{waypointId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRouteWaypoint(Guid id, Guid waypointId)
    {
        try
        {
            var waypoint = await _context.RouteWaypoints
                .FirstOrDefaultAsync(w => w.Id == waypointId && w.RouteId == id);

            if (waypoint == null)
            {
                return NotFound(ApiResponse.CreateError("Waypoint not found"));
            }

            _context.RouteWaypoints.Remove(waypoint);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Waypoint deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting waypoint", ex.Message));
        }
    }

    /// <summary>
    /// Get route schedules
    /// </summary>
    [HttpGet("{id}/schedules")]
    public async Task<ActionResult<ApiResponse<PagedResult<RouteScheduleDto>>>> GetRouteSchedules(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.RouteSchedules
                .Where(s => s.RouteId == id)
                .Select(s => new RouteScheduleDto
                {
                    Id = s.Id,
                    RouteId = s.RouteId,
                    ScheduleName = s.ScheduleName,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    DaysOfWeek = s.DaysOfWeek,
                    EffectiveDate = s.EffectiveDate,
                    ExpiryDate = s.ExpiryDate,
                    Frequency = s.Frequency,
                    ScheduleType = s.ScheduleType,
                    PriceModifier = s.PriceModifier,
                    IsActive = s.IsActive,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt,
                    IsExpired = s.ExpiryDate.HasValue && s.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(s => s.EffectiveDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteScheduleDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteScheduleDto>>.ErrorResponse("Error retrieving route schedules", ex.Message));
        }
    }

    /// <summary>
    /// Get single route schedule
    /// </summary>
    [HttpGet("{id}/schedules/{scheduleId}")]
    public async Task<ActionResult<ApiResponse<RouteScheduleDto>>> GetRouteSchedule(Guid id, Guid scheduleId)
    {
        try
        {
            var schedule = await _context.RouteSchedules
                .Where(s => s.Id == scheduleId && s.RouteId == id)
                .Select(s => new RouteScheduleDto
                {
                    Id = s.Id,
                    RouteId = s.RouteId,
                    ScheduleName = s.ScheduleName,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    DaysOfWeek = s.DaysOfWeek,
                    EffectiveDate = s.EffectiveDate,
                    ExpiryDate = s.ExpiryDate,
                    Frequency = s.Frequency,
                    ScheduleType = s.ScheduleType,
                    PriceModifier = s.PriceModifier,
                    IsActive = s.IsActive,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt,
                    IsExpired = s.ExpiryDate.HasValue && s.ExpiryDate < DateTime.Today
                })
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound(ApiResponse<RouteScheduleDto>.ErrorResponse("Schedule not found"));
            }

            return Ok(ApiResponse<RouteScheduleDto>.SuccessResponse(schedule));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteScheduleDto>.ErrorResponse("Error retrieving schedule", ex.Message));
        }
    }

    /// <summary>
    /// Add schedule to route
    /// </summary>
    [HttpPost("{id}/schedules")]
    public async Task<ActionResult<ApiResponse<RouteScheduleDto>>> CreateRouteSchedule(Guid id, CreateRouteScheduleDto dto)
    {
        try
        {
            if (!await RouteExists(id))
            {
                return NotFound(ApiResponse<RouteScheduleDto>.ErrorResponse("Route not found"));
            }

            var schedule = new RouteSchedule
            {
                Id = Guid.NewGuid(),
                RouteId = id,
                ScheduleName = dto.ScheduleName,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                DaysOfWeek = dto.DaysOfWeek,
                EffectiveDate = dto.EffectiveDate,
                ExpiryDate = dto.ExpiryDate,
                Frequency = dto.Frequency,
                ScheduleType = dto.ScheduleType,
                PriceModifier = dto.PriceModifier,
                Notes = dto.Notes
            };

            _context.RouteSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var responseDto = new RouteScheduleDto
            {
                Id = schedule.Id,
                RouteId = schedule.RouteId,
                ScheduleName = schedule.ScheduleName,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                DaysOfWeek = schedule.DaysOfWeek,
                EffectiveDate = schedule.EffectiveDate,
                ExpiryDate = schedule.ExpiryDate,
                Frequency = schedule.Frequency,
                ScheduleType = schedule.ScheduleType,
                PriceModifier = schedule.PriceModifier,
                IsActive = schedule.IsActive,
                Notes = schedule.Notes,
                CreatedAt = schedule.CreatedAt,
                IsExpired = schedule.ExpiryDate.HasValue && schedule.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<RouteScheduleDto>.SuccessResponse(responseDto, "Route schedule created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteScheduleDto>.ErrorResponse("Error creating route schedule", ex.Message));
        }
    }

    /// <summary>
    /// Update route schedule
    /// </summary>
    [HttpPut("{id}/schedules/{scheduleId}")]
    public async Task<ActionResult<ApiResponse<RouteScheduleDto>>> UpdateRouteSchedule(Guid id, Guid scheduleId, UpdateRouteScheduleDto dto)
    {
        try
        {
            var schedule = await _context.RouteSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.RouteId == id);

            if (schedule == null)
            {
                return NotFound(ApiResponse<RouteScheduleDto>.ErrorResponse("Schedule not found"));
            }

            schedule.ScheduleName = dto.ScheduleName;
            schedule.DepartureTime = dto.DepartureTime;
            schedule.ArrivalTime = dto.ArrivalTime;
            schedule.DaysOfWeek = dto.DaysOfWeek;
            schedule.EffectiveDate = dto.EffectiveDate;
            schedule.ExpiryDate = dto.ExpiryDate;
            schedule.Frequency = dto.Frequency;
            schedule.ScheduleType = dto.ScheduleType;
            schedule.PriceModifier = dto.PriceModifier;
            schedule.IsActive = dto.IsActive;
            schedule.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteScheduleDto
            {
                Id = schedule.Id,
                RouteId = schedule.RouteId,
                ScheduleName = schedule.ScheduleName,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                DaysOfWeek = schedule.DaysOfWeek,
                EffectiveDate = schedule.EffectiveDate,
                ExpiryDate = schedule.ExpiryDate,
                Frequency = schedule.Frequency,
                ScheduleType = schedule.ScheduleType,
                PriceModifier = schedule.PriceModifier,
                IsActive = schedule.IsActive,
                Notes = schedule.Notes,
                CreatedAt = schedule.CreatedAt,
                IsExpired = schedule.ExpiryDate.HasValue && schedule.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<RouteScheduleDto>.SuccessResponse(responseDto, "Schedule updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteScheduleDto>.ErrorResponse("Error updating schedule", ex.Message));
        }
    }

    /// <summary>
    /// Partially update route schedule
    /// </summary>
    [HttpPatch("{id}/schedules/{scheduleId}")]
    public async Task<ActionResult<ApiResponse<RouteScheduleDto>>> PatchRouteSchedule(Guid id, Guid scheduleId, PatchRouteScheduleDto dto)
    {
        try
        {
            var schedule = await _context.RouteSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.RouteId == id);

            if (schedule == null)
            {
                return NotFound(ApiResponse<RouteScheduleDto>.ErrorResponse("Schedule not found"));
            }

            if (dto.ScheduleName != null) schedule.ScheduleName = dto.ScheduleName;
            if (dto.DepartureTime.HasValue) schedule.DepartureTime = dto.DepartureTime.Value;
            if (dto.ArrivalTime.HasValue) schedule.ArrivalTime = dto.ArrivalTime.Value;
            if (dto.DaysOfWeek != null) schedule.DaysOfWeek = dto.DaysOfWeek;
            if (dto.EffectiveDate.HasValue) schedule.EffectiveDate = dto.EffectiveDate.Value;
            if (dto.ExpiryDate.HasValue) schedule.ExpiryDate = dto.ExpiryDate;
            if (dto.Frequency.HasValue) schedule.Frequency = dto.Frequency.Value;
            if (dto.ScheduleType != null) schedule.ScheduleType = dto.ScheduleType;
            if (dto.PriceModifier.HasValue) schedule.PriceModifier = dto.PriceModifier;
            if (dto.IsActive.HasValue) schedule.IsActive = dto.IsActive.Value;
            if (dto.Notes != null) schedule.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteScheduleDto
            {
                Id = schedule.Id,
                RouteId = schedule.RouteId,
                ScheduleName = schedule.ScheduleName,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                DaysOfWeek = schedule.DaysOfWeek,
                EffectiveDate = schedule.EffectiveDate,
                ExpiryDate = schedule.ExpiryDate,
                Frequency = schedule.Frequency,
                ScheduleType = schedule.ScheduleType,
                PriceModifier = schedule.PriceModifier,
                IsActive = schedule.IsActive,
                Notes = schedule.Notes,
                CreatedAt = schedule.CreatedAt,
                IsExpired = schedule.ExpiryDate.HasValue && schedule.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<RouteScheduleDto>.SuccessResponse(responseDto, "Schedule updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteScheduleDto>.ErrorResponse("Error updating schedule", ex.Message));
        }
    }

    /// <summary>
    /// Delete route schedule
    /// </summary>
    [HttpDelete("{id}/schedules/{scheduleId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRouteSchedule(Guid id, Guid scheduleId)
    {
        try
        {
            var schedule = await _context.RouteSchedules
                .FirstOrDefaultAsync(s => s.Id == scheduleId && s.RouteId == id);

            if (schedule == null)
            {
                return NotFound(ApiResponse.CreateError("Schedule not found"));
            }

            _context.RouteSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Schedule deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting schedule", ex.Message));
        }
    }

    /// <summary>
    /// Get route history
    /// </summary>
    [HttpGet("{id}/history")]
    public async Task<ActionResult<ApiResponse<PagedResult<RouteHistoryDto>>>> GetRouteHistory(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.RouteHistories
                .Where(h => h.RouteId == id)
                .Select(h => new RouteHistoryDto
                {
                    Id = h.Id,
                    RouteId = h.RouteId,
                    TripDate = h.TripDate,
                    ActualDepartureTime = h.ActualDepartureTime,
                    ActualArrivalTime = h.ActualArrivalTime,
                    ActualDurationMinutes = h.ActualDurationMinutes,
                    DriverId = h.DriverId,
                    VehicleId = h.VehicleId,
                    TripStatus = h.TripStatus,
                    DelayReason = h.DelayReason,
                    DelayMinutes = h.DelayMinutes,
                    ActualDistance = h.ActualDistance,
                    FuelConsumed = h.FuelConsumed,
                    FuelCost = h.FuelCost,
                    TollsPaid = h.TollsPaid,
                    Incidents = h.Incidents,
                    WeatherConditions = h.WeatherConditions,
                    TrafficConditions = h.TrafficConditions,
                    PassengerCount = h.PassengerCount,
                    Revenue = h.Revenue,
                    Notes = h.Notes,
                    CreatedAt = h.CreatedAt,
                    WasDelayed = h.DelayMinutes.HasValue && h.DelayMinutes > 0,
                    EfficiencyRating = h.ActualDistance > 0 ? h.FuelConsumed / h.ActualDistance * 100 : null
                })
                .OrderByDescending(h => h.TripDate)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<RouteHistoryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<RouteHistoryDto>>.ErrorResponse("Error retrieving route history", ex.Message));
        }
    }

    /// <summary>
    /// Add history record to route
    /// </summary>
    [HttpPost("{id}/history")]
    public async Task<ActionResult<ApiResponse<RouteHistoryDto>>> CreateRouteHistory(Guid id, CreateRouteHistoryDto dto)
    {
        try
        {
            if (!await RouteExists(id))
            {
                return NotFound(ApiResponse<RouteHistoryDto>.ErrorResponse("Route not found"));
            }

            var history = new RouteHistory
            {
                Id = Guid.NewGuid(),
                RouteId = id,
                TripDate = dto.TripDate,
                ActualDepartureTime = dto.ActualDepartureTime,
                ActualArrivalTime = dto.ActualArrivalTime,
                ActualDurationMinutes = dto.ActualDurationMinutes,
                DriverId = dto.DriverId,
                VehicleId = dto.VehicleId,
                TripStatus = dto.TripStatus,
                DelayReason = dto.DelayReason,
                DelayMinutes = dto.DelayMinutes,
                ActualDistance = dto.ActualDistance,
                FuelConsumed = dto.FuelConsumed,
                FuelCost = dto.FuelCost,
                TollsPaid = dto.TollsPaid,
                Incidents = dto.Incidents,
                WeatherConditions = dto.WeatherConditions,
                TrafficConditions = dto.TrafficConditions,
                PassengerCount = dto.PassengerCount,
                Revenue = dto.Revenue,
                Notes = dto.Notes
            };

            _context.RouteHistories.Add(history);
            await _context.SaveChangesAsync();

            var responseDto = new RouteHistoryDto
            {
                Id = history.Id,
                RouteId = history.RouteId,
                TripDate = history.TripDate,
                ActualDepartureTime = history.ActualDepartureTime,
                ActualArrivalTime = history.ActualArrivalTime,
                ActualDurationMinutes = history.ActualDurationMinutes,
                DriverId = history.DriverId,
                VehicleId = history.VehicleId,
                TripStatus = history.TripStatus,
                DelayReason = history.DelayReason,
                DelayMinutes = history.DelayMinutes,
                ActualDistance = history.ActualDistance,
                FuelConsumed = history.FuelConsumed,
                FuelCost = history.FuelCost,
                TollsPaid = history.TollsPaid,
                Incidents = history.Incidents,
                WeatherConditions = history.WeatherConditions,
                TrafficConditions = history.TrafficConditions,
                PassengerCount = history.PassengerCount,
                Revenue = history.Revenue,
                Notes = history.Notes,
                CreatedAt = history.CreatedAt,
                WasDelayed = history.DelayMinutes.HasValue && history.DelayMinutes > 0,
                EfficiencyRating = history.ActualDistance > 0 ? history.FuelConsumed / history.ActualDistance * 100 : null
            };

            return Ok(ApiResponse<RouteHistoryDto>.SuccessResponse(responseDto, "Route history record created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteHistoryDto>.ErrorResponse("Error creating route history record", ex.Message));
        }
    }

    /// <summary>
    /// Update route history record
    /// </summary>
    [HttpPut("{id}/history/{historyId}")]
    public async Task<ActionResult<ApiResponse<RouteHistoryDto>>> UpdateRouteHistory(Guid id, Guid historyId, UpdateRouteHistoryDto dto)
    {
        try
        {
            var history = await _context.RouteHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.RouteId == id);

            if (history == null)
            {
                return NotFound(ApiResponse<RouteHistoryDto>.ErrorResponse("History record not found"));
            }

            history.TripDate = dto.TripDate;
            history.ActualDepartureTime = dto.ActualDepartureTime;
            history.ActualArrivalTime = dto.ActualArrivalTime;
            history.ActualDurationMinutes = dto.ActualDurationMinutes;
            history.ActualDistance = dto.ActualDistance;
            history.FuelConsumed = dto.FuelConsumed;
            history.TripStatus = dto.TripStatus;
            history.DriverId = dto.DriverId;
            history.VehicleId = dto.VehicleId;
            history.DelayReason = dto.DelayReason;
            history.DelayMinutes = dto.DelayMinutes;
            history.FuelCost = dto.FuelCost;
            history.TollsPaid = dto.TollsPaid;
            history.Incidents = dto.Incidents;
            history.WeatherConditions = dto.WeatherConditions;
            history.TrafficConditions = dto.TrafficConditions;
            history.PassengerCount = dto.PassengerCount;
            history.Revenue = dto.Revenue;
            history.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteHistoryDto
            {
                Id = history.Id,
                RouteId = history.RouteId,
                TripDate = history.TripDate,
                ActualDepartureTime = history.ActualDepartureTime,
                ActualArrivalTime = history.ActualArrivalTime,
                ActualDurationMinutes = history.ActualDurationMinutes,
                DriverId = history.DriverId,
                VehicleId = history.VehicleId,
                TripStatus = history.TripStatus,
                DelayReason = history.DelayReason,
                DelayMinutes = history.DelayMinutes,
                ActualDistance = history.ActualDistance,
                FuelConsumed = history.FuelConsumed,
                FuelCost = history.FuelCost,
                TollsPaid = history.TollsPaid,
                Incidents = history.Incidents,
                WeatherConditions = history.WeatherConditions,
                TrafficConditions = history.TrafficConditions,
                PassengerCount = history.PassengerCount,
                Revenue = history.Revenue,
                Notes = history.Notes,
                CreatedAt = history.CreatedAt,
                WasDelayed = history.DelayMinutes.HasValue && history.DelayMinutes > 0,
                EfficiencyRating = history.ActualDistance > 0 ? history.FuelConsumed / history.ActualDistance * 100 : null
            };

            return Ok(ApiResponse<RouteHistoryDto>.SuccessResponse(responseDto, "History record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteHistoryDto>.ErrorResponse("Error updating history record", ex.Message));
        }
    }

    /// <summary>
    /// Partially update route history record
    /// </summary>
    [HttpPatch("{id}/history/{historyId}")]
    public async Task<ActionResult<ApiResponse<RouteHistoryDto>>> PatchRouteHistory(Guid id, Guid historyId, PatchRouteHistoryDto dto)
    {
        try
        {
            var history = await _context.RouteHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.RouteId == id);

            if (history == null)
            {
                return NotFound(ApiResponse<RouteHistoryDto>.ErrorResponse("History record not found"));
            }

            if (dto.TripDate.HasValue) history.TripDate = dto.TripDate.Value;
            if (dto.ActualDepartureTime.HasValue) history.ActualDepartureTime = dto.ActualDepartureTime.Value;
            if (dto.ActualArrivalTime.HasValue) history.ActualArrivalTime = dto.ActualArrivalTime.Value;
            if (dto.ActualDurationMinutes.HasValue) history.ActualDurationMinutes = dto.ActualDurationMinutes.Value;
            if (dto.ActualDistance.HasValue) history.ActualDistance = dto.ActualDistance.Value;
            if (dto.FuelConsumed.HasValue) history.FuelConsumed = dto.FuelConsumed.Value;
            if (dto.TripStatus != null) history.TripStatus = dto.TripStatus;
            if (dto.DriverId != null) history.DriverId = dto.DriverId;
            if (dto.VehicleId != null) history.VehicleId = dto.VehicleId;
            if (dto.DelayReason != null) history.DelayReason = dto.DelayReason;
            if (dto.DelayMinutes.HasValue) history.DelayMinutes = dto.DelayMinutes;
            if (dto.FuelCost.HasValue) history.FuelCost = dto.FuelCost;
            if (dto.TollsPaid.HasValue) history.TollsPaid = dto.TollsPaid;
            if (dto.Incidents != null) history.Incidents = dto.Incidents;
            if (dto.WeatherConditions != null) history.WeatherConditions = dto.WeatherConditions;
            if (dto.TrafficConditions != null) history.TrafficConditions = dto.TrafficConditions;
            if (dto.PassengerCount.HasValue) history.PassengerCount = dto.PassengerCount;
            if (dto.Revenue.HasValue) history.Revenue = dto.Revenue;
            if (dto.Notes != null) history.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new RouteHistoryDto
            {
                Id = history.Id,
                RouteId = history.RouteId,
                TripDate = history.TripDate,
                ActualDepartureTime = history.ActualDepartureTime,
                ActualArrivalTime = history.ActualArrivalTime,
                ActualDurationMinutes = history.ActualDurationMinutes,
                DriverId = history.DriverId,
                VehicleId = history.VehicleId,
                TripStatus = history.TripStatus,
                DelayReason = history.DelayReason,
                DelayMinutes = history.DelayMinutes,
                ActualDistance = history.ActualDistance,
                FuelConsumed = history.FuelConsumed,
                FuelCost = history.FuelCost,
                TollsPaid = history.TollsPaid,
                Incidents = history.Incidents,
                WeatherConditions = history.WeatherConditions,
                TrafficConditions = history.TrafficConditions,
                PassengerCount = history.PassengerCount,
                Revenue = history.Revenue,
                Notes = history.Notes,
                CreatedAt = history.CreatedAt,
                WasDelayed = history.DelayMinutes.HasValue && history.DelayMinutes > 0,
                EfficiencyRating = history.ActualDistance > 0 ? history.FuelConsumed / history.ActualDistance * 100 : null
            };

            return Ok(ApiResponse<RouteHistoryDto>.SuccessResponse(responseDto, "History record updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<RouteHistoryDto>.ErrorResponse("Error updating history record", ex.Message));
        }
    }

    /// <summary>
    /// Delete route history record
    /// </summary>
    [HttpDelete("{id}/history/{historyId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRouteHistory(Guid id, Guid historyId)
    {
        try
        {
            var history = await _context.RouteHistories
                .FirstOrDefaultAsync(h => h.Id == historyId && h.RouteId == id);

            if (history == null)
            {
                return NotFound(ApiResponse.CreateError("History record not found"));
            }

            _context.RouteHistories.Remove(history);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("History record deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting history record", ex.Message));
        }
    }

    private async Task<bool> RouteExists(Guid id)
    {
        return await _context.Routes.AnyAsync(e => e.Id == id);
    }
}