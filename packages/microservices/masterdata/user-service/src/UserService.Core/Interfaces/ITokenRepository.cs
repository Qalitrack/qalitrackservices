using UserService.Core.Entities;

namespace UserService.Core.Interfaces
{
    public interface ITokenRepository: IRepository<PersonalAccessToken>
    {
        Task<PersonalAccessToken?> GetTokenByUserIdAsync(Guid userId);
        Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token);
        Task<bool> RevokeTokenAsync(Guid userId);
        Task<bool> DeleteTokenAsync(Guid userId);
    }
}