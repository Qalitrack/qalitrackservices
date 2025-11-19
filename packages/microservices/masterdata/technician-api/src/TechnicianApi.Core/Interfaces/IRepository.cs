using System.Linq.Expressions;
using TechnicianApi.Core.Entities;

namespace TechnicianApi.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    // Basic CRUD
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);

    // Advanced queries
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    // Pagination
    Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool ascending = true);

    // Include related entities
    Task<T?> GetByIdWithIncludesAsync(string id, params Expression<Func<T, object>>[] includes);
}