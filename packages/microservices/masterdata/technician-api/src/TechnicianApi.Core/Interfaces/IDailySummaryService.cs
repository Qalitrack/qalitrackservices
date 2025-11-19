using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.DailySummary;

namespace TechnicianApi.Core.Interfaces;

public interface IDailySummaryService
{
    Task<DailySummaryResponseDto?> GetByIdAsync(string id);
    Task<DailySummaryResponseDto?> GetByTechnicianAndDateAsync(string technicianId, DateTime date);
    Task<PagedResponseDto<DailySummaryResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? technicianId = null);
    Task<DailySummaryResponseDto> GenerateSummaryAsync(string technicianId, DateTime date);
}
