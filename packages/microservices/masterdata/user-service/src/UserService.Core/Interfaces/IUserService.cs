using UserService.Core.DTOs;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserReadDto>> GetAllAsync();
    Task<UserReadDto?> GetByIdAsync(string id);
    Task<UserReadDto> CreateAsync(CreateUserDto dto);
    Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    
    // TODO: Add domain-specific service methods here
    Task<User?> ValidateUserCredentials(string email, string password);
}