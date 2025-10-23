using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Organisation;
using Masterdata.Core.DTOs.Affiliation;
using Masterdata.Core.DTOs.Shared;
using Masterdata.Core.Interfaces;

namespace Masterdata.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    [Produces("application/json")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;
        private readonly ILogger<OrganisationsController> _logger;

        public OrganisationsController(
            IOrganisationService organisationService,
            ILogger<OrganisationsController> logger)
        {
            _organisationService = organisationService ?? throw new ArgumentNullException(nameof(organisationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a paginated list of organisations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<OrganisationDto>>> GetOrganisations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null)
        {
            try
            {
                var result = await _organisationService.GetPagedOrganisationsAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organisations");
                return StatusCode(500, "An error occurred while retrieving organisations");
            }
        }

        /// <summary>
        /// Get an organisation by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganisationDto>> GetOrganisation(Guid id)
        {
            try
            {
                var organisation = await _organisationService.GetByIdAsync(id);
                if (organisation == null)
                {
                    return NotFound();
                }
                return Ok(organisation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organisation with ID: {OrganisationId}", id);
                return StatusCode(500, "An error occurred while retrieving the organisation");
            }
        }

        /// <summary>
        /// Create a new organisation
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrganisationDto>> CreateOrganisation([FromBody] CreateOrganisationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _organisationService.CreateAsync(createDto);
                return CreatedAtAction(nameof(GetOrganisation), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organisation");
                return StatusCode(500, "An error occurred while creating the organisation");
            }
        }

        /// <summary>
        /// Update an existing organisation
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganisation(Guid id, [FromBody] UpdateOrganisationDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _organisationService.UpdateAsync(id, updateDto);
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
                _logger.LogError(ex, "Error updating organisation with ID: {OrganisationId}", id);
                return StatusCode(500, "An error occurred while updating the organisation");
            }
        }

        /// <summary>
        /// Delete an organisation (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganisation(Guid id)
        {
            try
            {
                var success = await _organisationService.DeleteAsync(id);
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
                _logger.LogError(ex, "Error deleting organisation with ID: {OrganisationId}", id);
                return StatusCode(500, "An error occurred while deleting the organisation");
            }
        }

        /// <summary>
        /// Deactivate an organisation
        /// </summary>
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateOrganisation(Guid id)
        {
            try
            {
                var success = await _organisationService.DeactivateAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating organisation with ID: {OrganisationId}", id);
                return StatusCode(500, "An error occurred while deactivating the organisation");
            }
        }

        /// <summary>
        /// Activate an organisation
        /// </summary>
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateOrganisation(Guid id)
        {
            try
            {
                var success = await _organisationService.ActivateAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating organisation with ID: {OrganisationId}", id);
                return StatusCode(500, "An error occurred while activating the organisation");
            }
        }

        #region Affiliations

        /// <summary>
        /// Get paginated list of affiliations for an organisation
        /// </summary>
        [HttpGet("{organisationId}/affiliations")]
        public async Task<ActionResult<PagedResult<AffiliationDto>>> GetAffiliations(
            Guid organisationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _organisationService.GetAffiliatesAsync(organisationId, pageNumber, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving affiliations for organisation ID: {OrganisationId}", organisationId);
                return StatusCode(500, "An error occurred while retrieving affiliations");
            }
        }

        /// <summary>
        /// Get a specific affiliation by ID
        /// </summary>
        [HttpGet("affiliations/{affiliationId}")]
        public async Task<ActionResult<AffiliationDto>> GetAffiliation(Guid affiliationId)
        {
            try
            {
                var affiliation = await _organisationService.GetAffiliationByIdAsync(affiliationId);
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
        /// Add a new affiliation to an organisation
        /// </summary>
        [HttpPost("{organisationId}/affiliations")]
        public async Task<ActionResult<AffiliationDto>> AddAffiliation(
            Guid organisationId,
            [FromBody] CreateAffiliationDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _organisationService.AddAffiliationAsync(organisationId, createDto);
                return CreatedAtAction(nameof(GetAffiliation), new { affiliationId = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding affiliation to organisation ID: {OrganisationId}", organisationId);
                return StatusCode(500, "An error occurred while adding the affiliation");
            }
        }

        /// <summary>
        /// Remove an affiliation from an organisation
        /// </summary>
        [HttpDelete("affiliations/{affiliationId}")]
        public async Task<IActionResult> RemoveAffiliation(Guid affiliationId)
        {
            try
            {
                var success = await _organisationService.RemoveAffiliationAsync(affiliationId);
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

        #endregion
    }
}
