using System;
using System.Threading.Tasks;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

/// <summary>
/// Service for retrieving audit logs
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Gets a paginated list of audit logs with optional filtering
    /// </summary>
    /// <param name="entityName">Optional. Filter by entity name</param>
    /// <param name="entityId">Optional. Filter by entity ID</param>
    /// <param name="action">Optional. Filter by action type (Create/Update/Delete)</param>
    /// <param name="userId">Optional. Filter by user ID</param>
    /// <param name="startDate">Optional. Filter by start date</param>
    /// <param name="endDate">Optional. Filter by end date</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 50, max: 100)</param>
    /// <returns>Paginated list of audit logs</returns>
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? entityName = null,
        string? entityId = null,
        string? action = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50);

    /// <summary>
    /// Gets a single audit log by its ID
    /// </summary>
    /// <param name="id">The ID of the audit log to retrieve</param>
    /// <returns>The audit log if found, otherwise null</returns>
    Task<AuditLogDto?> GetAuditLogByIdAsync(string id);
}
