using TechnicianApi.Core.DTOs.Driver;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IDriverService
{
    Task<DriverResponseDto?> GetByIdAsync(string id);
    Task<DriverResponseDto?> GetByUserIdAsync(string userId);
    Task<PagedResponseDto<DriverResponseDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<DriverResponseDto> CreateAsync(CreateDriverDto dto);
    Task<DriverResponseDto?> UpdateAsync(string id, UpdateDriverDto dto);
    Task<bool> DeleteAsync(string id);
}
