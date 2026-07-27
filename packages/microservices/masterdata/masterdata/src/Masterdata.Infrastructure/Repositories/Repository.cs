using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Masterdata.Core.Entities;
using Masterdata.Core.Interfaces;
using Masterdata.Core.Models;
using Masterdata.Core.Utils;
using Masterdata.Infrastructure.Data;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Masterdata.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly MasterdataDbContext Context;
    protected readonly DbSet<T> DbSet;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenExtractionService _tokenExtractionService;
    private readonly IAuditLogRepository _auditLogRepository;

    private string? CurrentUserId => _httpContextAccessor.HttpContext != null 
        ? _tokenExtractionService.GetUserIdFromToken(_httpContextAccessor.HttpContext.User)?.ToString() 
        : null;

    public Repository(
        MasterdataDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ITokenExtractionService tokenExtractionService,
        IAuditLogRepository auditLogRepository)
    {
        Context = context;
        DbSet = context.Set<T>();
        _httpContextAccessor = httpContextAccessor;
        _tokenExtractionService = tokenExtractionService;
        _auditLogRepository = auditLogRepository;
    }

    private async Task<TResult> ExecuteWithStrategyAsync<TResult>(Func<Task<TResult>> operation)
    {
        var strategy = Context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(operation);
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Expression<Func<T, bool>>? searchPredicate = null,
        string[]? searchProperties = null,
        string sortBy = "CreatedAt",
        bool sortDescending = false)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = DbSet.Where(e => !e.IsDeleted);

        // Apply search predicate if provided
        if (searchPredicate != null)
        {
            query = query.Where(searchPredicate);
        }
        // Apply search term across specified properties or default to Id
        else if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();

            if (searchProperties != null && searchProperties.Any())
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                Expression? searchExpression = null;
                // Postgres's native case-insensitive ILIKE — EF Core silently drops a
                // reflection-built .ToLower() call inside EF.Functions.Like(), which made
                // every real-world (properly-capitalized) search term match nothing.
                // ILIKE sidesteps that translation gap entirely.
                var likeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod(
                    nameof(NpgsqlDbFunctionsExtensions.ILike),
                    new[] { typeof(DbFunctions), typeof(string), typeof(string) });

                foreach (var propertyName in searchProperties)
                {
                    var property = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property != null && property.PropertyType == typeof(string))
                    {
                        var propertyExpression = Expression.Property(parameter, property);
                        var likeCall = Expression.Call(
                            null,
                            likeMethod!,
                            Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions))!),
                            propertyExpression,
                            Expression.Constant($"%{searchTerm}%"));

                        searchExpression = searchExpression == null ? likeCall : Expression.OrElse(searchExpression, likeCall);
                    }
                }

                if (searchExpression != null)
                {
                    var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
                    query = query.Where(lambda);
                }
            }
            else
            {
                // Default to searching by Id
                query = query.Where(e => EF.Functions.ILike(e.Id, $"%{searchTerm}%"));
            }
        }

        // Apply sorting
        var sortProperty = typeof(T).GetProperty(sortBy, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                           ?? typeof(BaseEntity).GetProperty("CreatedAt"); // Fallback to CreatedAt

        if (sortProperty != null)
        {
            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyExpression = Expression.Property(parameter, sortProperty);
            var lambda = Expression.Lambda<Func<T, object>>(
                Expression.Convert(propertyExpression, typeof(object)),
                parameter);

            query = sortDescending ? query.OrderByDescending(lambda) : query.OrderBy(lambda);
        }

        // Execute query
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public virtual async Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var idSet = new HashSet<string>(ids);
        return await DbSet.Where(e => idSet.Contains(e.Id) && !e.IsDeleted).ToListAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.Id = Guid.NewGuid().ToString();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        if (CurrentUserId != null)
        {
            entity.CreatedBy = CurrentUserId;
            entity.UpdatedBy = CurrentUserId;
        }

        return await ExecuteWithStrategyAsync(async () =>
        {
            await using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                DbSet.Add(entity);
                // Detect and track related entities before save
                await TrackRelatedEntitiesAsync(entity, isCreate: true);
                await Context.SaveChangesAsync();

                await _auditLogRepository.LogCreateAsync(entity);
                // Log related entity changes
                await LogRelatedEntityChangesAsync();

                await transaction.CommitAsync();
                return entity;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public virtual async Task<T?> UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var originalEntity = await DbSet.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entity.Id && !e.IsDeleted);

        if (originalEntity == null)
        {
            return null;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        if (CurrentUserId != null)
        {
            entity.UpdatedBy = CurrentUserId;

            var existingEntity = await DbSet.AsNoTracking()
                .Where(e => e.Id == entity.Id)
                .Select(e => new { e.CreatedBy })
                .FirstOrDefaultAsync();

            if (existingEntity != null && entity.CreatedBy == null)
            {
                entity.CreatedBy = existingEntity.CreatedBy;
            }
        }

        return await ExecuteWithStrategyAsync(async () =>
        {
            await using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                DbSet.Update(entity);
                // Detect and track related entities before save
                await TrackRelatedEntitiesAsync(entity, isCreate: false);
                await Context.SaveChangesAsync();

                await _auditLogRepository.LogUpdateAsync(originalEntity, entity);
                // Log related entity changes
                await LogRelatedEntityChangesAsync();

                await transaction.CommitAsync();
                return entity;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var entities = await GetByIdsAsync(new[] { id });
        var entity = entities.FirstOrDefault();

        if (entity == null)
        {
            return false;
        }

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        if (CurrentUserId != null)
        {
            entity.UpdatedBy = CurrentUserId;
        }

        return await ExecuteWithStrategyAsync(async () =>
        {
            await using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                DbSet.Update(entity);
                // Detect and track related entities before save
                await TrackRelatedEntitiesAsync(entity, isCreate: false);
                await Context.SaveChangesAsync();

                await _auditLogRepository.LogDeleteAsync(entity);
                // Log related entity changes
                await LogRelatedEntityChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public virtual async Task<PagedResult<T>> GetDeletedPagedAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = DbSet.Where(e => e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.Trim().ToLower();
            query = query.Where(e => EF.Functions.Like(e.Id.ToLower(), $"%{searchTerm}%"));
        }

        var totalItems = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.UpdatedAt)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public virtual async Task<T?> GetByPredicateAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<IEnumerable<T>> GetAllByPredicateAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    public virtual async Task<bool> ExistsByPredicateAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet
            .Where(e => !e.IsDeleted)
            .AnyAsync(predicate);
    }

           /// <summary>
        /// Bulk update multiple entities in a single operation
        /// </summary>
        /// <param name="entities">The entities to update</param>
        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            var entityList = entities?.ToList() ?? new List<T>();
            if (!entityList.Any()) return;

            var strategy = Context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Context.Database.BeginTransactionAsync();
                try
                {
                    // Apply audit fields to all entities
                    foreach (var entity in entityList)
                    {
                        entity.UpdatedAt = DateTime.UtcNow;
                        if (CurrentUserId != null)
                        {
                            entity.UpdatedBy = CurrentUserId;

                            // Preserve CreatedBy if it was not set
                            var existingCreatedBy = await DbSet.AsNoTracking()
                                .Where(e => e.Id == entity.Id)
                                .Select(e => e.CreatedBy)
                                .FirstOrDefaultAsync();

                            if (!string.IsNullOrEmpty(existingCreatedBy) && string.IsNullOrEmpty(entity.CreatedBy))
                            {
                                entity.CreatedBy = existingCreatedBy;
                            }
                        }
                    }

                    // Perform the bulk update
                    DbSet.UpdateRange(entityList);

                    // Allow tracking of related entities (if overridden)
                    foreach (var entity in entityList)
                    {
                        await TrackRelatedEntitiesAsync(entity, isCreate: false);
                    }

                    await Context.SaveChangesAsync();

                    // Audit logging for each updated entity
                    // Note: original values are not captured here (unlike single UpdateAsync)
                    foreach (var entity in entityList)
                    {
                        // Using same entity as old/new — for real diff, fetch originals beforehand if needed
                        await _auditLogRepository.LogUpdateAsync(entity, entity);
                    }

                    // Log any related entity changes detected by ChangeTracker
                    await LogRelatedEntityChangesAsync();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        return await DbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }

    /// <summary>
    /// Tracks related entities that will be modified as a result of the current operation
    /// </summary>
    protected virtual async Task TrackRelatedEntitiesAsync(T entity, bool isCreate)
    {
        // This can be overridden in specific repositories if needed
        await Task.CompletedTask;
    }

    /// <summary>
    /// Logs audit entries for all related entities that were modified
    /// </summary>
    private async Task LogRelatedEntityChangesAsync()
    {
        var changeTracker = Context.ChangeTracker;
        var entries = changeTracker.Entries()
            .Where(e => e.Entity is BaseEntity && e.Entity.GetType() != typeof(T))
            .ToList();

        foreach (var entry in entries)
        {
            var baseEntity = (BaseEntity)entry.Entity;
            switch (entry.State)
            {
                case EntityState.Added:
                    await _auditLogRepository.LogCreateAsync(baseEntity);
                    break;
                case EntityState.Modified:
                    var originalValues = entry.OriginalValues.ToObject();
                    if (originalValues is BaseEntity originalEntity)
                    {
                        await _auditLogRepository.LogUpdateAsync(originalEntity, baseEntity);
                    }
                    break;
                case EntityState.Deleted:
                    await _auditLogRepository.LogDeleteAsync(baseEntity);
                    break;
            }
        }
    }
}