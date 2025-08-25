using CustomerService.Core.Entities;
using System.Linq.Expressions;

namespace CustomerService.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T?> FindAsync(string id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
}