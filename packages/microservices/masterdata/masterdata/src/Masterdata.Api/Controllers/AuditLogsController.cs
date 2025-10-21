using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Masterdata.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
    /// Get audit logs with filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogReadDto>), 200)]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Getting audit logs with filter: {Filter}", filter);
            
            var result = await _auditLogService.GetAuditLogsAsync(
                filter.EntityName,
                filter.EntityId,
                filter.Action,
                filter.UserId,
                filter.StartDate,
                filter.EndDate,
                filter.PageNumber,
                filter.PageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, new { message = "An error occurred while retrieving audit logs", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific audit log by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AuditLogReadDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAuditLogById(string id)
    {
        try
        {
            _logger.LogInformation("Getting audit log with ID: {Id}", id);
            
            var result = await _auditLogService.GetAuditLogByIdAsync(id);
            if (result == null)
            {
                _logger.LogWarning("Audit log with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit log with ID: {Id}", id);
            return StatusCode(500, new { message = $"An error occurred while retrieving audit log with ID: {id}", error = ex.Message });
        }
    }

    /// <summary>
    /// Get audit logs for a specific entity
    /// </summary>
    [HttpGet("entity/{entityName}/{entityId}")]
    [ProducesResponseType(typeof(PagedResult<AuditLogReadDto>), 200)]
    public async Task<IActionResult> GetAuditLogsForEntity(
        string entityName, 
        string entityId, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting audit logs for entity {EntityName} with ID: {EntityId}", entityName, entityId);
            
            var result = await _auditLogService.GetAuditLogsAsync(
                entityName,
                entityId,
                pageNumber: pageNumber,
                pageSize: pageSize);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for entity {EntityName} with ID: {EntityId}", entityName, entityId);
            return StatusCode(500, new { message = $"An error occurred while retrieving audit logs for {entityName} with ID: {entityId}", error = ex.Message });
        }
    }
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

