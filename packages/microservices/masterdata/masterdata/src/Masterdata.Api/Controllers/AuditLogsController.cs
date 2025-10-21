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
    /// Get a paginated list of audit logs with filtering options
    /// </summary>
    /// <param name="filter">Filter criteria for audit logs</param>
    /// <returns>A paginated list of audit logs</returns>
    /// <response code="200">Returns the paginated list of audit logs</response>
    /// <response code="500">If there was an error retrieving the audit logs</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), 200)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
    {
        try
        {
            _logger.LogInformation("Getting audit logs with filter: {Filter}", filter);
            
            // Ensure page size is reasonable
            filter.PageSize = Math.Clamp(filter.PageSize, 1, 100);
            filter.PageNumber = Math.Max(1, filter.PageNumber);

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

