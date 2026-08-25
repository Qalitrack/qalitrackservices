using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Core.Entities;

namespace UserService.Infrastructure.Data.Configurations;

public class PersonalAccessTokenConfiguration : IEntityTypeConfiguration<PersonalAccessToken>
{
    public void Configure(EntityTypeBuilder<PersonalAccessToken> builder)
    {
        builder.HasKey(e => e.Id);
        // TokenRepository.GetTokenByJtiAsync runs on every authorized request
        // (via PermissionAuthorizationHandler -> TokenService.ValidateTokenAsync)
        // filtering on Jti — previously unindexed, forcing a full scan of a
        // table that only ever grows (a row per login, never deleted).
        builder.HasIndex(e => e.Jti).IsUnique();
    }
}
