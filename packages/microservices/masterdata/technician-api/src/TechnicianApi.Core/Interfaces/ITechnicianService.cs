using TechnicianApi.Core.DTOs;

namespace TechnicianApi.Core.Interfaces;

public interface ITechnicianService
{
    Task<IEnumerable<TechnicianReadDto>> GetAllAsync();
    Task<TechnicianReadDto?> GetByIdAsync(string id);
    Task<TechnicianReadDto> CreateAsync(CreateTechnicianDto dto);
    Task<TechnicianReadDto?> UpdateAsync(string id, UpdateTechnicianDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}