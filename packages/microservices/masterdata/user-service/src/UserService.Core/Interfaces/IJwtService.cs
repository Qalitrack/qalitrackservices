using UserService.Core.DTOs;

namespace UserService.Core.Interfaces;

public interface IJwtService
{
    Task<string> GenerateAccessTokenAsync(JwtClaimsDto claims);
    Task<string> GenerateRefreshTokenAsync();
    Task<JwtClaimsDto?> ValidateTokenAsync(string token);
    Task<bool> IsTokenValidAsync(string token);
    Task<bool> IsRefreshTokenValidAsync(string refreshToken);
    Task<DateTime> GetTokenExpirationAsync(string token);
    Task<string?> GetUserIdFromTokenAsync(string token);
    Task<IEnumerable<string>> GetRolesFromTokenAsync(string token);
    Task<IEnumerable<string>> GetPermissionsFromTokenAsync(string token);
    Task<string?> GetClaimFromTokenAsync(string token, string claimType);
    Task<bool> RevokeTokenAsync(string token);
}