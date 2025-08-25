using ReportService.Core.DTOs;
using ReportService.Core.Entities;

namespace ReportService.Core.Interfaces;

public interface IReportService
{
    // Basic CRUD Operations - only these are implemented
    Task<IEnumerable<ReportReadDto>> GetAllAsync();
    Task<ReportReadDto?> GetByIdAsync(string id);
    Task<ReportReadDto> CreateAsync(CreateReportDto dto);
    Task<ReportReadDto?> UpdateAsync(string id, UpdateReportDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
}