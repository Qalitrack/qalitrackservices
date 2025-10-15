using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Utils;
using Masterdata.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Masterdata.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly MasterdataDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenExtractionService _tokenExtractionService;

    public AuditLogRepository(
        MasterdataDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ITokenExtractionService tokenExtractionService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _tokenExtractionService = tokenExtractionService;
    }

    public async Task LogCreateAsync<T>(T entity) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityName = typeof(T).Name,
            EntityId = entity.Id,
            Action = "Create",
            NewValues = JsonSerializer.Serialize(entity),
            UserId = GetCurrentUserId(),
            UserName = GetCurrentUserName(),
            IpAddress = GetClientIpAddress(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task LogUpdateAsync<T>(T originalEntity, T updatedEntity) where T : BaseEntity
    {
        var originalValues = new Dictionary<string, object?>();
        var updatedValues = new Dictionary<string, object?>();
        var changedProperties = new List<string>();

        var properties = typeof(T).GetProperties();
        foreach (var property in properties)
        {
            var originalValue = property.GetValue(originalEntity);
            var updatedValue = property.GetValue(updatedEntity);

            if (!Equals(originalValue, updatedValue))
            {
                originalValues[property.Name] = originalValue;
                updatedValues[property.Name] = updatedValue;
                changedProperties.Add(property.Name);
            }
        }

        if (changedProperties.Any())
        {
            var auditLog = new AuditLog
            {
                EntityName = typeof(T).Name,
                EntityId = updatedEntity.Id,
                Action = "Update",
                OldValues = JsonSerializer.Serialize(originalValues),
                NewValues = JsonSerializer.Serialize(updatedValues),
                AffectedProperties = string.Join(", ", changedProperties),
                UserId = GetCurrentUserId(),
                UserName = GetCurrentUserName(),
                IpAddress = GetClientIpAddress(),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId()
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }

    public async Task LogDeleteAsync<T>(T entity) where T : BaseEntity
    {
        var auditLog = new AuditLog
        {
            EntityName = typeof(T).Name,
            EntityId = entity.Id,
            Action = "Delete",
            OldValues = JsonSerializer.Serialize(entity),
            UserId = GetCurrentUserId(),
            UserName = GetCurrentUserName(),
            IpAddress = GetClientIpAddress(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId()
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        if (pageNumber < 1)
            pageNumber = 1;

        if (pageSize < 1)
            pageSize = 10;

        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(e =>
                EF.Functions.Like(e.EntityName.ToLower(), $"%{searchTerm}%") ||
                EF.Functions.Like(e.EntityId.ToLower(), $"%{searchTerm}%") ||
                EF.Functions.Like(e.Action.ToLower(), $"%{searchTerm}%") ||
                EF.Functions.Like(e.UserName.ToLower(), $"%{searchTerm}%")
            );
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<AuditLog>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByActionAsync(string action)
    {
        return await _context.AuditLogs
            .Where(a => a.Action == action)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<AuditLog?> GetByIdAsync(string id)
    {
        return await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    private string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext != null ? 
            _tokenExtractionService.GetUserIdFromToken(_httpContextAccessor.HttpContext.User)?.ToString() : null;
    }

    private string? GetCurrentUserName()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }

    private string? GetClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }
}