using TechnicianApi.Core.DTOs.Settings;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface ILicenseClassService
{
    Task<LicenseClassResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<LicenseClassResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<IEnumerable<LicenseClassResponseDto>> GetAllAsync();
    Task<LicenseClassResponseDto> CreateAsync(CreateLicenseClassDto dto);
    Task<LicenseClassResponseDto?> UpdateAsync(string id, CreateLicenseClassDto dto);
    Task<bool> DeleteAsync(string id);
}
