using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<UserDto?> GetUserByUsernameAsync(string username);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<UserDto?> GetCompleteUserAsync(string userId);
    Task<PaginatedResponseDto<UserDto>> GetUsersAsync(PaginationRequestDto request);
    Task<PaginatedResponseDto<UserDto>> GetUsersByOrganizationAsync(string organizationId, PaginationRequestDto request);
    Task<UserDto> CreateUserAsync(CreateUserDto request);
    Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto request);
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> ActivateUserAsync(string userId);
    Task<bool> DeactivateUserAsync(string userId);
    Task<bool> SuspendUserAsync(string userId);
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
    Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto request);
    Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(string userId);
    Task<bool> AssignUserRoleAsync(AssignUserRoleDto request);
    Task<bool> RemoveUserRoleAsync(RemoveUserRoleDto request);
    Task<bool> IsUsernameAvailableAsync(string username);
    Task<bool> IsEmailAvailableAsync(string email);
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(string userId);
    Task<bool> RevokeUserSessionAsync(string userId, string sessionId);
    Task<bool> RevokeAllUserSessionsAsync(string userId);
}