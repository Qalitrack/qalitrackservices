using System.Collections;
using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Auth;
using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs.User;
using UserService.Core.DTOs.Common;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IRoleService roleService, IMapper mapper)
        {
            _userRepository = userRepository;
            _roleService = roleService;
            _mapper = mapper;
        }


        public async Task<bool> RestoreAsync(string id)
        {
            return await _userRepository.RestoreAsync(id);  
        }


        public async Task<UserReadDto?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id,true);
            return user == null ? null : _mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
        {
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                MobileNumber = dto.MobileNumber,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsFirstLogin = true,
                IsActive = false
            };

            await _userRepository.CreateAsync(user);
            return _mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id,true);
            if (existingUser == null)
            {
                return null;
            }

            // Create a new entity with updated values to avoid tracking conflicts
            // Only update fields that are provided (not null)
            var userToUpdate = new User
            {
                Id = existingUser.Id,
                FirstName = dto.FirstName ?? existingUser.FirstName,
                LastName = dto.LastName ?? existingUser.LastName,
                Email = dto.Email ?? existingUser.Email,
                MobileNumber = dto.MobileNumber ?? existingUser.MobileNumber,
                Password = existingUser.Password, // Keep existing password
                IsFirstLogin = dto.IsFirstLogin ?? existingUser.IsFirstLogin,
                UpdatedAt = DateTime.UtcNow
            };
            
            var updatedUser = await _userRepository.UpdateAsync(userToUpdate);
            return updatedUser == null ? null : _mapper.Map<UserReadDto>(updatedUser);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _userRepository.DeleteAsync(id);
        }

        public async Task<User?> ValidateUserCredentials(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // Get user by email (case-insensitive)
            var user = await _userRepository.GetByEmailAsync(email.Trim().ToLower());
            if (user == null || user.IsDeleted)
            {
                return null;
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return null;
            }

            // Just update the last login time
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return user;
        }

        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            return await _userRepository.HasPermissionAsync(userId, permissionName);
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            return await _userRepository.GetUserPermissionsAsync(userId);  // This returns IEnumerable<Permission>
        }


        public async Task<UserReadDto> UpdatePassword(string userId, UpdatePasswordDto dto)
        {
            var user = await _userRepository.GetByIdAsync(userId, true);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
            {
                throw new InvalidOperationException("Current password is incorrect");
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.IsFirstLogin = false;  // Reset first login flag
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserReadDto>(user);
        }


        // New Methods for Shift Management

        public async Task<IEnumerable<UserShiftDto>> GetUserShiftsAsync(string userId)
        {
            var userShifts = await _userRepository.GetUserShiftsAsync(userId);
            return _mapper.Map<IEnumerable<UserShiftDto>>(userShifts);
        }

        public async Task<UserShiftDto?> GetUserShiftByShiftIdAsync(string userId, string shiftId)
        {
            var userShift = await _userRepository.GetUserShiftByShiftIdAsync(userId, shiftId);
            return userShift == null ? null : _mapper.Map<UserShiftDto>(userShift);
        }

        public async Task<bool> AssignShiftToUserAsync(string userId, string shiftId)
        {
            return await _userRepository.AssignShiftToUserAsync(userId, shiftId);
        }

        public async Task<bool> RemoveShiftFromUserAsync(string userId, string shiftId)
        {
            return await _userRepository.RemoveShiftFromUserAsync(userId, shiftId);
        }

        public async Task<PagedResult<UserReadDto>> GetPagedAsync(PaginationParameters parameters)
        {
            var pagedUsers = await _userRepository.GetPagedAsync(parameters);
            var userDtos = _mapper.Map<IEnumerable<UserReadDto>>(pagedUsers.Items);

            // Map roles from eagerly loaded data
            foreach (var userDto in userDtos)
            {
                var user = pagedUsers.Items.FirstOrDefault(u => u.Id == userDto.Id);
                if (user?.UserRoles != null)
                {
                    userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                }
            }

            return new PagedResult<UserReadDto>
            {
                Items = userDtos,
                Page = pagedUsers.Page,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount
            };
        }

        public async Task<PagedResult<UserReadDto>> GetDeletedPagedAsync(PaginationParameters parameters)
        {
            var pagedUsers = await _userRepository.GetDeletedPagedAsync(parameters);
            var userDtos = _mapper.Map<IEnumerable<UserReadDto>>(pagedUsers.Items);

            // Map roles from eagerly loaded data
            foreach (var userDto in userDtos)
            {
                var user = pagedUsers.Items.FirstOrDefault(u => u.Id == userDto.Id);
                if (user?.UserRoles != null)
                {
                    userDto.Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                }
            }

            return new PagedResult<UserReadDto>
            {
                Items = userDtos,
                Page = pagedUsers.Page,
                PageSize = pagedUsers.PageSize,
                TotalCount = pagedUsers.TotalCount
            };
        }

        public async Task<bool> UpdateUserActiveStatusAsync(string userId, bool isActive)
        {
            return await _userRepository.UpdateUserActiveStatusAsync(userId, isActive);
        }
    }

