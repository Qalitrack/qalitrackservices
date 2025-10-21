using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Axle;
using Masterdata.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.Api.Controllers
{
    /// <summary>
    /// API controller for managing axle configurations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AxleConfigurationsController : BaseController
    {
        private readonly IAxleConfigurationService _axleConfigService;
        private readonly ILogger<AxleConfigurationsController> _logger;

        public AxleConfigurationsController(
            IAxleConfigurationService axleConfigService,
            ILogger<AxleConfigurationsController> logger)
        {
            _axleConfigService = axleConfigService ?? throw new ArgumentNullException(nameof(axleConfigService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a paginated list of axle configurations
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10)</param>
        /// <returns>Paginated list of axle configurations</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAxleConfigurations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting axle configurations. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);
                var result = await _axleConfigService.GetAxleConfigurationsAsync(pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting axle configurations");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving axle configurations");
            }
        }

        /// <summary>
        /// Get an axle configuration by ID
        /// </summary>
        /// <param name="id">The ID of the axle configuration</param>
        /// <returns>The requested axle configuration</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAxleConfiguration(string id)
        {
            try
            {
                _logger.LogInformation("Getting axle configuration with ID: {Id}", id);
                var result = await _axleConfigService.GetAxleConfigurationByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound($"Axle configuration with ID {id} not found");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting axle configuration with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving axle configuration with ID {id}");
            }
        }

        /// <summary>
        /// Create a new axle configuration
        /// </summary>
        /// <param name="dto">The axle configuration data</param>
        /// <returns>The created axle configuration</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAxleConfiguration([FromBody] CreateAxleConfigurationDto dto)
        {
            try
            {
                _logger.LogInformation("Creating new axle configuration with code: {Code}", dto.Code);
                var result = await _axleConfigService.CreateAxleConfigurationAsync(dto);
                return Created($"api/axleconfigurations/{result.Id}", result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating axle configuration: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating axle configuration");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the axle configuration");
            }
        }

        /// <summary>
        /// Update an existing axle configuration
        /// </summary>
        /// <param name="id">The ID of the axle configuration to update</param>
        /// <param name="dto">The updated axle configuration data</param>
        /// <returns>The updated axle configuration</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAxleConfiguration(string id, [FromBody] UpdateAxleConfigurationDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID in the URL does not match ID in the request body");
            }

            try
            {
                _logger.LogInformation("Updating axle configuration with ID: {Id}", id);
                var result = await _axleConfigService.UpdateAxleConfigurationAsync(dto);
                
                if (result == null)
                {
                    return NotFound($"Axle configuration with ID {id} not found");
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating axle configuration: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating axle configuration with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating axle configuration with ID {id}");
            }
        }

        /// <summary>
        /// Delete an axle configuration
        /// </summary>
        /// <param name="id">The ID of the axle configuration to delete</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAxleConfiguration(string id)
        {
            try
            {
                _logger.LogInformation("Deleting axle configuration with ID: {Id}", id);
                var success = await _axleConfigService.DeleteAxleConfigurationAsync(id);
                
                if (!success)
                {
                    return NotFound($"Axle configuration with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting axle configuration with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while deleting axle configuration with ID {id}");
            }
        }

        /// <summary>
        /// Toggle the active status of an axle configuration
        /// </summary>
        /// <param name="id">The ID of the axle configuration</param>
        /// <param name="isActive">The new active status</param>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ToggleAxleConfigurationStatus(string id, [FromBody] bool isActive)
        {
            try
            {
                _logger.LogInformation("Updating status of axle configuration with ID: {Id} to {Status}", id, isActive ? "Active" : "Inactive");
                var success = await _axleConfigService.ToggleAxleConfigurationStatusAsync(id, isActive);
                
                if (!success)
                {
                    return NotFound($"Axle configuration with ID {id} not found");
                }

                return Ok(new { message = $"Axle configuration status updated to {(isActive ? "Active" : "Inactive")}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for axle configuration with ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating status for axle configuration with ID {id}");
            }
        }
    }
}
