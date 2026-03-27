using TechnicianApi.Core.DTOs.Fleet;
using TechnicianApi.Core.DTOs.Common;

namespace TechnicianApi.Core.Interfaces;

public interface IMaterialService
{
    Task<MaterialResponseDto?> GetByIdAsync(string id);
    Task<PagedResponseDto<MaterialResponseDto>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null);
    Task<MaterialResponseDto> CreateAsync(CreateMaterialDto dto);
    Task<MaterialResponseDto?> UpdateAsync(string id, CreateMaterialDto dto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<MaterialVariantResponseDto>> GetVariantsByMaterialIdAsync(string materialId);
}
