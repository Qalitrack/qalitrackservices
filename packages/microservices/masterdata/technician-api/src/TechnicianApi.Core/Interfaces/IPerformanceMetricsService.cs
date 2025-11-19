using TechnicianApi.Core.DTOs.Common;
using TechnicianApi.Core.DTOs.PerformanceMetrics;

namespace TechnicianApi.Core.Interfaces;

public interface IPerformanceMetricsService
{
    Task<PerformanceMetricsResponseDto?> GetByIdAsync(string id);
    Task<PerformanceMetricsResponseDto?> GetLatestByTechnicianIdAsync(string technicianId);
    Task<PagedResponseDto<PerformanceMetricsResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? technicianId = null);
    Task<PerformanceMetricsResponseDto> CalculateMetricsAsync(string technicianId, DateTime periodStart, DateTime periodEnd);
}
