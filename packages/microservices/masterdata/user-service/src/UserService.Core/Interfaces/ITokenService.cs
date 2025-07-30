using System;
using System.Threading.Tasks;
using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface ITokenService
    {
        Task<PersonalAccessToken> GenerateTokenAsync(string email, string password);
        Task<PersonalAccessToken> GenerateTokenForAuthenticatedUserAsync(User user);
        Task<bool> ValidateTokenAsync(string token);
        Task<Guid?> GetUserIdFromTokenAsync(string token);
        Task<bool> RevokeTokenAsync(string token);
        Task<bool> DeleteAllTokensForUserAsync(Guid userId);
    }
}