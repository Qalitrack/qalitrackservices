using System.ComponentModel.DataAnnotations;

namespace QaliTrack.DataManager.Core.Common;

/// <summary>
/// Query parameters for filtering, searching, and pagination
/// </summary>
public class QueryParameters
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default 25, max 100)
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Search query (searches across multiple fields like name, code, description)
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Ordering field (supports Django-style ordering: 'name', '-name', 'created_at', '-updated_at')
    /// </summary>
    public string? Ordering { get; set; }

    /// <summary>
    /// Filter by status (e.g., Active, Inactive, Pending)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by type/category
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Filter by created date (from)
    /// </summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>
    /// Filter by created date (to)
    /// </summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>
    /// Filter by updated date (from)
    /// </summary>
    public DateTime? UpdatedFrom { get; set; }

    /// <summary>
    /// Filter by updated date (to)
    /// </summary>
    public DateTime? UpdatedTo { get; set; }

    /// <summary>
    /// Filter by active/inactive status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Generic filters (key-value pairs for any additional filters)
    /// </summary>
    public Dictionary<string, string> Filters { get; set; } = new();
}