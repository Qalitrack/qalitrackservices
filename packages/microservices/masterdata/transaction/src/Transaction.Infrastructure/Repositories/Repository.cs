using Microsoft.EntityFrameworkCore;
using Transaction.Core.Entities;
using Transaction.Core.Interfaces;
using Transaction.Core.Services;
using Transaction.Infrastructure.Data;

namespace Transaction.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly TransactionDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ITimeService _timeService;

    public Repository(TransactionDbContext context, ITimeService timeService)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _timeService = timeService;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        // Read-only: UpdateAsync/DeleteAsync below always re-fetch by id
        // rather than relying on the caller's instance being tracked, so
        // callers of this method never need change tracking either.
        return await _dbSet.AsNoTracking().Where(e => !e.IsDeleted).ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = _timeService.UtcNow;
        entity.UpdatedAt = _timeService.UtcNow;
        
        _dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        var existingEntity = await _dbSet.FindAsync(entity.Id);
        if (existingEntity == null)
        {
            return null;
        }

        // Update all properties from the incoming entity to the existing entity
        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
        
        // Explicitly set UpdatedAt to current time
        existingEntity.UpdatedAt = _timeService.UtcNow;
        
        // Mark the entity as modified to ensure all changes are saved
        _context.Entry(existingEntity).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
        return existingEntity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = _timeService.UtcNow;
        
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }
}