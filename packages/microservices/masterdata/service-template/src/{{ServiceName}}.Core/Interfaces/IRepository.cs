using {{ServiceName}}.Core.Entities;

namespace {{ServiceName}}.Core.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<T> CreateAsync(T entity);
    Task<T?> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> ExistsAsync(string id);
}