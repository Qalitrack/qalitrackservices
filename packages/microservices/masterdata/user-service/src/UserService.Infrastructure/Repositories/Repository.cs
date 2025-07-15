using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Repositories
{
    public abstract class Repository<T> where T : class
    {
        protected readonly UserServiceDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;

        public Repository(UserServiceDbContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
        }

        public async Task<IQueryable<T>> GetAllAsync()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T?> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            try
            {
                // Detach any existing tracked entity with the same ID
                var existingEntity = await _dbSet.FindAsync(GetEntityId(entity));
                if (existingEntity != null)
                {
                    var entry = _dbContext.Entry(existingEntity);
                    if (entry.State != EntityState.Detached)
                    {
                        entry.State = EntityState.Detached;
                    }
                }

                // Attach and update the new entity
                var updatedEntity = _dbSet.Update(entity).Entity;
                await _dbContext.SaveChangesAsync();
                return updatedEntity;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new DbUpdateConcurrencyException("Concurrency conflict occurred while updating the entity.", ex);
            }
        }

// Helper method to get the entity's ID
        private object GetEntityId(T entity)
        {
            var key = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey();
            if (key == null)
                throw new InvalidOperationException($"No primary key defined for entity type {typeof(T).Name}");

            var id = key.Properties
                .Select(p => entity.GetType().GetProperty(p.Name)?.GetValue(entity))
                .FirstOrDefault();

            if (id == null)
                throw new InvalidOperationException($"Could not get ID for entity of type {typeof(T).Name}");

            return id;
        }
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return false;

            _dbSet.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}