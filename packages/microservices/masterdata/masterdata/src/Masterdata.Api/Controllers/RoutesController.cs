using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Route;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class RoutesController : ControllerBase
    {
        private readonly IRouteService _routeService;
        private readonly ILogger<RoutesController> _logger;

        public RoutesController(
            IRouteService routeService,
            ILogger<RoutesController> logger)
        {
            _routeService = routeService ?? throw new ArgumentNullException(nameof(routeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all routes with pagination and search
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<RouteDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<RouteDto>>> GetRoutes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _routeService.GetPagedRoutesAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving routes");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving routes");
            }
        }

        /// <summary>
        /// Get a specific route by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RouteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RouteDto>> GetRouteById(Guid id)
        {
            try
            {
                var route = await _routeService.GetByIdAsync(id);
                if (route == null)
                {
                    return NotFound();
                }
                return Ok(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving route with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving the route with ID: {id}");
            }
        }

        /// <summary>
        /// Create a new route
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(RouteDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RouteDto>> CreateRoute([FromBody] CreateRouteDto createDto)
        {
            try
            {
                var result = await _routeService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetRouteById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating route");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the route");
            }
        }

        /// <summary>
        /// Update an existing route
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateRouteDto updateDto)
        {
            try
            {
                var result = await _routeService.UpdateAsync(id, updateDto);
                if (result == null)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating route with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the route with ID: {id}");
            }
        }

        /// <summary>
        /// Delete a route
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoute(Guid id)
        {
            try
            {
                var result = await _routeService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting route with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting the route with ID: {id}");
            }
        }
    }
}
