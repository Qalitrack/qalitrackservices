using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface ITruckService
{
    Task<TruckResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<TruckResponseDto>> GetPagedAsync(int pageNumber, int pageSize);
    Task<TruckResponseDto> CreateAsync(CreateTruckDto dto);
    Task<TruckResponseDto?> UpdateAsync(string id, CreateTruckDto dto);
    Task<bool> DeleteAsync(string id);
}
