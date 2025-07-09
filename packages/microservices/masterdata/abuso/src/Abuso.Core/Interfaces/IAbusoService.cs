using Abuso.Core.DTOs;

namespace Abuso.Core.Interfaces;

public interface IAbusoService
{
    Task<IEnumerable<AbusoReadDto>> GetAllAsync();
    Task<AbusoReadDto?> GetByIdAsync(string id);
    Task<AbusoReadDto> CreateAsync(CreateAbusoDto dto);
    Task<AbusoReadDto?> UpdateAsync(string id, UpdateAbusoDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}