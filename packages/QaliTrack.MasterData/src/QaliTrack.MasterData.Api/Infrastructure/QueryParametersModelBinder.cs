using Microsoft.AspNetCore.Mvc.ModelBinding;
using QaliTrack.MasterData.Core.Common;

namespace QaliTrack.MasterData.Api.Infrastructure;

/// <summary>
/// Django-style query parameter binding
/// </summary>
public class QueryParametersModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.ModelType != typeof(QueryParameters))
        {
            return Task.CompletedTask;
        }

        var request = bindingContext.HttpContext.Request;
        var queryParams = new QueryParameters();

        // Page (Django uses 'page')
        if (int.TryParse(request.Query["page"], out var page))
        {
            queryParams.Page = Math.Max(1, page);
        }

        // Page size (Django REST uses 'page_size')
        if (int.TryParse(request.Query["page_size"], out var pageSize))
        {
            queryParams.PageSize = Math.Min(Math.Max(1, pageSize), 100); // Cap at 100
        }

        // Search (Django uses 'search')
        queryParams.Search = request.Query["search"];

        // Ordering (Django uses 'ordering')
        queryParams.Ordering = request.Query["ordering"];

        // Dynamic filters (any other query parameters)
        var excludedParams = new[] { "page", "page_size", "search", "ordering" };
        foreach (var kvp in request.Query.Where(q => !excludedParams.Contains(q.Key.ToLower())))
        {
            queryParams.Filters[kvp.Key] = kvp.Value.ToString();
        }

        bindingContext.Result = ModelBindingResult.Success(queryParams);
        return Task.CompletedTask;
    }
}

public class QueryParametersModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(QueryParameters))
        {
            return new QueryParametersModelBinder();
        }
        return null;
    }
}