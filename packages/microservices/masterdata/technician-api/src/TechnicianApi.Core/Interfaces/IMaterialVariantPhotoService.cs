using TechnicianApi.Core.DTOs.Fleet;

namespace TechnicianApi.Core.Interfaces;

public interface IMaterialVariantPhotoService
{
    Task<MaterialVariantPhotoResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<MaterialVariantPhotoResponseDto>> GetByVariantIdAsync(string variantId);
    Task<MaterialVariantPhotoResponseDto> CreateAsync(CreateMaterialVariantPhotoDto dto);
    Task<bool> DeleteAsync(string id);
}
