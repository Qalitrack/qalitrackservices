using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Masterdata.Core.DTOs.Sacco;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class SaccosController : ControllerBase
    {
        private readonly ISaccoService _saccoService;
        private readonly ILogger<SaccosController> _logger;

        public SaccosController(
            ISaccoService saccoService,
            ILogger<SaccosController> logger)
        {
            _saccoService = saccoService ?? throw new ArgumentNullException(nameof(saccoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a paginated list of saccos
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="searchTerm">Optional search term to filter saccos</param>
        /// <returns>A paginated list of saccos</returns>
        /// <response code="200">Returns the paginated list of saccos</response>
        /// <response code="500">If there was an error retrieving the saccos</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<SaccoDto>>> GetSaccos(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                // Ensure page size is reasonable
                pageSize = Math.Min(Math.Max(1, pageSize), 100);
                pageNumber = Math.Max(1, pageNumber);

                var result = await _saccoService.GetPagedSaccosAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving saccos. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving saccos");
            }
        }

        /// <summary>
        /// Get a sacco by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSacco(Guid id)
        {
            try
            {
                var sacco = await _saccoService.GetByIdAsync(id);
                if (sacco == null)
                {
                    return NotFound();
                }
                return Ok(sacco);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sacco with ID: {SaccoId}", id);
                return StatusCode(500, "An error occurred while retrieving the sacco");
            }
        }

        /// <summary>
        /// Create a new sacco
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSacco([FromBody] CreateSaccoDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _saccoService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetSacco), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sacco");
                return StatusCode(500, "An error occurred while creating the sacco");
            }
        }

        /// <summary>
        /// Update an existing sacco
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSacco(Guid id, [FromBody] UpdateSaccoDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _saccoService.UpdateAsync(id, updateDto);
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
                _logger.LogError(ex, "Error updating sacco with ID: {SaccoId}", id);
                return StatusCode(500, "An error occurred while updating the sacco");
            }
        }

        /// <summary>
        /// Delete a sacco (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSacco(Guid id)
        {
            try
            {
                var success = await _saccoService.DeleteAsync(id);
                if (!success)
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
                _logger.LogError(ex, "Error deleting sacco with ID: {SaccoId}", id);
                return StatusCode(500, "An error occurred while deleting the sacco");
            }
        }

        /// <summary>
        /// Deactivate a sacco
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateSacco(Guid id)
        {
            try
            {
                var success = await _saccoService.DeactivateAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating sacco with ID: {SaccoId}", id);
                return StatusCode(500, "An error occurred while deactivating the sacco");
            }
        }

        /// <summary>
        /// Activate a sacco
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateSacco(Guid id)
        {
            try
            {
                var success = await _saccoService.ActivateAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating sacco with ID: {SaccoId}", id);
                return StatusCode(500, "An error occurred while activating the sacco");
            }
        }

        #region Affiliations

        /// <summary>
        /// Get paginated list of affiliations for a sacco
        /// </summary>
        [HttpGet("{saccoId}/affiliations")]
        public async Task<IActionResult> GetAffiliations(
            Guid saccoId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _saccoService.GetAffiliatesAsync(saccoId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving affiliations for sacco ID: {SaccoId}", saccoId);
                return StatusCode(500, "An error occurred while retrieving affiliations");
            }
        }

        /// <summary>
        /// Get a specific affiliation by ID
        /// </summary>
        [HttpGet("affiliations/{affiliationId}")]
        public async Task<IActionResult> GetAffiliation(Guid affiliationId)
        {
            try
            {
                var affiliation = await _saccoService.GetAffiliationByIdAsync(affiliationId);
                if (affiliation == null)
                {
                    return NotFound();
                }
                return Ok(affiliation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving affiliation with ID: {AffiliationId}", affiliationId);
                return StatusCode(500, "An error occurred while retrieving the affiliation");
            }
        }

        /// <summary>
        /// Add a new affiliation to a sacco
        /// </summary>
        [HttpPost("{saccoId}/affiliations")]
        public async Task<IActionResult> AddAffiliation(
            Guid saccoId,
            [FromBody] CreateAffiliationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _saccoService.AddAffiliationAsync(saccoId, createDto);
                return CreatedAtAction(nameof(GetAffiliation), new { affiliationId = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding affiliation to sacco ID: {SaccoId}", saccoId);
                return StatusCode(500, "An error occurred while adding the affiliation");
            }
        }

        /// <summary>
        /// Remove an affiliation from a sacco
        /// </summary>
        [HttpDelete("affiliations/{affiliationId}")]
        public async Task<IActionResult> RemoveAffiliation(Guid affiliationId)
        {
            try
            {
                var success = await _saccoService.RemoveAffiliationAsync(affiliationId);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing affiliation with ID: {AffiliationId}", affiliationId);
                return StatusCode(500, "An error occurred while removing the affiliation");
            }
        }

        /// <summary>
        /// Update status of all affiliations for a sacco
        /// </summary>
        [HttpPut("{saccoId}/affiliations/status")]
        public async Task<IActionResult> UpdateAffiliationStatus(Guid saccoId, [FromBody] string status)
        {
            try
            {
                var success = await _saccoService.UpdateAffiliationStatusAsync(saccoId, status);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating affiliation status for sacco ID: {SaccoId}", saccoId);
                return StatusCode(500, "An error occurred while updating the affiliation status");
            }
        }

        #endregion
    }
}
