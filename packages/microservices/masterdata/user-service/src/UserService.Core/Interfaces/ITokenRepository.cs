using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface ITokenRepository: IRepository<PersonalAccessToken>
    {
        Task<PersonalAccessToken?> GetTokenByUserIdAsync(Guid userId);
        Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token);
        Task<bool> RevokeTokenAsync(Guid userId);
        Task<bool> DeleteTokenAsync(Guid userId);
        
        /// <summary>
        /// Deletes all tokens for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if any tokens were deleted, false otherwise</returns>
        Task<bool> DeleteAllTokensForUserAsync(Guid userId);

        Task<PersonalAccessToken> GetTokenByJtiAsync(string jti);
    }
}