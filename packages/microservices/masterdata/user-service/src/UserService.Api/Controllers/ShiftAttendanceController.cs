using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Enums;
using UserService.Core.Interfaces.Services;
using UserService.Core.DTOs.Common;

namespace UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShiftAttendanceController : ControllerBase
    {
        private readonly IShiftAttendanceService _shiftAttendanceService;
        private readonly ILogger<ShiftAttendanceController> _logger;

        public ShiftAttendanceController(
            IShiftAttendanceService shiftAttendanceService,
            ILogger<ShiftAttendanceController> logger)
        {
            _shiftAttendanceService =
                shiftAttendanceService ?? throw new ArgumentNullException(nameof(shiftAttendanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all shift attendances with pagination
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
        /// <returns>Paginated list of shift attendances</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ShiftAttendanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 20)
        {
            try
            {
                // Validate page size to prevent excessive data retrieval
                pageSize = Math.Min(pageSize, 100);

                var attendances = await _shiftAttendanceService.GetAllAsync(pageNumber, pageSize);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shift attendances");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving shift attendances");
            }
        }

        /// <summary>
        /// Get shift attendance by ID
        /// </summary>
        /// <param name="id">Shift attendance ID</param>
        /// <returns>Shift attendance details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ShiftAttendanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var attendance = await _shiftAttendanceService.GetByIdAsync(id);
                if (attendance == null)
                {
                    return NotFound();
                }

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving shift attendance with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the shift attendance");
            }
        }

        /// <summary>
        /// Delete a shift attendance
        /// </summary>
        /// <param name="id">Shift attendance ID</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var success = await _shiftAttendanceService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting shift attendance with ID {id}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while deleting the shift attendance");
            }
        }
        

        /// <summary>
        /// Get attendances for a specific shift instance
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        [HttpGet("shift-instance/{shiftInstanceId}")]
        [ProducesResponseType(typeof(IEnumerable<ShiftAttendanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByShiftInstance(string shiftInstanceId)
        {
            try
            {
                var attendances = await _shiftAttendanceService.GetAttendanceByShiftInstanceAsync(shiftInstanceId);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendances for shift instance {ShiftInstanceId}",
                    shiftInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving attendances");
            }
        }

        /// <summary>
        /// Get attendances for a specific employee
        /// </summary>
        /// <param name="employeeId">Employee ID</param>
        [HttpGet("employee/{employeeId}")]
        [ProducesResponseType(typeof(IEnumerable<ShiftAttendanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByEmployee(string employeeId)
        {
            try
            {
                var attendances = await _shiftAttendanceService.GetEmployeeAttendanceAsync(employeeId);
                return Ok(attendances);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendances for employee {EmployeeId}", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving attendances");
            }
        }

        /// <summary>
        /// Get attendance for a specific user and shift instance
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        [HttpGet("user/{userId}/shift-instance/{shiftInstanceId}")]
        [ProducesResponseType(typeof(ShiftAttendance), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserAndInstance(string userId, string shiftInstanceId)
        {
            try
            {
                var attendance =
                    await _shiftAttendanceService.GetAttendanceByUserAndInstanceAsync(userId, shiftInstanceId);
                if (attendance == null)
                {
                    return NotFound();
                }

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving attendance for user {UserId} and shift instance {ShiftInstanceId}", userId,
                    shiftInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the attendance");
            }
        }
    }
}