using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IMaterialVariantService
{
    Task<MaterialVariantResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<MaterialVariantResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? materialId = null);
    Task<IEnumerable<MaterialVariantResponseDto>> GetByMaterialIdAsync(string materialId);
    Task<MaterialVariantResponseDto> CreateAsync(CreateMaterialVariantDto dto);
    Task<MaterialVariantResponseDto?> UpdateAsync(string id, CreateMaterialVariantDto dto);
    Task<bool> DeleteAsync(string id);
}
