using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.DTOs.Transporters;
using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class TransportersController : ControllerBase
    {
        private readonly ITransporterService _transporterService;
        private readonly ILogger<TransportersController> _logger;

        public TransportersController(
            ITransporterService transporterService,
            ILogger<TransportersController> logger)
        {
            _transporterService = transporterService ?? throw new ArgumentNullException(nameof(transporterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all transporters with pagination and search
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<TransporterReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<TransporterReadDto>>> GetTransporters(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _transporterService.GetPagedTransportersAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transporters");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving transporters");
            }
        }

        /// <summary>
        /// Get a specific transporter by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransporterReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransporterReadDto>> GetTransporter(string id)
        {
            try
            {
                var transporter = await _transporterService.GetTransporterByIdAsync(id);
                if (transporter == null)
                {
                    return NotFound();
                }
                return Ok(transporter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transporter with ID: {TransporterId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the transporter");
            }
        }

        /// <summary>
        /// Create a new transporter
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TransporterReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TransporterReadDto>> CreateTransporter([FromBody] CreateTransporterDto dto)
        {
            try
            {
                var result = await _transporterService.CreateTransporterAsync(dto);
                return CreatedAtAction(nameof(GetTransporter), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating transporter");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the transporter");
            }
        }

        /// <summary>
        /// Update an existing transporter
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTransporter(string id, [FromBody] UpdateTransporterDto dto)
        {
            try
            {
                var exists = await _transporterService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound();
                }

                var success = await _transporterService.UpdateTransporterAsync(id, dto);
                if (!success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the transporter");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating transporter with ID: {TransporterId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the transporter");
            }
        }

        /// <summary>
        /// Delete a transporter
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTransporter(string id)
        {
            try
            {
                var exists = await _transporterService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound();
                }

                var success = await _transporterService.DeleteTransporterAsync(id);
                if (!success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the transporter");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transporter with ID: {TransporterId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the transporter");
            }
        }
        // Add these methods to TransportersController

        /// <summary>
        /// Get all vehicles associated with a specific transporter
        /// </summary>
        [HttpGet("{id}/vehicles")]
        [ProducesResponseType(typeof(IEnumerable<VehicleReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<VehicleReadDto>>> GetTransporterVehicles(string id)
        {
            try
            {
                var vehicles = await _transporterService.GetTransporterVehiclesAsync(id);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vehicles for transporter with ID: {TransporterId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the transporter's vehicles");
            }
        }

        /// <summary>
        /// Get all drivers associated with a specific transporter
        /// </summary>
        [HttpGet("{id}/drivers")]
        [ProducesResponseType(typeof(IEnumerable<DriverReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DriverReadDto>>> GetTransporterDrivers(string id)
        {
            try
            {
                var drivers = await _transporterService.GetTransporterDriversAsync(id);
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving drivers for transporter with ID: {TransporterId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the transporter's drivers");
            }
        }
    }
}
