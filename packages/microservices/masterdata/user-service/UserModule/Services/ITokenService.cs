using UserModule.Models;

namespace UserModule.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
        Task<(bool IsValid, Guid UserId)> ValidateTokenAsync(string token);
        Task<bool> HasRoleAsync(string token, string role);
    }
}