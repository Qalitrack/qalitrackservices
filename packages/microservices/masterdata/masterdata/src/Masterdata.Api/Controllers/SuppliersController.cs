using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.Supplier;
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
    public class SuppliersController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SuppliersController> _logger;

        public SuppliersController(
            ISupplierService supplierService,
            ILogger<SuppliersController> logger)
        {
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all suppliers with pagination and search
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<SupplierReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<SupplierReadDto>>> GetSuppliers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _supplierService.GetPagedSuppliersAsync(pageNumber, pageSize, searchTerm);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving suppliers");
            }
        }

        /// <summary>
        /// Get a specific supplier by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SupplierReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SupplierReadDto>> GetSupplier(string id)
        {
            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id);
                if (supplier == null)
                {
                    return NotFound();
                }
                return Ok(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier with ID: {SupplierId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the supplier");
            }
        }

        /// <summary>
        /// Create a new supplier
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SupplierReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SupplierReadDto>> CreateSupplier([FromBody] CreateSupplierDto dto)
        {
            try
            {
                var result = await _supplierService.CreateSupplierAsync(dto);
                return CreatedAtAction(nameof(GetSupplier), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating supplier");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the supplier");
            }
        }

        /// <summary>
        /// Update an existing supplier
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(string id, [FromBody] UpdateSupplierDto dto)
        {
            try
            {
                var exists = await _supplierService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound();
                }

                var success = await _supplierService.UpdateSupplierAsync(id, dto);
                if (!success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the supplier");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating supplier with ID: {SupplierId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the supplier");
            }
        }

        /// <summary>
        /// Delete a supplier
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(string id)
        {
            try
            {
                var exists = await _supplierService.ExistsAsync(id);
                if (!exists)
                {
                    return NotFound();
                }

                var success = await _supplierService.DeleteSupplierAsync(id);
                if (!success)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the supplier");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier with ID: {SupplierId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the supplier");
            }
        }
    }
}
