using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserReadDto>> GetAllAsync();
    Task<UserReadDto?> GetByIdAsync(string id);
    Task<UserReadDto> CreateAsync(CreateUserDto dto);
    Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    
    // TODO: Add domain-specific service methods here
}