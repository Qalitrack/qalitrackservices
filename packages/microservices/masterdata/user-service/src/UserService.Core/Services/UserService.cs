using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.DTOs.Shift;
using UserService.Core.DTOs.User;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserReadDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserReadDto>>(users);
        }

        public async Task<UserReadDto?> GetByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? null : _mapper.Map<UserReadDto>(user);
        }

        public async Task<UserReadDto> CreateAsync(CreateUserDto dto)
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = _mapper.Map<User>(dto);
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            
            // Set the hashed password
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            
            var createdUser = await _userRepository.CreateAsync(user);
            return _mapper.Map<UserReadDto>(createdUser);
        }

        public async Task<UserReadDto?> UpdateAsync(string id, UpdateUserDto dto)
        {
            var existingUser = await _userRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return null;
            }

            _mapper.Map(dto, existingUser);
            existingUser.UpdatedAt = DateTime.UtcNow;
            
            var updatedUser = await _userRepository.UpdateAsync(existingUser);
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
            if (user == null)
            {
                // User not found
                return null;
            }

            // Verify password using the User entity's method
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Invalid password
                return null;
            }

            // Update last login time
            await _userRepository.UpdateAsync(user);

            return user;
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
    }
}
