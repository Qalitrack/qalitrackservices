using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.DTOs.Report;
using UserService.Core.Interfaces;

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
        [ProducesResponseType(typeof(ShiftReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShiftReportResponse>> GetShiftReport()
        {
            _logger.LogInformation("Generating shift report");
            var report = await _reportService.GenerateShiftReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Generates a comprehensive user report
        /// </summary>
        /// <returns>User report with statistics</returns>
        [HttpGet("users")]
        [Authorize(Policy = "reports.view")]
        [ProducesResponseType(typeof(UserReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserReportResponse>> GetUserReport()
        {
            _logger.LogInformation("Generating user report");
            var report = await _reportService.GenerateUserReportAsync();
            return Ok(report);
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
            _logger.LogInformation("Generating shift details report for shift {ShiftId}", shiftId);
            var report = await _reportService.GetShiftDetailsReportAsync(shiftId);
            
            if (report == null)
            {
                return NotFound($"Shift with ID {shiftId} not found");
            }

            return Ok(report);
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
            _logger.LogInformation("Generating user details report for user {UserId}", userId);
            var report = await _reportService.GetUserDetailsReportAsync(userId);
            
            if (report == null)
            {
                return NotFound($"User with ID {userId} not found");
            }

            return Ok(report);
        }
    }
}
