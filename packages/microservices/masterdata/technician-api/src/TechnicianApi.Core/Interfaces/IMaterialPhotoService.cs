using TechnicianApi.Core.DTOs.Fleet;

namespace TechnicianApi.Core.Interfaces;

public interface IMaterialPhotoService
{
    Task<MaterialPhotoResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<MaterialPhotoResponseDto>> GetByMaterialIdAsync(string materialId);
    Task<MaterialPhotoResponseDto> CreateAsync(CreateMaterialPhotoDto dto);
    Task<bool> DeleteAsync(string id);
}
