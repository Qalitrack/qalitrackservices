using System.Text.Json.Serialization;

namespace Masterdata.Core.Models;

public class PagedResult<T>
{
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = new List<T>();
    
    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }
    
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalItems")]
    public int TotalItems { get; set; }
    
    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage => PageNumber > 1;
    
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage => PageNumber < TotalPages;
}
