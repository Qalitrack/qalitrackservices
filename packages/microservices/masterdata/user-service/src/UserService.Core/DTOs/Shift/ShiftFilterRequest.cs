using UserService.Core.Enums;

namespace UserService.Core.DTOs.Shift;

public class ShiftFilterRequest
{
    public string? Search { get; set; }
    public ShiftStatus? Status { get; set; }
    public ShiftType? Type { get; set; }
    public ShiftMode? Mode { get; set; }
    public string? DepartmentId { get; set; }
    public string? LocationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
        
    // Pagination
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}