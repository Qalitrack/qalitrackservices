using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<bool> LogoutAsync(LogoutRequestDto request);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<bool> ResetPasswordConfirmAsync(ResetPasswordConfirmDto request);
    Task<bool> ConfirmEmailAsync(ConfirmEmailDto request);
    Task<bool> SendEmailConfirmationAsync(string userId);
    Task<bool> ValidateUserAsync(string username, string password);
    Task<bool> IsUserLockedAsync(string userId);
    Task<bool> UnlockUserAsync(string userId);
    Task<JwtClaimsDto> GetUserClaimsAsync(string userId);
}