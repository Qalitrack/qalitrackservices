namespace UserService.Core.Interfaces.Repositories
{
    public interface ITokenRepository: IRepository<PersonalAccessToken>
    {
        new Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token);
        
        /// <summary>
        /// Deletes all tokens for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if any tokens were deleted, false otherwise</returns>
        Task<bool> DeleteAllTokensForUserAsync(Guid userId);

        Task<PersonalAccessToken?> GetTokenByJtiAsync(string jti);
    }
}