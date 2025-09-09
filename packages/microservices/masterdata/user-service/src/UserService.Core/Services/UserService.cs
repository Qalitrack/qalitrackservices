using System.Collections;
using AutoMapper;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.User;
using UserService.Core.DTOs.Common;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using UserService.Core.Interfaces.Emails;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using UserService.Core.Utilities;

namespace UserService.Core.Services
{
    public class UserService(
        IUserRepository userRepository,
        IMapper mapper,
        IRoleRepository roleRepository,
        IHttpContextAccessor httpContextAccessor,
        IEmailQueueService emailQueueService,
        PasswordPolicyService passwordPolicyService)
        : IUserService
    {
        private readonly ILogger<UserService> _logger = new Logger<UserService>(new LoggerFactory());
        private readonly PasswordPolicyService _passwordPolicyService = passwordPolicyService ?? throw new ArgumentNullException(nameof(passwordPolicyService));
        private readonly IEmailQueueService _emailQueueService = emailQueueService ?? throw new ArgumentNullException(nameof(emailQueueService));

        public async Task<bool> RestoreAsync(string id)
        {
            return await userRepository.RestoreAsync(id);
        }

        public async Task<UserReadDto?> GetByIdAsync(string id)
        {
            var user = await userRepository.GetByIdAsync(id, true);
    
            if (user == null) 
            {
                return null;
            }
            
            return mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
        {
            var currentUserId = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MobileNumber = dto.MobileNumber,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsFirstLogin = true,
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                UpdatedBy = currentUserId
            };

            await userRepository.CreateAsync(user);
            return mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto)
        {
            var existingUser = await userRepository.GetByIdAsync(id, true);
            if (existingUser == null)
            {
                return null;
            }

            var currentUserId = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
            
            // FIXED: Properly preserve all existing user data
            var userToUpdate = new User
            {
                Id = existingUser.Id,
                FirstName = dto.FirstName ?? existingUser.FirstName,
                LastName = dto.LastName ?? existingUser.LastName,
                Email = dto.Email ?? existingUser.Email,
                MobileNumber = dto.MobileNumber ?? existingUser.MobileNumber,
                Password = existingUser.Password, // Keep existing password
                IsFirstLogin = dto.IsFirstLogin ?? existingUser.IsFirstLogin,
                IsActive = existingUser.IsActive, // FIXED: Preserve IsActive status
                CreatedAt = existingUser.CreatedAt, // FIXED: Preserve original CreatedAt
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = currentUserId,
                CreatedBy = existingUser.CreatedBy, // Keep original creator
                IsDeleted = existingUser.IsDeleted, // Preserve deletion status
               
            };
            
            var updatedUser = await userRepository.UpdateAsync(userToUpdate);
            return updatedUser == null ? null : mapper.Map<UserReadDto>(updatedUser);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await userRepository.DeleteAsync(id);
        }

        public async Task<User?> ValidateUserCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var user = await userRepository.GetByEmailAsync(email.Trim().ToLower());
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            // FIXED: Only update if login validation succeeds - avoid unnecessary DB calls
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User);
            await userRepository.UpdateAsync(user);

            return user;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            return await userRepository.HasPermissionAsync(userId, permissionName);
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            return await userRepository.GetUserPermissionsAsync(userId);
        }

        public async Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto)
        {
            var user = await userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Validate new password against password policy
            var validationResult = await _passwordPolicyService.ValidatePasswordAsync(dto.NewPassword, userId);
            if (!validationResult.IsValid)
            {
                throw new InvalidOperationException($"New password does not meet policy requirements: {string.Join(", ", validationResult.Errors)}");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsFirstLogin = false;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = AuthUtils.GetUserIdFromClaims(httpContextAccessor.HttpContext?.User) ?? "System";

            await userRepository.UpdateAsync(user);
            return mapper.Map<UserReadDto>(user);
        }

        public async Task<IEnumerable<UserShiftDto>> GetUserShiftsAsync(string userId)
        {
            var userShifts = await userRepository.GetUserShiftsAsync(userId);
            return mapper.Map<IEnumerable<UserShiftDto>>(userShifts);
        }

        public async Task<UserShiftDto?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
        {
            var userShift = await userRepository.GetUserShiftByShiftIdAsync(userId, shiftId);
            return userShift == null ? null : mapper.Map<UserShiftDto>(userShift);
        }

        public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
        {
            return await userRepository.AssignShiftToUserAsync(userId, shiftId);
        }

        public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
        {
            return await userRepository.RemoveShiftFromUserAsync(userId, shiftId);
        }

        public async Task<PagedResult<UserReadDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var pagedUsers = await userRepository.GetPagedAsync(parameters);
            var userDtos = mapper.Map<IEnumerable<UserReadDto>>(pagedUsers.Items);

            var userReadDtos = userDtos as UserReadDto[] ?? userDtos.ToArray();
            foreach (var userDto in userReadDtos)
            {
                var user = pagedUsers.Items.FirstOrDefault(u => u.Id == userDto.Id);
                if (user?.UserRoles != null)
                {
                    userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                }
            }

            return new PagedResult<UserReadDto>
            {
                Items = userReadDtos,
                Page = pagedUsers.Page,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount
            };
        }

        public async Task<PagedResult<UserReadDto>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            var pagedUsers = await userRepository.GetDeletedPagedAsync(parameters);
            var userDtos = mapper.Map<IEnumerable<UserReadDto>>(pagedUsers.Items);

            var userReadDtos = userDtos as UserReadDto[] ?? userDtos.ToArray();
            foreach (var userDto in userReadDtos)
            {
                var user = pagedUsers.Items.FirstOrDefault(u => u.Id == userDto.Id);
                if (user?.UserRoles != null)
                {
                    userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                }
            }

            return new PagedResult<UserReadDto>
            {
                Items = userReadDtos,
                Page = pagedUsers.Page,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount
            };
        }

        public async Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive)
        {
            var user = await userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                return false;
            }

            user.IsActive = isActive;
            // Repository will handle UpdatedAt/UpdatedBy
            
            var result = await userRepository.UpdateUserActiveStatusAsync(userId, isActive);
            
            return result;
        }

        public async Task<IEnumerable<string>> GetPermissionsForRoleAsync(string roleName)
        {
            var role = await roleRepository.GetRoleWithPermissionsAsync(roleName);
            if (role?.RolePermissions == null)
                return Enumerable.Empty<string>();

            return role.RolePermissions
                .Where(rp => rp.Permission != null)
                .Select(rp => rp.Permission.Name);
        }

        public async Task<IEnumerable<Role>> GetUserRolesByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            // Check if user exists
            var user = await userRepository.GetByIdAsync(userId, false);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Get roles for the user
            return await userRepository.GetUserRolesByUserIdAsync(userId);
        }

        public async Task<bool> ResetUserPasswordAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));

            try
            {
                // Check if user exists first
                var user = await userRepository.GetByIdAsync(userId, false);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found.");
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    throw new InvalidOperationException("Cannot reset password for an inactive user.");
                }

                // Reset password using repository method
                var result = await userRepository.ResetUserPasswordAsync(userId, _emailQueueService);
        
                if (result)
                {
                    _logger.LogInformation("Password reset successfully initiated for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Password reset failed for user {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                throw;
            }
        }
    }
}