using ReportService.Core.DTOs;

namespace ReportService.Core.Interfaces;

public interface IReportService
{
    Task<IEnumerable<ReportReadDto>> GetAllAsync();
    Task<ReportReadDto?> GetByIdAsync(string id);
    Task<ReportReadDto> CreateAsync(CreateReportDto dto);
    Task<ReportReadDto?> UpdateAsync(string id, UpdateReportDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}