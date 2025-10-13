using Masterdata.Core.DTOs;

namespace Masterdata.Core.Interfaces;

public interface IBaseService
{
    Task<IEnumerable<BaseReadDto>> GetAllAsync();
    Task<BaseReadDto?> GetByIdAsync(string id);
    Task<BaseReadDto> CreateAsync(CreateBaseDto dto);
    Task<BaseReadDto?> UpdateAsync(string id, UpdateBaseDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}