using UserService.Core.Interfaces;

namespace UserService.Core.Mappings
{
    public interface ITokenRepository: IRepository<PersonalAccessToken>
    {
        Task<PersonalAccessToken?> GetTokenByUserIdAsync(Guid userId);
        Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token);
        Task<bool> RevokeTokenAsync(Guid userId);
    }
}