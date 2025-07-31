using Microsoft.EntityFrameworkCore;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using UserService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace UserService.Infrastructure.Repositories
{
    public class TokenRepository : Repository<PersonalAccessToken>, ITokenRepository
    {
        private readonly UserServiceDbContext _dbContext;
        private readonly ILogger<TokenRepository> _logger;

        public TokenRepository(UserServiceDbContext dbContext, ILogger<TokenRepository> logger)
            : base(dbContext)
        {
            _dbContext = dbContext;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Retrieve token by userId (non-revoked)
        public async Task<PersonalAccessToken?> GetTokenByUserIdAsync(Guid userId)
        {
            string userIdString = userId.ToString();

            return await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.UserId == userIdString && !t.IsRevoked);
        }

        // Retrieve token by ID
        public async Task<PersonalAccessToken?> GetByIdAsync(string id, bool b)
        {
            return await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        // Create a new token and save it
        public async Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token)
        {
            try
            {
                if (string.IsNullOrEmpty(token.UserId))
                    throw new ArgumentNullException(nameof(token.UserId), "UserId cannot be null or empty");

                // Revoke all previous tokens for the user before creating the new one
                var existingTokens = await _dbContext.PersonalAccessTokens
                    .Where(t => t.UserId == token.UserId && !t.IsRevoked)
                    .ToListAsync();

                foreach (var existing in existingTokens)
                {
                    existing.IsRevoked = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                // Set up the new token
                token.IsRevoked = false;
                token.CreatedAt = DateTime.UtcNow;
                token.UpdatedAt = DateTime.UtcNow;

                await _dbContext.PersonalAccessTokens.AddAsync(token);
                int changes = await _dbContext.SaveChangesAsync();

                if (changes > 0)
                {
                    _logger.LogInformation("Created new token for user {UserId}", token.UserId);
                    return token;
                }

                throw new InvalidOperationException("Failed to save token to database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating token for user {UserId}", token.UserId);
                throw;
            }
        }

        // Delete token by ID
        public async Task<bool> DeleteAsync(string id)
        {
            return await _dbContext.PersonalAccessTokens
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync() > 0;
        }

        // Revoke token by userId
        // public async Task<bool> RevokeTokenAsync(Guid userId)
        // {
        //     string userIdString = userId.ToString();
        //
        //     try
        //     {
        //         // Find non-revoked tokens
        //         var tokens = await _dbContext.PersonalAccessTokens
        //             .Where(t => t.UserId == userIdString && !t.IsRevoked)
        //             .ToListAsync();
        //
        //         if (!tokens.Any())
        //         {
        //             var anyToken = await _dbContext.PersonalAccessTokens
        //                 .AnyAsync(t => t.UserId == userIdString);
        //
        //             if (anyToken)
        //             {
        //                 _logger.LogInformation("All tokens already revoked for user {UserId}", userId);
        //                 return true;
        //             }
        //
        //             _logger.LogWarning("No tokens found for user {UserId}", userId);
        //             return false;
        //         }
        //
        //         // Revoke all active tokens
        //         foreach (var token in tokens)
        //         {
        //             token.IsRevoked = true;
        //             token.UpdatedAt = DateTime.UtcNow;
        //         }
        //
        //         int changes = await _dbContext.SaveChangesAsync();
        //         _logger.LogInformation("Revoked {Count} tokens for user {UserId}", tokens.Count, userId);
        //         return changes > 0;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error revoking tokens for user {UserId}", userId);
        //         return false;
        //     }
        // }

        // Delete all tokens for user
        public async Task<bool> DeleteTokenAsync(Guid userId)
        {
            string userIdString = userId.ToString();

            try
            {
                var tokens = await _dbContext.PersonalAccessTokens
                    .Where(t => t.UserId == userIdString)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    _logger.LogInformation("No tokens found to delete for user {UserId}", userId);
                    return false;
                }

                _dbContext.PersonalAccessTokens.RemoveRange(tokens);
                int changes = await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} tokens for user {UserId}", tokens.Count, userId);
                return changes > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tokens for user {UserId}", userId);
                return false;
            }
        }

        // Retrieve user based on token
        // public async Task<User?> GetUserFromTokenAsync(string token)
        // {
        //     try
        //     {
        //         var tokenHandler = new JwtSecurityTokenHandler();
        //         var jwtToken = tokenHandler.ReadJwtToken(token);
        //
        //         var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        //         if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        //         {
        //             return await _dbContext.Users.FirstOrDefaultAsync(u => Equals(u.Id, userId));
        //         }
        //
        //         _logger.LogWarning("No valid user ID found in token");
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error getting user from token");
        //     }
        //
        //     return null;
        // }

        // Get all non-revoked tokens
        public async Task<IEnumerable<PersonalAccessToken>> GetAllAsync()
        {
            return await _dbContext.PersonalAccessTokens
                .Where(t => !t.IsRevoked)
                .ToListAsync();
        }

        // Delete all tokens for user
        public async Task<bool> DeleteAllTokensForUserAsync(Guid userId)
        {
            try
            {
                var userIdString = userId.ToString();
                var tokens = await _dbContext.PersonalAccessTokens
                    .Where(t => t.UserId == userIdString)
                    .ToListAsync();

                if (!tokens.Any())
                {
                    _logger.LogInformation("No tokens found for user {UserId}", userId);
                    return false;
                }

                _dbContext.PersonalAccessTokens.RemoveRange(tokens);
                var changes = await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} tokens for user {UserId}", tokens.Count, userId);
                return changes > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tokens for user {UserId}", userId);
                return false;
            }
        }

        // Retrieve token by JTI (JWT ID) and check if revoked
        public async Task<PersonalAccessToken?> GetTokenByJtiAsync(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti))
                return null;

            try
            {
                return await _dbContext.PersonalAccessTokens
                    .FirstOrDefaultAsync(t => t.Jti == jti && !t.IsRevoked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve token by JTI: {Jti}", jti);
                return null;
            }
        }
    }
}
