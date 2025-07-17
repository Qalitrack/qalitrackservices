using System.Collections;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserReadDto>> GetAllAsync();
    Task<UserReadDto?> GetByIdAsync(string id);
    Task<UserReadDto> CreateAsync(DTOs.User.CreateUserDto dto);
    Task<UserReadDto?> UpdateAsync(string id, DTOs.User.UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    
    // TODO: Add domain-specific service methods here
    Task<User?> ValidateUserCredentials(string email, string password);
    Task<bool> HasPermissionAsync(string userId, string permissionName);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<bool> RestoreAsync(string id);
    Task<IEnumerable<UserReadDto>> GetDeletedAsync();
    Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto);}