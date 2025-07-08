using AutoMapper;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class UserServiceImpl : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IMapper _mapper;

    public UserServiceImpl(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _mapper = mapper;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<UserDto?> GetCompleteUserAsync(string userId)
    {
        var user = await _userRepository.GetCompleteUserAsync(userId);
        return user != null ? _mapper.Map<UserDto>(user) : null;
    }

    public async Task<PaginatedResponseDto<UserDto>> GetUsersAsync(PaginationRequestDto request)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(
            request.Page,
            request.PageSize);

        var userDtos = _mapper.Map<List<UserDto>>(users);

        return new PaginatedResponseDto<UserDto>
        {
            Data = userDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            HasNextPage = request.Page * request.PageSize < totalCount,
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<PaginatedResponseDto<UserDto>> GetUsersByOrganizationAsync(string organizationId, PaginationRequestDto request)
    {
        var users = await _userRepository.GetByOrganizationAsync(organizationId);
        var userDtos = _mapper.Map<List<UserDto>>(users);

        var pagedUsers = userDtos
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PaginatedResponseDto<UserDto>
        {
            Data = pagedUsers,
            TotalCount = userDtos.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)userDtos.Count / request.PageSize),
            HasNextPage = request.Page * request.PageSize < userDtos.Count,
            HasPreviousPage = request.Page > 1
        };
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto request)
    {
        var user = _mapper.Map<User>(request);
        user.Id = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> UpdateUserAsync(string userId, UpdateUserDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _mapper.Map(request, user);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivateUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Inactive;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SuspendUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Status = UserStatus.Suspended;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userRepository.GetWithProfileAsync(userId);
        return user?.Profile != null ? _mapper.Map<UserProfileDto>(user.Profile) : null;
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileDto request)
    {
        var user = await _userRepository.GetWithProfileAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Profile == null)
        {
            user.Profile = new UserService.Core.Entities.UserProfile
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        _mapper.Map(request, user.Profile);
        user.Profile.UpdatedAt = DateTime.UtcNow;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(user.Profile);
    }

    public async Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(string userId)
    {
        var user = await _userRepository.GetWithRolesAsync(userId);
        return user?.UserRoles != null ? _mapper.Map<IEnumerable<UserRoleDto>>(user.UserRoles) : new List<UserRoleDto>();
    }

    public async Task<bool> AssignUserRoleAsync(AssignUserRoleDto request)
    {
        // Implementation would go here
        return await Task.FromResult(true);
    }

    public async Task<bool> RemoveUserRoleAsync(RemoveUserRoleDto request)
    {
        // Implementation would go here
        return await Task.FromResult(true);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username)
    {
        return await _userRepository.IsUsernameAvailableAsync(username);
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return await _userRepository.IsEmailAvailableAsync(email);
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(string userId)
    {
        var sessions = await _sessionRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<UserSessionDto>>(sessions);
    }

    public async Task<bool> RevokeUserSessionAsync(string userId, string sessionId)
    {
        await _sessionRepository.DeactivateSessionAsync(sessionId);
        await _sessionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeAllUserSessionsAsync(string userId)
    {
        await _sessionRepository.DeactivateAllUserSessionsAsync(userId);
        await _sessionRepository.SaveChangesAsync();
        return true;
    }
}