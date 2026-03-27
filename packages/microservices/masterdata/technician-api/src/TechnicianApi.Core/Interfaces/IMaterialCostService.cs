using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IMaterialCostService
{
    Task<MaterialCostResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<MaterialCostResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? materialId = null);
    Task<IEnumerable<MaterialCostResponseDto>> GetByMaterialIdAsync(string materialId);
    Task<MaterialCostResponseDto> CreateAsync(CreateMaterialCostDto dto);
    Task<MaterialCostResponseDto?> UpdateAsync(string id, CreateMaterialCostDto dto);
    Task<bool> DeleteAsync(string id);
}
