using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Core.Entities;

namespace UserService.Core.Services;

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
        var user = _mapper.Map<User>(dto);
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        
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

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        return await _userRepository.IsNameAvailableAsync(name);
    }
}