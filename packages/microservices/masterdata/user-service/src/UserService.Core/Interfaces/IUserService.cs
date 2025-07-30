using System.Collections;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.User;
using UserService.Core.DTOs.Common;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    Task<UserReadDto?> GetByIdAsync(string id);
    Task<UserReadDto> CreateAsync(DTOs.User.CreateUserDto dto);
    Task<UserReadDto?> UpdateAsync(string id, DTOs.User.UpdateUserDto dto);
    Task<bool> DeleteAsync(string id);
    
    // TODO: Add domain-specific service methods here
    Task<User?> ValidateUserCredentials(string email, string password);
    Task<bool> HasPermissionAsync(string userId, string permissionName);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId);
    Task<bool> RestoreAsync(string id);
    Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto);
    Task<PagedResult<UserReadDto>> GetPagedAsync(PaginationParameters parameters);
    Task<PagedResult<UserReadDto>> GetDeletedPagedAsync(PaginationParameters parameters);
    
    // Shift-related methods
    Task<IEnumerable<UserShiftDto>> GetUserShiftsAsync(string userId);
    Task<UserShiftDto?> GetUserShiftByShiftIdAsync(string userId, string shiftId);
    Task<bool> AssignShiftToUserAsync(string userId, string shiftId);
    Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId);
    
    // User status management
    Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive);}