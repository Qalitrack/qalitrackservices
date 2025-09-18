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
        /// Get paginated shift instances with attendance summaries and detailed records
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <returns>Paginated shift instances with attendance data</returns>
        [HttpGet("paginated-instances")]
        [ProducesResponseType(typeof(PaginatedShiftInstancesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPaginatedShiftInstances(int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                // Validate parameters
                if (pageNumber < 1)
                {
                    return BadRequest("Page number must be greater than 0");
                }

                pageSize = Math.Min(Math.Max(pageSize, 1), 100);

                var result = await _shiftAttendanceService.GetPaginatedShiftInstancesWithAttendanceAsync(
                    pageNumber, pageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated shift instances with attendance");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving shift instances");
            }
        }
        /// <summary>
        /// Get paginated attendance details for a specific shift instance
        /// </summary>
        /// <param name="instanceId">The ID of the shift instance</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 50, max: 100)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Paginated attendance details for the specified shift instance</returns>
        [HttpGet("instance/{instanceId}/attendance")]
        [ProducesResponseType(typeof(PagedResult<ShiftAttendanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetShiftInstanceAttendance(
            Guid instanceId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (instanceId == Guid.Empty)
                {
                    return BadRequest("Instance ID is required");
                }

                // Validate pagination
                pageNumber = Math.Max(1, pageNumber);
                pageSize = Math.Clamp(pageSize, 1, 100); // Enforce max page size

                var result = await _shiftAttendanceService.GetShiftInstanceAttendanceAsync(
                    instanceId,
                    pageNumber,
                    pageSize,
                    cancellationToken);

                if (result == null || !result.Items.Any())
                {
                    return NotFound($"No attendance records found for shift instance with ID {instanceId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for shift instance {InstanceId}", instanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving shift instance attendance");
            }
        }

        /// <summary>
        /// Get detailed attendance information by ID (DTO)
        /// </summary>
        /// <param name="id">Shift attendance ID</param>
        /// <returns>Detailed shift attendance response</returns>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(ShiftAttendanceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceDetails(string id)
        {
            try
            {
                var attendanceDetails = await _shiftAttendanceService.GetAttendanceDetailsAsync(id);
                if (attendanceDetails == null)
                {
                    return NotFound();
                }

                return Ok(attendanceDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance details for ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the attendance details");
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
                _logger.LogError(ex, "Error deleting shift attendance with ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while deleting the shift attendance");
            }
        }


        /// <summary>
        /// Clock out an employee for a shift instance
        /// </summary>
        /// <param name="clockOutRequest">Clock out request</param>
        /// <returns>Updated attendance record</returns>
        [HttpPost("clock-out")]
        [ProducesResponseType(typeof(ShiftAttendance), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClockOut([FromBody] ClockOutRequest clockOutRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _shiftAttendanceService.ClockOutAsync(
                    clockOutRequest.ShiftInstanceId,
                    clockOutRequest.EmployeeId,
                    clockOutRequest.ClockOutTime,
                    clockOutRequest.Notes);

                if (result == null)
                {
                    return BadRequest("Unable to clock out. Please ensure the employee is clocked in.");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during clock out for employee {EmployeeId} and shift instance {ShiftInstanceId}",
                    clockOutRequest?.EmployeeId, clockOutRequest?.ShiftInstanceId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while clocking out");
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

        /// <summary>
        /// Get attendance by shift instance and employee
        /// </summary>
        /// <param name="shiftInstanceId">Shift instance ID</param>
        /// <param name="employeeId">Employee ID</param>
        [HttpGet("shift-instance/{shiftInstanceId}/employee/{employeeId}")]
        [ProducesResponseType(typeof(ShiftAttendance), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByShiftInstanceAndEmployee(string shiftInstanceId, string employeeId)
        {
            try
            {
                var attendance = await _shiftAttendanceService.GetByShiftInstanceAndEmployeeAsync(shiftInstanceId, employeeId);
                if (attendance == null)
                {
                    return NotFound();
                }

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error retrieving attendance for shift instance {ShiftInstanceId} and employee {EmployeeId}",
                    shiftInstanceId, employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while retrieving the attendance");
            }
        }
    }
    

    public class ClockOutRequest
    {
        public string ShiftInstanceId { get; set; } = string.Empty;
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime ClockOutTime { get; set; }
        public string? Notes { get; set; }
    }
}