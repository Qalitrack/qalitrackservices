using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Core.Entities;
using UserService.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Serilog;

namespace UserService.Core.Services
{
    public class TokenService(IUserService userService, IConfiguration configuration, ITokenRepository tokenRepository)
        : ITokenService
    {
        // Existing method to generate JWT token
        public async Task<PersonalAccessToken> GenerateTokenAsync(string email, string password)
        {
            var user = await userService.ValidateUserCredentials(email, password);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = configuration["Jwt:SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: null,  // No expiration time set
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new PersonalAccessToken
            {
                Token = tokenString,
                UserId = user.Id,
                IsRevoked = false
            };
        }

        // Existing method to validate JWT token
        public async Task<bool> ValidateTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = false,  // If you want to skip lifetime validation or handle it separately
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero  // No clock skew allowed (for precise expiration time checking)
                }, out var validatedToken);

                // If the token is valid, return true
                return await Task.FromResult(validatedToken != null);
            }
            catch (SecurityTokenExpiredException)
            {
                // Handle expired token
                Log.Error("Token has expired.");
                return await Task.FromResult(false);
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                // Handle invalid issuer
                Log.Error("Invalid token issuer.");
                return await Task.FromResult(false);
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                // Handle invalid audience
                Log.Error("Invalid token audience.");
                return await Task.FromResult(false);
            }
            catch (SecurityTokenException ex)
            {
                // Handle general token validation failure
                Log.Error($"Token validation failed: {ex.Message}");
                return await Task.FromResult(false);
            }
            catch (Exception ex)
            {
                // Log any unexpected errors
                Log.Error($"An error occurred while validating the token: {ex.Message}");
                return await Task.FromResult(false);
            }
        }


        async Task<Guid?> ITokenService.GetUserIdFromTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ReadJwtToken(token);
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim.Value);
                return userId;
            }
            return null;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ReadJwtToken(token);
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                var userId = Guid.Parse(userIdClaim.Value);
                return await tokenRepository.RevokeTokenAsync(userId);
            }
            return false;
        }

        public async Task<Guid?> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                // If it's not a valid JWT token
                if (jwtToken == null)
                {
                    return null;
                }

                // Extract the user ID (which is typically stored in the NameIdentifier claim)
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId; // Return the user ID
                }

                return null; // If user ID is not found or invalid
            }
            catch (Exception)
            {
                // Log the error if needed (you could use Serilog or other logging mechanism)
                return null; // Return null if there's an error
            }
        }


        // New method to revoke and delete token
        public async Task<bool> RevokeAndDeleteTokenAsync(Guid userId)
        {
            // Step 1: Retrieve the token from the repository
            var token = await tokenRepository.GetTokenByUserIdAsync(userId);

            // Step 2: Mark the token as revoked
            if (token != null)
            {
                token.IsRevoked = true;
                await tokenRepository.UpdateAsync(token); // Assuming UpdateAsync persists changes

                // Step 3: Delete the token from the repository
                var deleteSuccess = await tokenRepository.DeleteTokenAsync(userId);
                return deleteSuccess;
            }

            return false; // Token not found or could not be revoked
        }
    }
}
