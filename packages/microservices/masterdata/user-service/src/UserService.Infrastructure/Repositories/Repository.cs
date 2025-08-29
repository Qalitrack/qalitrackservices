using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UserService.Core.Utilities;

namespace UserService.Infrastructure.Repositories
{
    public abstract class Repository<T> where T : class
    {
        private readonly UserServiceDbContext _dbContext;
        private readonly DbSet<T> _dbSet;
        protected readonly IHttpContextAccessor HttpContextAccessor;
        protected readonly ILogger<Repository<T>> _logger;

        protected Repository(UserServiceDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<Repository<T>> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<T>();
            HttpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                var currentUserId = GetCurrentUserId();
                baseEntity.IsDeleted = true;
                baseEntity.UpdatedAt = DateTime.UtcNow;
                baseEntity.UpdatedBy = currentUserId;

                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
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
            try
            {
                if (HttpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated != true)
                {
                    _logger.LogWarning("No authenticated user context available, defaulting to 'System' for CreatedBy/UpdatedBy");
                    return "System";
                }

                var userId = AuthUtils.GetUserIdFromClaims(HttpContextAccessor.HttpContext.User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims, defaulting to 'System'");
                    return "System";
                }

                _logger.LogInformation("Retrieved user ID: {UserId}", userId);
                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user ID, defaulting to 'System'");
                return "System";
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
    }
}