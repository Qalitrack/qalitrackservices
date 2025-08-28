using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Report;
using UserService.Core.DTOs.Common;
using UserService.Core.Interfaces.Services;

namespace UserService.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IReportService reportService,
            ILogger<ReportsController> logger)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        [HttpGet("shifts")]
        [Authorize(Policy = "reports.view")]
        [ProducesResponseType(typeof(PagedResult<ShiftReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<ShiftReportDto>>> GetShiftReport([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var report = await _reportService.GenerateShiftReportAsync(parameters);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift report");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating the shift report");
            }
        }

        /// <summary>
        /// Generates a comprehensive user report
        /// </summary>
        /// <returns>User report with statistics</returns>
        [HttpGet("users")]
        [Authorize(Policy = "reports.view")]
        [ProducesResponseType(typeof(PagedResult<UserReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<UserReportDto>>> GetUserReport([FromQuery] PaginationParameters parameters)
        {
            try
            {
                var report = await _reportService.GenerateUserReportAsync(parameters);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating user report");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating the user report");
            }
        }

        [HttpGet("shifts/{shiftId}")]
        [Authorize(Policy = "reports.view")]
        [ProducesResponseType(typeof(ShiftReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShiftReportDto>> GetShiftDetails(string shiftId)
        {
            try
            {
                var report = await _reportService.GetShiftDetailsReportAsync(shiftId);
                
                if (report == null)
                {
                    return NotFound($"Shift with ID {shiftId} not found");
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shift details for {ShiftId}", shiftId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting shift details");
            }
        }

      
        [HttpGet("users/{userId}")]
        [Authorize(Policy = "reports.view")]
        [ProducesResponseType(typeof(UserReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserReportDto>> GetUserDetails(string userId)
        {
            try
            {
                var report = await _reportService.GetUserDetailsReportAsync(userId);
                
                if (report == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user details for {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while getting user details");
            }
        }
    }
}
