using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UserService.Core.DTOs.Shift;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;
        private readonly IUserShiftRepository _userShiftRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public ShiftService(
            IShiftRepository shiftRepository,
            IUserShiftRepository userShiftRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _shiftRepository = shiftRepository ?? throw new ArgumentNullException(nameof(shiftRepository));
            _userShiftRepository = userShiftRepository ?? throw new ArgumentNullException(nameof(userShiftRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ShiftDto>> GetAllAsync()
        {
            var shifts = await _shiftRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShiftDto>>(shifts);
        }

        public async Task<ShiftDto> GetByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            return _mapper.Map<ShiftDto>(shift);
        }

        public async Task<ShiftDto> CreateAsync(CreateShiftDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate shift times
            if (dto.StartTime >= dto.EndTime)
                throw new ValidationException("End time must be after start time");

            var shift = _mapper.Map<Shift>(dto);
            var createdShift = await _shiftRepository.CreateAsync(shift);
            return _mapper.Map<ShiftDto>(createdShift);
        }

        public async Task<ShiftDto?> UpdateAsync(string id, UpdateShiftDto dto)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));
                
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            // Validate shift times
            if (dto.StartTime >= dto.EndTime)
                throw new ValidationException("End time must be after start time");

            var existingShift = await _shiftRepository.GetByIdAsync(id);
            if (existingShift == null)
                throw new Exception("Shift not found");

            // Update properties
            existingShift.Name = dto.Name;
            existingShift.Description = dto.Description;
            existingShift.StartTime = dto.StartTime.TimeOfDay;
            existingShift.EndTime = dto.EndTime.TimeOfDay;
            existingShift.Mode = dto.Mode; // dto.Mode;
            existingShift.UpdatedAt = DateTime.UtcNow;

            var updatedShift = await _shiftRepository.UpdateAsync(existingShift);
            return _mapper.Map<ShiftDto>(updatedShift);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Shift ID is required", nameof(id));

            // Check if shift exists
            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null)
                throw new Exception("Shift not found");

            // Check if any users are assigned to this shift
            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(id);
            if (userShifts.Any())
                throw new ValidationException("Cannot delete shift with assigned users");

            return await _shiftRepository.DeleteAsync(id);
        }

        public async Task<bool> IsShiftActive(string shiftId)
        {
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
                throw new Exception("Shift not found");

            return await _shiftRepository.IsShiftActiveAsync(shiftId);
        }

        public async Task<bool> AssignUserToShiftAsync(string userId, string shiftId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
                
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            // Check if user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // Check if shift exists
            var shift = await _shiftRepository.GetByIdAsync(shiftId);
            if (shift == null)
                throw new Exception("Shift not found");

            // Check if user is already assigned to this shift
            var existingAssignment = (await _userShiftRepository.GetShiftsForUserAsync(userId))
                .FirstOrDefault(us => us.ShiftId == shiftId);
                
            if (existingAssignment != null)
                throw new ValidationException("User is already assigned to this shift");

            // Create new user-shift assignment
            var userShift = new UserShift
            {
                UserId = userId,
                ShiftId = shiftId,
                AssignedAt = DateTime.UtcNow
            };

            var result = await _userShiftRepository.CreateAsync(userShift);
            return result != null;
        }

        public async Task<bool> RemoveUserFromShiftAsync(string userId, string shiftId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID is required", nameof(userId));
                
            if (string.IsNullOrEmpty(shiftId))
                throw new ArgumentException("Shift ID is required", nameof(shiftId));

            // Check if the assignment exists
            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(userId);
            var userShift = userShifts.FirstOrDefault(us => us.ShiftId == shiftId);
            
            if (userShift == null)
                throw new Exception("User is not assigned to this shift");

            return await _userShiftRepository.DeleteAsync(userShift.Id);
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

            // Verify password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Invalid password
                return null;
            }

            // Check if user is assigned to any active shift
            var userShifts = await _userShiftRepository.GetShiftsForUserAsync(user.Id);
            var hasActiveShift = userShifts.Any(us => 
            {
                var shift = _shiftRepository.GetByIdAsync(us.ShiftId).Result;
                return shift != null && shift.IsActive;
            });

            if (!hasActiveShift)
            {
                return null;
            }

            return user;
        }
    }
}