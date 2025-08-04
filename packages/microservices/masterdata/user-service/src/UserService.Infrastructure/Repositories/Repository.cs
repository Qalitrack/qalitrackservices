using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserService.Api.Authorization;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace UserService.Infrastructure.Repositories
{
    public abstract class Repository<T> where T : class
    {
        private readonly UserServiceDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        protected readonly IHttpContextAccessor HttpContextAccessor;

        protected Repository(UserServiceDbContext dbContext, IHttpContextAccessor httpContextAccessor = null)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task<T> CreateAsync(T entity)
        {
            // Set audit fields if entity implements IBaseEntity
            if (entity is BaseEntity baseEntity)
            {
                var currentUserId = GetCurrentUserId();
                var now = DateTime.UtcNow;

                if (string.IsNullOrEmpty(baseEntity.Id))
                {
                    baseEntity.Id = Guid.NewGuid().ToString();
                }
                
                baseEntity.CreatedAt = now;
                baseEntity.UpdatedAt = now;
                baseEntity.CreatedBy = currentUserId;
                baseEntity.UpdatedBy = currentUserId;
                baseEntity.IsDeleted = false;
            }

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
                // Set audit fields if entity implements IBaseEntity
                if (entity is BaseEntity baseEntity)
                {
                    var currentUserId = GetCurrentUserId();
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                    baseEntity.UpdatedBy = currentUserId;
                }

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

        // Soft delete method
        public virtual async Task<bool> DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
                return false;

            if (entity is BaseEntity baseEntity)
            {
                // Soft delete
                var currentUserId = GetCurrentUserId();
                baseEntity.IsDeleted = true;
                baseEntity.UpdatedAt = DateTime.UtcNow;
                baseEntity.UpdatedBy = currentUserId;
                
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                // Hard delete for entities that don't support soft delete
                _dbSet.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }

            return true;
        }

        // Restore method for soft-deleted entities
        public virtual async Task<bool> RestoreAsync(object id)
        {
            var entity = await _dbSet.IgnoreQueryFilters().Cast<BaseEntity>().FirstOrDefaultAsync(e => e.Id.Equals(id.ToString()));
            if (entity == null || !entity.IsDeleted)
                return false;

            var currentUserId = GetCurrentUserId();
            entity.IsDeleted = false;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = currentUserId;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        // Helper method to get current user ID
        protected string GetCurrentUserId()
        {
            return AuthUtils.GetUserIdFromClaims(HttpContextAccessor?.HttpContext?.User) ?? "System";
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

        // Common helper methods
        protected async Task<bool> ExistsAsync(object id)
        {
            return await _dbSet.FindAsync(id) != null;
        }
        
        protected async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        
        protected IQueryable<T> Query => _dbSet.AsQueryable();
    }
}