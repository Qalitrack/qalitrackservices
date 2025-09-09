using System.Linq.Expressions;

namespace QaliTrack.DataManager.Core.Common;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<T> Items, long TotalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default);
}

public interface ITenantRepository<T> : IRepository<T> where T : TenantEntity
{
    Task<IEnumerable<T>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<T> Items, long TotalCount)> GetPagedByOrganizationAsync(
        Guid organizationId,
        int page, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default);
}