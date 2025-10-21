using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Masterdata.Core.DTOs.AuditLog;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;

namespace Masterdata.Core.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IMapper _mapper;

    public AuditLogService(IAuditLogRepository auditLogRepository, IMapper mapper)
    {
        _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        string? entityName = null,
        string? entityId = null,
        string? action = null,
        string? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int pageNumber = 1,
        int pageSize = 50)
    {
        // Validate page size
        pageSize = Math.Clamp(pageSize, 1, 100);
        
        // Build the query
        var query = _auditLogRepository.GetAll();

        if (!string.IsNullOrEmpty(entityName))
        {
            query = query.Where(x => x.EntityName == entityName);
        }

        if (!string.IsNullOrEmpty(entityId))
        {
            query = query.Where(x => x.EntityId == entityId);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(x => x.Action == action);
        }

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(x => x.UserId == userId);
        }

        if (startDate.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Include the entire end date
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(x => x.CreatedAt <= endOfDay);
        }

        // Get total count before pagination
        var totalItems = query.Count();

        // Apply pagination
        var items = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Map to DTOs
        var itemDtos = _mapper.Map<List<AuditLogDto>>(items);

        return new PagedResult<AuditLogDto>
        {
            Items = itemDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(string id)
    {
        var auditLog = await _auditLogRepository.GetByIdAsync(id);
        return auditLog != null ? _mapper.Map<AuditLogDto>(auditLog) : null;
    }
}
