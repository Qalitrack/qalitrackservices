using Microsoft.AspNetCore.Mvc;
using RouteService.Core.DTOs;
using RouteService.Core.Interfaces;

namespace RouteService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;
    private readonly ILogger<RoutesController> _logger;

    public RoutesController(IRouteService routeService, ILogger<RoutesController> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<RouteDto>>>> GetRoutes(
        [FromQuery] string? origin = null,
        [FromQuery] string? destination = null,
        [FromQuery] string? search = null,
        [FromQuery] bool activeOnly = false)
    {
        try
        {
            IEnumerable<RouteDto> routes;

            if (!string.IsNullOrEmpty(search))
            {
                routes = await _routeService.SearchRoutesAsync(search);
            }
            else if (!string.IsNullOrEmpty(origin) && !string.IsNullOrEmpty(destination))
            {
                // For vehicle specifications, you'd typically get this from query params or request body
                var vehicleSpecs = new VehicleSpecifications();
                routes = await _routeService.GetAvailableRoutesAsync(origin, destination, vehicleSpecs);
            }
            else if (activeOnly)
            {
                routes = await _routeService.GetActiveRoutesAsync();
            }
            else
            {
                routes = await _routeService.GetAllRoutesAsync();
            }

            return Ok(new ApiResponseDto<IEnumerable<RouteDto>>
            {
                Success = true,
                Data = routes,
                Message = "Routes retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving routes");
            return StatusCode(500, new ApiResponseDto<IEnumerable<RouteDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving routes",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<RouteDto>>> GetRoute(string id)
    {
        try
        {
            var route = await _routeService.GetRouteAsync(id);
            if (route == null)
            {
                return NotFound(new ApiResponseDto<RouteDto>
                {
                    Success = false,
                    Message = $"Route with ID '{id}' not found"
                });
            }

            return Ok(new ApiResponseDto<RouteDto>
            {
                Success = true,
                Data = route,
                Message = "Route retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route {RouteId}", id);
            return StatusCode(500, new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("code/{code}")]
    public async Task<ActionResult<ApiResponseDto<RouteDto>>> GetRouteByCode(string code)
    {
        try
        {
            var route = await _routeService.GetRouteByCodeAsync(code);
            if (route == null)
            {
                return NotFound(new ApiResponseDto<RouteDto>
                {
                    Success = false,
                    Message = $"Route with code '{code}' not found"
                });
            }

            return Ok(new ApiResponseDto<RouteDto>
            {
                Success = true,
                Data = route,
                Message = "Route retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route by code {RouteCode}", code);
            return StatusCode(500, new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<RouteDto>>> CreateRoute([FromBody] CreateRouteRequest request)
    {
        try
        {
            var route = await _routeService.CreateRouteAsync(request);
            return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, new ApiResponseDto<RouteDto>
            {
                Success = true,
                Data = route,
                Message = "Route created successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route");
            return StatusCode(500, new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = "An error occurred while creating the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<RouteDto>>> UpdateRoute(string id, [FromBody] UpdateRouteRequest request)
    {
        try
        {
            var route = await _routeService.UpdateRouteAsync(id, request);
            return Ok(new ApiResponseDto<RouteDto>
            {
                Success = true,
                Data = route,
                Message = "Route updated successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating route {RouteId}", id);
            return StatusCode(500, new ApiResponseDto<RouteDto>
            {
                Success = false,
                Message = "An error occurred while updating the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponseDto>> DeleteRoute(string id)
    {
        try
        {
            var deleted = await _routeService.DeleteRouteAsync(id);
            if (!deleted)
            {
                return NotFound(new ApiResponseDto
                {
                    Success = false,
                    Message = $"Route with ID '{id}' not found"
                });
            }

            return Ok(new ApiResponseDto
            {
                Success = true,
                Message = "Route deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting route {RouteId}", id);
            return StatusCode(500, new ApiResponseDto
            {
                Success = false,
                Message = "An error occurred while deleting the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // Waypoint endpoints
    [HttpGet("{routeId}/waypoints")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<RouteWaypointDto>>>> GetWaypoints(string routeId)
    {
        try
        {
            var waypoints = await _routeService.GetWaypointsAsync(routeId);
            return Ok(new ApiResponseDto<IEnumerable<RouteWaypointDto>>
            {
                Success = true,
                Data = waypoints,
                Message = "Waypoints retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving waypoints for route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<IEnumerable<RouteWaypointDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving waypoints",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{routeId}/waypoints")]
    public async Task<ActionResult<ApiResponseDto<RouteWaypointDto>>> AddWaypoint(string routeId, [FromBody] CreateRouteWaypointRequest request)
    {
        try
        {
            var waypoint = await _routeService.AddWaypointAsync(routeId, request);
            return Ok(new ApiResponseDto<RouteWaypointDto>
            {
                Success = true,
                Data = waypoint,
                Message = "Waypoint added successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponseDto<RouteWaypointDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding waypoint to route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<RouteWaypointDto>
            {
                Success = false,
                Message = "An error occurred while adding the waypoint",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // Restriction endpoints
    [HttpGet("{routeId}/restrictions")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<RouteRestrictionDto>>>> GetRestrictions(string routeId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var restrictions = activeOnly 
                ? await _routeService.GetActiveRestrictionsAsync(routeId)
                : await _routeService.GetRestrictionsAsync(routeId);
                
            return Ok(new ApiResponseDto<IEnumerable<RouteRestrictionDto>>
            {
                Success = true,
                Data = restrictions,
                Message = "Restrictions retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving restrictions for route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<IEnumerable<RouteRestrictionDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving restrictions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{routeId}/restrictions")]
    public async Task<ActionResult<ApiResponseDto<RouteRestrictionDto>>> AddRestriction(string routeId, [FromBody] CreateRouteRestrictionRequest request)
    {
        try
        {
            var restriction = await _routeService.AddRestrictionAsync(routeId, request);
            return Ok(new ApiResponseDto<RouteRestrictionDto>
            {
                Success = true,
                Data = restriction,
                Message = "Restriction added successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponseDto<RouteRestrictionDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding restriction to route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<RouteRestrictionDto>
            {
                Success = false,
                Message = "An error occurred while adding the restriction",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    // Condition endpoints
    [HttpGet("{routeId}/conditions")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<RouteConditionDto>>>> GetConditions(string routeId, [FromQuery] bool activeOnly = false)
    {
        try
        {
            var conditions = activeOnly 
                ? await _routeService.GetActiveConditionsAsync(routeId)
                : await _routeService.GetConditionsAsync(routeId);
                
            return Ok(new ApiResponseDto<IEnumerable<RouteConditionDto>>
            {
                Success = true,
                Data = conditions,
                Message = "Conditions retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conditions for route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<IEnumerable<RouteConditionDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving conditions",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{routeId}/conditions")]
    public async Task<ActionResult<ApiResponseDto<RouteConditionDto>>> ReportCondition(string routeId, [FromBody] CreateRouteConditionRequest request)
    {
        try
        {
            var condition = await _routeService.ReportConditionAsync(routeId, request);
            return Ok(new ApiResponseDto<RouteConditionDto>
            {
                Success = true,
                Data = condition,
                Message = "Condition reported successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponseDto<RouteConditionDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting condition for route {RouteId}", routeId);
            return StatusCode(500, new ApiResponseDto<RouteConditionDto>
            {
                Success = false,
                Message = "An error occurred while reporting the condition",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{id}/map")]
    public async Task<ActionResult<ApiResponseDto<RouteMapDto>>> GetRouteMap(string id)
    {
        try
        {
            var routeMap = await _routeService.GetRouteMapAsync(id);
            if (routeMap == null)
            {
                return NotFound(new ApiResponseDto<RouteMapDto>
                {
                    Success = false,
                    Message = $"Route map for ID '{id}' not found"
                });
            }

            return Ok(new ApiResponseDto<RouteMapDto>
            {
                Success = true,
                Data = routeMap,
                Message = "Route map retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route map for {RouteId}", id);
            return StatusCode(500, new ApiResponseDto<RouteMapDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the route map",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{id}/traffic")]
    public async Task<ActionResult<ApiResponseDto<RouteTrafficDto>>> GetRouteTraffic(string id)
    {
        try
        {
            var routeTraffic = await _routeService.GetRouteTrafficAsync(id);
            if (routeTraffic == null)
            {
                return NotFound(new ApiResponseDto<RouteTrafficDto>
                {
                    Success = false,
                    Message = $"Route traffic for ID '{id}' not found"
                });
            }

            return Ok(new ApiResponseDto<RouteTrafficDto>
            {
                Success = true,
                Data = routeTraffic,
                Message = "Route traffic retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving route traffic for {RouteId}", id);
            return StatusCode(500, new ApiResponseDto<RouteTrafficDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the route traffic",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost("{id}/optimize")]
    public async Task<ActionResult<ApiResponseDto<RouteOptimizationDto>>> OptimizeRoute(string id, [FromBody] RouteOptimizationRequestDto request)
    {
        try
        {
            var optimizedRoute = await _routeService.OptimizeRouteAsync(id, request);
            return Ok(new ApiResponseDto<RouteOptimizationDto>
            {
                Success = true,
                Data = optimizedRoute,
                Message = "Route optimized successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponseDto<RouteOptimizationDto>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing route {RouteId}", id);
            return StatusCode(500, new ApiResponseDto<RouteOptimizationDto>
            {
                Success = false,
                Message = "An error occurred while optimizing the route",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}