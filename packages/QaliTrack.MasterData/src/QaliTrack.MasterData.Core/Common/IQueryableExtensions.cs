using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace QaliTrack.MasterData.Core.Common;

/// <summary>
/// Django-style query extensions for filtering, searching, and pagination
/// </summary>
public static class IQueryableExtensions
{
    /// <summary>
    /// Apply Django-style filtering, searching, and pagination
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        QueryParameters parameters,
        string baseUrl,
        CancellationToken cancellationToken = default) where T : class
    {
        // Apply search
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Search(parameters.Search);
        }

        // Apply filters
        query = query.ApplyFilters(parameters.Filters);

        // Apply ordering
        if (!string.IsNullOrEmpty(parameters.Ordering))
        {
            query = query.ApplyOrdering(parameters.Ordering);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling((double)totalCount / parameters.PageSize);

        // Apply pagination
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Count = totalCount,
            Results = items,
            Next = parameters.Page < totalPages ? $"{baseUrl}?page={parameters.Page + 1}&page_size={parameters.PageSize}" : null,
            Previous = parameters.Page > 1 ? $"{baseUrl}?page={parameters.Page - 1}&page_size={parameters.PageSize}" : null,
            Pagination = new PaginationInfo
            {
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalPages = totalPages,
                TotalItems = totalCount,
                HasNext = parameters.Page < totalPages,
                HasPrevious = parameters.Page > 1
            }
        };
    }

    /// <summary>
    /// Django-style search across multiple fields
    /// </summary>
    public static IQueryable<T> Search<T>(this IQueryable<T> query, string searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
            return query;

        var searchFields = GetSearchableFields<T>();
        if (!searchFields.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? searchExpression = null;

        foreach (var field in searchFields)
        {
            var property = Expression.Property(parameter, field);
            var toString = Expression.Call(property, "ToString", null);
            var contains = Expression.Call(toString, "Contains", null, Expression.Constant(searchTerm, typeof(string)));

            searchExpression = searchExpression == null ? contains : Expression.OrElse(searchExpression, contains);
        }

        if (searchExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(searchExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Django-style filtering with key-value pairs
    /// </summary>
    public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, Dictionary<string, string> filters)
    {
        if (!filters.Any())
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");

        foreach (var filter in filters)
        {
            var property = GetPropertyExpression<T>(parameter, filter.Key);
            if (property != null)
            {
                var constant = Expression.Constant(filter.Value);
                var equals = Expression.Equal(property, constant);
                var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);
                query = query.Where(lambda);
            }
        }

        return query;
    }

    /// <summary>
    /// Django-style ordering (supports 'field' and '-field')
    /// </summary>
    public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, string ordering)
    {
        if (string.IsNullOrEmpty(ordering))
            return query;

        var isDescending = ordering.StartsWith('-');
        var fieldName = isDescending ? ordering.Substring(1) : ordering;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = GetPropertyExpression<T>(parameter, fieldName);
        
        if (property == null)
            return query;

        var lambda = Expression.Lambda(property, parameter);
        var methodName = isDescending ? "OrderByDescending" : "OrderBy";

        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.Type },
            query.Expression,
            lambda);

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    private static IEnumerable<string> GetSearchableFields<T>()
    {
        return typeof(T).GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .Select(p => p.Name)
            .Where(name => name != "Id" && !name.EndsWith("Id"))
            .ToList();
    }

    private static Expression? GetPropertyExpression<T>(ParameterExpression parameter, string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        return property != null ? Expression.Property(parameter, property) : null;
    }

    // Add missing extension methods for async operations
    public static Task<int> CountAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query, cancellationToken);
    }
}