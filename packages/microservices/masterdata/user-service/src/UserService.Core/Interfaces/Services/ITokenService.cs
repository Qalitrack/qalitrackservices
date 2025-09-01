using UserService.Core.DTOs.User;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces.Services
{
    public interface ITokenService
    {
        Task<PersonalAccessToken> GenerateTokenForAuthenticatedUserAsync(UserReadDto user);
        Task<bool> ValidateTokenAsync(string token);
        Task<Guid?> GetUserIdFromTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> DeleteAllTokensForUserAsync(Guid userId);
    }
}
