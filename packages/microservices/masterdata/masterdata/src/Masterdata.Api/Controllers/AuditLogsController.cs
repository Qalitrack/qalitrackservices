using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admins can access audit logs
    [Produces("application/json")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditLogsController> _logger;

        public AuditLogsController(
            IAuditLogService auditLogService,
            ILogger<AuditLogsController> logger)
        {
            _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get a paginated list of audit logs
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AuditLogDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
            [FromQuery] string? entityName = null,
            [FromQuery] string? entityId = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                // Ensure page size is reasonable
                pageSize = Math.Clamp(pageSize, 1, 100);
                pageNumber = Math.Max(1, pageNumber);

                var result = await _auditLogService.GetAuditLogsAsync(
                    entityName,
                    entityId,
                    action,
                    userId,
                    startDate,
                    endDate,
                    pageNumber,
                    pageSize
                );

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, "An error occurred while retrieving audit logs");
            }
        }

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuditLogDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<AuditLogDto>> GetAuditLog(string id)
        {
            try
            {
                var auditLog = await _auditLogService.GetAuditLogByIdAsync(id);
                if (auditLog == null)
                {
                    return NotFound();
                }
                return Ok(auditLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit log with ID: {AuditLogId}", id);
                return StatusCode(500, "An error occurred while retrieving the audit log");
            }
        }
    }
}
