using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Shift;
using System.ComponentModel.DataAnnotations;
using UserService.Core.Interfaces.Services;
using UserService.Core.DTOs.Common;

namespace UserService.Api.Controllers
{
    [Authorize]  // This ensures that the user is authenticated
    [ApiController]
    [Route("[controller]")]
    public class ShiftController : BaseController
    {
        private readonly IShiftService _shiftService;
        private readonly ILogger<ShiftController> _logger;

        public ShiftController(IShiftService shiftService, ILogger<ShiftController> logger)
        {
            _shiftService = shiftService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Policy = "shifts.view")]
        [ProducesResponseType(typeof(PagedResult<ShiftDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest("Page and pageSize must be positive integers.");
                }

                var paginationParameters = new PaginationParameters
                {
                    Page = page,
                    PageSize = pageSize
                };

                var shifts = await _shiftService.GetAllAsync(paginationParameters);
                return Ok(shifts);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Invalid pagination parameters provided");
                return BadRequest("Pagination parameters are required");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all shifts");
                return StatusCode(500, "An error occurred while retrieving shifts");
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "shifts.view")]
        [ProducesResponseType(typeof(ShiftResponse), StatusCodes.Status200OK)] // Changed from ShiftDto to ShiftResponse
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var shift = await _shiftService.GetByIdAsync(id); // This returns ShiftResponse
                return Ok(shift);
            }
            catch (Exception ex) when (ex.Message == "Shift not found")
            {
                _logger.LogWarning("Attempt to get non-existent shift with ID: {ShiftId}", id);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Shift with ID {id} not found", 
                    Errors = (string[])null, 
                    StatusCode = 404 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting shift with ID: {id}");
                return StatusCode(500, "An error occurred while retrieving the shift");
            }
        }

        [HttpPost]
        [Authorize(Policy = "shifts.manage")]  // Permission-based policy check
        [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateShiftRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var shift = await _shiftService.CreateEnhancedAsync(dto);
                _logger.LogInformation("User created shift {ShiftId}", shift.Id);
                return CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error creating shift");
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23505" => pgEx.ConstraintName switch
                    {
                        "IX_Shifts_Name" => "A shift with this name already exists. Please choose a different name.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error creating shift: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating shift");
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while creating the shift", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "shifts.manage")]
        [ProducesResponseType(typeof(ShiftDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateShiftRequest dto)
        {
            try
            {
                
                // Just pass the DTO directly - your service handles null checks
                var shift = await _shiftService.UpdateEnhancedAsync(id, dto);
                
                if (shift == null)
                {
                    _logger.LogWarning("Controller: Service returned null for shift {ShiftId}", id);
                    return NotFound(new { 
                        Success = false, 
                        Message = $"Shift with ID {id} not found", 
                        Errors = (string[])null, 
                        StatusCode = 404 
                    });
                }
                return Ok(shift);
            }
            catch (Exception ex) when (ex.Message == "Shift not found")
            {
                _logger.LogWarning("Attempt to update non-existent shift with ID: {ShiftId}", id);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Shift with ID {id} not found", 
                    Errors = (string[])null, 
                    StatusCode = 404 
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error updating shift with ID: {ShiftId}", id);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                string errorMessage = pgEx.SqlState switch
                {
                    "23505" => pgEx.ConstraintName switch
                    {
                        "IX_Shifts_Name" => "A shift with this name already exists. Please choose a different name.",
                        _ => $"Duplicate entry detected: {pgEx.ConstraintName}"
                    },
                    "23503" => "Referenced record does not exist",
                    "23514" => "Data validation failed - check constraint violation",
                    _ => $"Database error: {pgEx.MessageText}"
                };

                _logger.LogWarning(ex, "Database constraint error updating shift: {ErrorMessage}", errorMessage);
                return BadRequest(new { 
                    Success = false, 
                    Message = errorMessage, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shift with ID: {ShiftId}", id);
                return BadRequest(new { 
                    Success = false, 
                    Message = "An error occurred while updating the shift", 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "shifts.manage")]  // Permission-based policy check
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _shiftService.DeleteAsync(id);
                return Ok(new { message = "Shift deleted successfully" });
            }
            catch (Exception ex) when (ex.Message == "Shift not found")
            {
                _logger.LogWarning("Attempt to delete non-existent shift with ID: {ShiftId}", id);
                return NotFound(new { 
                    Success = false, 
                    Message = $"Shift with ID {id} not found", 
                    Errors = (string[])null, 
                    StatusCode = 404 
                });
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning(ex, "Validation error deleting shift with ID: {ShiftId}", id);
                return BadRequest(new { 
                    Success = false, 
                    Message = ex.Message, 
                    Errors = (string[])null, 
                    StatusCode = 400 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shift with ID: {ShiftId}", id);
                return StatusCode(500, "An error occurred while deleting the shift");
            }
        }

        [HttpGet("deleted")]
        [Authorize(Policy = "shifts.view")]
        [ProducesResponseType(typeof(PagedResult<ShiftDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDeletedShifts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool sortDescending = false)
        {
            try
            {
                var parameters = new PaginationParameters
                {
                    Page = page,
                    PageSize = pageSize,
                    Search = search,
                    SortBy = sortBy,
                    SortDescending = sortDescending
                };

                var result = await _shiftService.GetDeletedPagedAsync(parameters);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted shifts");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { Success = false, Message = "An error occurred while retrieving deleted shifts" });
            }
        }
        
    }
}


