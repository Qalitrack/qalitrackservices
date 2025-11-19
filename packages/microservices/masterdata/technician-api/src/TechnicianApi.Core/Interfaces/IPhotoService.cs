using TechnicianApi.Core.DTOs.Photo;

namespace TechnicianApi.Core.Interfaces;

public interface IPhotoService
{
    Task<PhotoResponseDto?> GetByIdAsync(string id);
    Task<IEnumerable<PhotoResponseDto>> GetByAssignmentIdAsync(string assignmentId);
    Task<IEnumerable<PhotoResponseDto>> GetByAssignmentAndTypeAsync(string assignmentId, string type);
    Task<PhotoResponseDto> CreateAsync(CreatePhotoDto dto);
    Task<bool> DeleteAsync(string id);
}
