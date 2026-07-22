using System.Linq.Expressions;
using Masterdata.Core.Entities;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Expression<Func<T, bool>>? searchPredicate = null,
        string[]? searchProperties = null,
        string sortBy = "CreatedAt",
        bool sortDescending = false);
    
    
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<string> ids);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<PagedResult<T>> GetDeletedPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
        
    Task<T?> GetByPredicateAsync(Expression<Func<T, bool>> predicate);
    Task<IEnumerable<T>> GetAllByPredicateAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsByPredicateAsync(Expression<Func<T, bool>> predicate);
    Task UpdateRangeAsync(IEnumerable<T> entities);
}