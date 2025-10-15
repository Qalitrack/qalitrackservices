using Masterdata.Core.Entities;
using Masterdata.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Masterdata.Core.Interfaces;

public interface IAuditLogRepository
{
    Task LogCreateAsync<T>(T entity) where T : BaseEntity;
    Task LogUpdateAsync<T>(T originalEntity, T updatedEntity) where T : BaseEntity;
    Task LogDeleteAsync<T>(T entity) where T : BaseEntity;
    Task<PagedResult<AuditLog>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<List<AuditLog>> GetByEntityAsync(string entityName, string entityId);
    Task<List<AuditLog>> GetByActionAsync(string action);
    Task<AuditLog?> GetByIdAsync(string id);
}