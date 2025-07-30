using System.ComponentModel.DataAnnotations;

namespace UserService.Core.DTOs.Common;

public class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int Page { get; set; } = 1;

    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value; 
    }

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}