using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Core.DTOs;
using UserService.Core.Entities;
using UserService.Core.Interfaces;

namespace UserService.Core.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _maxFailedAttempts;
    private readonly int _lockoutMinutes;

    public AuthenticationService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IJwtService jwtService,
        IPasswordService passwordService,
        IEmailService emailService,
        IMapper mapper,
        ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _jwtService = jwtService;
        _passwordService = passwordService;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
        _configuration = configuration;
        _maxFailedAttempts = int.Parse(_configuration["Security:MaxFailedAttempts"] ?? "5");
        _lockoutMinutes = int.Parse(_configuration["Security:LockoutMinutes"] ?? "30");
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username) ??
                      await _userRepository.GetByEmailAsync(request.Username);

            if (user == null)
            {
                _logger.LogWarning("Login attempt with invalid username/email: {Username}", request.Username);
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            // Check if user is locked
            if (await IsUserLockedAsync(user.Id))
            {
                _logger.LogWarning("Login attempt for locked user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Account is locked due to multiple failed attempts");
            }

            // Validate password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                await HandleFailedLoginAsync(user);
                _logger.LogWarning("Failed login attempt for user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Invalid username or password");
            }

            // Check user status
            if (user.Status != UserStatus.Active)
            {
                _logger.LogWarning("Login attempt for inactive user: {UserId}", user.Id);
                throw new UnauthorizedAccessException("Account is not active");
            }

            // Successful login
            await _userRepository.UpdateLastLoginAsync(user.Id);
            await _userRepository.SaveChangesAsync();

            // Generate JWT claims
            var claims = await GetUserClaimsAsync(user.Id);
            
            // Generate tokens
            var accessToken = await _jwtService.GenerateAccessTokenAsync(claims);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync();

            // Create session
            var session = new UserSession
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                SessionId = Guid.NewGuid().ToString(),
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _sessionRepository.AddAsync(session);
            await _sessionRepository.SaveChangesAsync();

            var userDto = _mapper.Map<UserDto>(user);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = await _jwtService.GetTokenExpirationAsync(accessToken),
                User = userDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            throw;
        }
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if username/email already exists
            if (!await _userRepository.IsUsernameAvailableAsync(request.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            if (!await _userRepository.IsEmailAvailableAsync(request.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            // Create user
            var user = _mapper.Map<User>(request);
            user.Id = Guid.NewGuid().ToString();
            user.PasswordHash = _passwordService.HashPassword(request.Password);
            user.Status = UserStatus.Pending;
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            // Assign default role
            var defaultRoles = await _roleRepository.GetDefaultRolesAsync();
            foreach (var role in defaultRoles)
            {
                var userRole = new UserRole
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                user.UserRoles.Add(userRole);
            }

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Send email confirmation
            await SendEmailConfirmationAsync(user.Id);

            return new RegisterResponseDto
            {
                UserId = user.Id,
                Message = "Registration successful. Please check your email to confirm your account.",
                RequiresEmailConfirmation = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
            throw;
        }
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var session = await _sessionRepository.GetActiveSessionByRefreshTokenAsync(request.RefreshToken);
            if (session == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = await _userRepository.GetByIdAsync(session.UserId);
            if (user == null || user.Status != UserStatus.Active)
            {
                throw new UnauthorizedAccessException("User not found or inactive");
            }

            // Generate new tokens
            var claims = await GetUserClaimsAsync(user.Id);
            var accessToken = await _jwtService.GenerateAccessTokenAsync(claims);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync();

            // Update session
            session.RefreshToken = refreshToken;
            session.UpdatedAt = DateTime.UtcNow;
            session.LastAccessedAt = DateTime.UtcNow;

            _sessionRepository.Update(session);
            await _sessionRepository.SaveChangesAsync();

            return new RefreshTokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = await _jwtService.GetTokenExpirationAsync(accessToken)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }

    public async Task<bool> LogoutAsync(LogoutRequestDto request)
    {
        try
        {
            var session = await _sessionRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (session != null)
            {
                await _sessionRepository.DeactivateSessionAsync(session.SessionId);
                await _sessionRepository.SaveChangesAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return false;
            }

            if (!_passwordService.IsPasswordValid(request.NewPassword))
            {
                return false;
            }

            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
            await _userRepository.SaveChangesAsync();

            // Send confirmation email
            await _emailService.SendPasswordChangedEmailAsync(user.Email, user.FirstName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal if email exists
                return true;
            }

            var token = await _passwordService.GeneratePasswordResetTokenAsync(user.Id);
            var resetLink = $"{_configuration["App:BaseUrl"]}/reset-password?token={token}&userId={user.Id}";
            
            await _emailService.SendPasswordResetAsync(user.Email, resetLink);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset for email: {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> ResetPasswordConfirmAsync(ResetPasswordConfirmDto request)
    {
        try
        {
            if (!await _passwordService.ValidatePasswordResetTokenAsync(request.Token, request.Email))
            {
                return false;
            }

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                return false;
            }

            if (!_passwordService.IsPasswordValid(request.NewPassword))
            {
                return false;
            }

            await _passwordService.ResetPasswordAsync(user.Id, request.Token, request.NewPassword);
            
            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            await _userRepository.UpdatePasswordAsync(user.Id, newPasswordHash);
            await _userRepository.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming password reset for email: {Email}", request.Email);
            return false;
        }
    }

    public async Task<bool> ConfirmEmailAsync(ConfirmEmailDto request)
    {
        try
        {
            // In a real implementation, you would validate the token
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return false;
            }

            user.EmailConfirmed = true;
            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email for user: {UserId}", request.UserId);
            return false;
        }
    }

    public async Task<bool> SendEmailConfirmationAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var confirmationLink = $"{_configuration["App:BaseUrl"]}/confirm-email?userId={userId}&token={Guid.NewGuid()}";
            await _emailService.SendEmailConfirmationAsync(user.Email, confirmationLink);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email confirmation for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username) ??
                  await _userRepository.GetByEmailAsync(username);

        return user != null && 
               _passwordService.VerifyPassword(password, user.PasswordHash) &&
               user.Status == UserStatus.Active;
    }

    public async Task<bool> IsUserLockedAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.LockoutEnd.HasValue == true && user.LockoutEnd > DateTime.UtcNow;
    }

    public async Task<bool> UnlockUserAsync(string userId)
    {
        try
        {
            await _userRepository.UnlockUserAsync(userId);
            await _userRepository.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
            return false;
        }
    }

    public async Task<JwtClaimsDto> GetUserClaimsAsync(string userId)
    {
        var user = await _userRepository.GetWithRolesAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var permissions = await _permissionRepository.GetUserPermissionsAsync(userId);
        var organizations = user.OrganizationUsers.Where(ou => ou.Status == OrganizationUserStatus.Active)
                                                 .Select(ou => ou.OrganizationId)
                                                 .ToList();

        return new JwtClaimsDto
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.UserRoles.Where(ur => ur.IsActive).Select(ur => ur.Role.Name).ToList(),
            Permissions = permissions.Select(p => p.Name).ToList(),
            Organizations = organizations
        };
    }

    private async Task HandleFailedLoginAsync(User user)
    {
        user.FailedLoginAttempts++;
        user.UpdatedAt = DateTime.UtcNow;

        if (user.FailedLoginAttempts >= _maxFailedAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
            user.Status = UserStatus.Locked;
            
            await _emailService.SendAccountLockedEmailAsync(user.Email, user.FirstName);
        }

        await _userRepository.UpdateFailedLoginAttemptsAsync(user.Id, user.FailedLoginAttempts);
        if (user.LockoutEnd.HasValue)
        {
            await _userRepository.LockUserAsync(user.Id, user.LockoutEnd.Value);
        }
        await _userRepository.SaveChangesAsync();
    }
}