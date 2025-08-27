using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Core.Interfaces;
using Serilog;
using UserService.Core.DTOs.User;
using UserService.Core.Interfaces.Repositories;

namespace UserService.Core.Services
{
    public class TokenService( IJwtConfigurationService jwtConfigService, ITokenRepository tokenRepository)
        : ITokenService
    {
        public async Task<PersonalAccessToken> GenerateTokenForAuthenticatedUserAsync(UserReadDto user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await GenerateTokenForUserAsync(user);
        }

/* <<<<<<<<<<<<<<  ✨ Windsurf Command ⭐ >>>>>>>>>>>>>>>> */
/// <summary>
/// Generates a JSON Web Token (JWT) for a given authenticated user, including user claims and roles.
/// </summary>
/// <param name="user">The user object containing user details and roles.</param>
/// <returns>A <see cref="PersonalAccessToken"/> containing the generated JWT and associated metadata.</returns>
/// <exception cref="ArgumentNullException">Thrown when the user is null.</exception>
/// <exception cref="InvalidOperationException">Thrown when saving the token to the database fails.</exception>

/* <<<<<<<<<<  7f9f5679-c25e-4668-80d2-e86827675583  >>>>>>>>>>> */
       private async Task<PersonalAccessToken> GenerateTokenForUserAsync(UserReadDto user)
            {
                var jti = Guid.NewGuid().ToString(); // Generate unique token ID

                // Use List<Claim> instead of array for easier manipulation
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, jti),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Email),
                };

                // Log user's roles before adding to claims
                if (user.Roles != null)
                {
                    foreach (var role in user.Roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
                
                var secretKey = jwtConfigService.GetSecretKey();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var issuer = jwtConfigService.GetIssuer();
                var audience = jwtConfigService.GetAudience();
                var expiration = jwtConfigService.GetTokenExpiration();
                
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.Add(expiration),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Log the final token's role claims for verification
                var decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);
                var tokenRoleClaims = decodedToken.Claims
                    .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                    .ToList();

                var personalAccessToken = new PersonalAccessToken
                {
                    Token = tokenString,
                    UserId = user.Id,
                    Jti = jti,
                    IsRevoked = false,
                };

                try
                {
                    var savedToken = await tokenRepository.CreateAsync(personalAccessToken);
                    return savedToken;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to save token to database for user {UserId}", user.Id);
                    throw new InvalidOperationException("Failed to create token", ex);
                }
            }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = jwtConfigService.GetSecretKey();
                
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var issuer = jwtConfigService.GetIssuer();
                var audience = jwtConfigService.GetAudience();
                
                var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = key
                });

                if (!validationResult.IsValid)
                {
                    Log.Warning($"Token validation failed: {validationResult.Exception?.Message}");
                    return false;
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                {
                    Log.Warning("Token missing jti claim");
                    return false;
                }

                var dbToken = await tokenRepository.GetTokenByJtiAsync(jti);
                if (dbToken == null || dbToken.IsRevoked)
                {
                    Log.Warning("Token not found or revoked for jti {Jti}", jti);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Token validation failed");
                return false;
            }
        }

        public Task<Guid?> GetUserIdFromTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult<Guid?>(null);

            try
            {
                token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    return Task.FromResult<Guid?>(userId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error parsing token for user ID");
            }

            return Task.FromResult<Guid?>(null);
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            try
            {
                token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                    return false;

                var dbToken = await tokenRepository.GetTokenByJtiAsync(jti);
                if (dbToken != null)
                {
                    dbToken.IsRevoked = true;
                    await tokenRepository.UpdateAsync(dbToken);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error revoking token");
                return false;
            }
        }
        

        public async Task<bool> DeleteAllTokensForUserAsync(Guid userId)
        {
            return await tokenRepository.DeleteAllTokensForUserAsync(userId);
        }
    }
}
