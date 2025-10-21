using Masterdata.Core.DTOs.Vehicles;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masterdata.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class VehiclesController : BaseController
    {
        private readonly IVehicleService _vehicleService;
        private readonly ILogger<VehiclesController> _logger;

        public VehiclesController(
            IVehicleService vehicleService,
            ILogger<VehiclesController> logger)
        {
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<VehicleReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetVehicles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _vehicleService.GetPagedVehiclesAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting vehicles");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VehicleReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVehicle(string id)
        {
            try
            {
                var vehicle = await _vehicleService.GetByIdAsync(id);
                if (vehicle == null)
                {
                    return NotFound();
                }
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the vehicle");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(VehicleReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if registration number is available
                if (!await _vehicleService.IsRegistrationNumberAvailableAsync(dto.RegistrationNumber))
                {
                    ModelState.AddModelError(nameof(dto.RegistrationNumber), "Registration number is already in use");
                    return BadRequest(ModelState);
                }

                var result = await _vehicleService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetVehicle), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the vehicle");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(VehicleReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVehicle(string id, [FromBody] UpdateVehicleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if registration number is available (if it's being updated)
                if (!string.IsNullOrEmpty(dto.RegistrationNumber))
                {
                    if (!await _vehicleService.IsRegistrationNumberAvailableAsync(dto.RegistrationNumber, id))
                    {
                        ModelState.AddModelError(nameof(dto.RegistrationNumber), "Registration number is already in use");
                        return BadRequest(ModelState);
                    }
                }

                var result = await _vehicleService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating vehicle with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the vehicle");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVehicle(string id)
        {
            try
            {
                var success = await _vehicleService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting vehicle with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the vehicle");
            }
        }
        

        [HttpGet("{id}/drivers")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAssignedDrivers(string id)
        {
            try
            {
                var drivers = await _vehicleService.GetAssignedDriversAsync(id);
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting drivers for vehicle with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the assigned drivers");
            }
        }

        [HttpPost("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateStatusRequest request)
        {
            try
            {
                var success = await _vehicleService.UpdateStatusAsync(id, request.Status);
                if (!success)
                {
                    return NotFound();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating status for vehicle with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the vehicle status");
            }
        }
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; } = null!;
    }
}
