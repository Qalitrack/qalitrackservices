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
using Serilog;

namespace UserService.Infrastructure.Repositories
{
    public class TokenRepository : Repository<PersonalAccessToken>, ITokenRepository
    {
        private readonly UserServiceDbContext _dbContext;

        public TokenRepository(UserServiceDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // Retrieve token by userId (with no revocation)
        public async Task<PersonalAccessToken?> GetTokenByUserIdAsync(Guid userId)
        {
            string userIdString = userId.ToString();

            var token = await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.UserId == userIdString && !t.IsRevoked);
            return token;
        }

        // Create a new token and save to the database
        public async Task<PersonalAccessToken?> GetByIdAsync(string id)
        {
            return await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.Id == id);  
            
        }

        public async Task<PersonalAccessToken> CreateAsync(PersonalAccessToken token)
        {
            try
            {
                // Check if a token already exists for this user
                var existingToken = await _dbContext.PersonalAccessTokens
                    .FirstOrDefaultAsync(t => t.UserId == token.UserId);
                
                // Ensure UserId is properly set as a string
                if (token.UserId == null)
                {
                    throw new ArgumentNullException(nameof(token.UserId), "UserId cannot be null");
                }

                if (existingToken != null)
                {
                    existingToken.IsRevoked = true;
                    existingToken.UpdatedAt = DateTime.UtcNow;
                }

                // Ensure the new token is not marked as revoked
                token.IsRevoked = false;
                token.CreatedAt = DateTime.UtcNow;
                
                await _dbContext.PersonalAccessTokens.AddAsync(token);
                int changes = await _dbContext.SaveChangesAsync();
                return token;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            return await _dbContext.PersonalAccessTokens
                .Where(t => t.Id == id)
                .ExecuteDeleteAsync() > 0;
        }

        // Revoke token by userId
        public async Task<bool> RevokeTokenAsync(Guid userId)
        {
            string userIdString = userId.ToString();

            var token = await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.UserId == userIdString && !t.IsRevoked);

            if (token == null)
            {
                // Check if there are any tokens for this user at all
                var anyToken = await _dbContext.PersonalAccessTokens
                    .AnyAsync(t => t.UserId == userIdString);
                
                if (anyToken)
                {
                    return true; // Consider all tokens already revoked as success
                }
                return false;
            }

            try
            {
                token.IsRevoked = true;
                token.UpdatedAt = DateTime.UtcNow;
                int changes = await _dbContext.SaveChangesAsync();
                return changes > 0;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Delete token by userId
        public async Task<bool> DeleteTokenAsync(Guid userId)
        {
            string userIdString = userId.ToString();

            var token = await _dbContext.PersonalAccessTokens
                .FirstOrDefaultAsync(t => t.UserId == userIdString && !t.IsRevoked);

            if (token == null)
            {
                return false;
            }

            _dbContext.PersonalAccessTokens.Remove(token);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        // Optional: Method to get a user based on token info
        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);  // Parse the token

                var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    // Convert the userId from string to Guid
                    var userId = Guid.Parse(userIdClaim.Value);

                    // Retrieve the user from the database based on userId
                    var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId));

                    if (user != null)
                    {
                        return user; // Return the user entity
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }

            return null; // Return null if user was not found or there was an error
        }

        // Optional: Method to get all tokens (for administrative purposes)
        public async Task<IEnumerable<PersonalAccessToken>> GetAllAsync()
        {

            var tokens = await _dbContext.PersonalAccessTokens
                .Where(t => !t.IsRevoked) // Only get non-revoked tokens
                .ToListAsync();


            return tokens;
        }
    }
}
