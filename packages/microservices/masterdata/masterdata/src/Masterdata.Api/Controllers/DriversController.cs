using Masterdata.Core.DTOs.Drivers;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Masterdata.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DriversController : BaseController
    {
        private readonly IDriverService _driverService;
        private readonly ILogger<DriversController> _logger;

        public DriversController(
            IDriverService driverService,
            ILogger<DriversController> logger)
        {
            _driverService = driverService ?? throw new ArgumentNullException(nameof(driverService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DriverReadDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDrivers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _driverService.GetPagedDriversAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting drivers");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting drivers");
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DriverReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDriver(string id)
        {
            try
            {
                var driver = await _driverService.GetByIdAsync(id);
                if (driver == null)
                {
                    return NotFound();
                }
                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the driver");
            }
        }

        /// <summary>
        /// Gets a driver by their NFC code
        /// </summary>
        /// <param name="nfcCode">The NFC code to search for</param>
        /// <returns>Driver information if found</returns>
        [HttpGet("nfc/{nfcCode}")]
        [ProducesResponseType(typeof(DriverReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDriverByNfcCode(string nfcCode)
        {
            try
            {
                var driver = await _driverService.GetByNfcCodeAsync(nfcCode);
                if (driver == null)
                {
                    return NotFound(new { message = "Driver not found with the provided NFC code" });
                }
                return Ok(driver);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting driver with NFC code: {nfcCode}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting the driver");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(DriverReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDriver([FromBody] CreateDriverDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _driverService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetDriver), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating driver");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the driver");
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DriverReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDriver(string id, [FromBody] UpdateDriverDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _driverService.UpdateAsync(id, dto);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Validation error updating driver with ID: {id}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating driver with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the driver");
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDriver(string id)
        {
            try
            {
                var success = await _driverService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting driver with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the driver");
            }
        }

        [HttpPost("{driverId}/vehicles/{vehicleId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignVehicle(string driverId, string vehicleId)
        {
            try
            {
                await _driverService.AssignVehicleAsync(driverId, vehicleId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid arguments when assigning vehicle {vehicleId} to driver {driverId}");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Failed to assign vehicle {vehicleId} to driver {driverId}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning vehicle {vehicleId} to driver {driverId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while assigning the vehicle");
            }
        }

        [HttpDelete("{driverId}/vehicles/{vehicleId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveVehicle(string driverId, string vehicleId)
        {
            try
            {
                var success = await _driverService.RemoveVehicleAsync(driverId, vehicleId);
                if (!success)
                {
                    return BadRequest("Unable to remove vehicle from driver");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing vehicle {vehicleId} from driver {driverId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the vehicle");
            }
        }

        /// <summary>
        /// Assigns a driver to a supplier
        /// </summary>
        /// <param name="driverId">ID of the driver</param>
        /// <param name="supplierId">ID of the supplier</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{driverId}/suppliers/{supplierId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignToSupplier(string driverId, string supplierId)
        {
            try
            {
                var result = await _driverService.AssignToSupplierAsync(driverId, supplierId);
                if (!result)
                {
                    _logger.LogWarning("Failed to assign driver {DriverId} to supplier {SupplierId}", driverId, supplierId);
                    return NotFound("Driver or supplier not found or deleted");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict assigning driver {DriverId} to supplier {SupplierId}", driverId, supplierId);
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Removes a driver from a supplier
        /// </summary>
        /// <param name="driverId">ID of the driver</param>
        /// <param name="supplierId">ID of the supplier</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{driverId}/suppliers/{supplierId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromSupplier(string driverId, string supplierId)
        {
            var result = await _driverService.RemoveFromSupplierAsync(driverId, supplierId);
            if (!result)
            {
                _logger.LogWarning("Failed to remove driver {DriverId} from supplier {SupplierId}", driverId, supplierId);
                return NotFound("Driver or supplier not found or deleted");
            }
            return NoContent();
        }
        
        /// <summary>
        /// Assigns a driver to a transporter
        /// </summary>
        /// <param name="driverId">ID of the driver</param>
        /// <param name="transporterId">ID of the transporter</param>
        /// <returns>No content if successful</returns>
        [HttpPost("{driverId}/transporters/{transporterId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> AssignToTransporter(string driverId, string transporterId)
        {
            try
            {
                var result = await _driverService.AssignToTransporterAsync(driverId, transporterId);
                if (!result)
                {
                    _logger.LogWarning("Failed to assign driver {DriverId} to transporter {TransporterId}", driverId, transporterId);
                    return NotFound("Driver or transporter not found or deleted");
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict assigning driver {DriverId} to transporter {TransporterId}", driverId, transporterId);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning driver {DriverId} to transporter {TransporterId}", driverId, transporterId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while assigning the driver to transporter");
            }
        }

        /// <summary>
        /// Removes a driver from a transporter
        /// </summary>
        /// <param name="driverId">ID of the driver</param>
        /// <param name="transporterId">ID of the transporter</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{driverId}/transporters/{transporterId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveFromTransporter(string driverId, string transporterId)
        {
            try
            {
                var result = await _driverService.RemoveFromTransporterAsync(driverId, transporterId);
                if (!result)
                {
                    _logger.LogWarning("Failed to remove driver {DriverId} from transporter {TransporterId}", driverId, transporterId);
                    return NotFound("Driver or transporter not found or deleted");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing driver {DriverId} from transporter {TransporterId}", driverId, transporterId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the driver from transporter");
            }
        }
    }
}