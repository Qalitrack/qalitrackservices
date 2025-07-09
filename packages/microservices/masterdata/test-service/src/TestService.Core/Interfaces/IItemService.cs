using TestService.Core.DTOs;

namespace TestService.Core.Interfaces;

public interface IItemService
{
    Task<IEnumerable<ItemReadDto>> GetAllAsync();
    Task<ItemReadDto?> GetByIdAsync(string id);
    Task<ItemReadDto> CreateAsync(CreateItemDto dto);
    Task<ItemReadDto?> UpdateAsync(string id, UpdateItemDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}