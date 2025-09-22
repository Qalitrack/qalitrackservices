using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Core.Interfaces;
using Serilog;
using UserService.Core.DTOs.User;
using UserService.Core.Interfaces.Repositories;
using UserService.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using UserService.Core.Entities;

namespace UserService.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly TimeSpan _expiration;
        private readonly ITokenRepository _tokenRepository;

        public TokenService(IConfiguration configuration, ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
            
            _secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ??
                        configuration["JWT_SECRET_KEY"] ??
                        configuration["JwtSettings:SecretKey"] ??
                        throw new InvalidOperationException("JWT Secret Key is not configured.");

            _issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ??
                     configuration["JWT_ISSUER"] ??
                     configuration["JwtSettings:Issuer"] ??
                     "UserService";

            _audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ??
                       configuration["JWT_AUDIENCE"] ??
                       configuration["JwtSettings:Audience"] ??
                       "UserService";

            _expiration = TimeSpan.FromMinutes(
                configuration.GetValue<int>("JwtSettings:ExpiryInMinutes", 480));
        }

        public async Task<PersonalAccessToken> GenerateTokenForAuthenticatedUserAsync(UserReadDto user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await GenerateTokenForUserAsync(user);
        }

        private async Task<PersonalAccessToken> GenerateTokenForUserAsync(UserReadDto user)
        {
            var jti = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            
            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(_expiration),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var personalAccessToken = new PersonalAccessToken
            {
                Token = tokenString,
                UserId = user.Id,
                Jti = jti,
                IsRevoked = false,
            };

            try
            {
                var savedToken = await _tokenRepository.CreateAsync(personalAccessToken);
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
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

                var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = key
                });

                if (!validationResult.IsValid)
                {
                    Log.Warning("Token validation failed: {Error}", validationResult.Exception?.Message);
                    return false;
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                {
                    Log.Warning("Token missing jti claim");
                    return false;
                }

                var dbToken = await _tokenRepository.GetTokenByJtiAsync(jti);
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

                var dbToken = await _tokenRepository.GetTokenByJtiAsync(jti);
                if (dbToken != null)
                {
                    dbToken.IsRevoked = true;
                    await _tokenRepository.UpdateAsync(dbToken);
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
            return await _tokenRepository.DeleteAllTokensForUserAsync(userId);
        }
        public async Task<bool> RevokeTokenAsync1(Guid userId)
        {
            try
            {
                var userIdString = userId.ToString();
                var dbToken = await _tokenRepository.GetTokenByUserIdAsync(userId);
        
                if (dbToken != null && !dbToken.IsRevoked)
                {
                    dbToken.IsRevoked = true;
                    dbToken.UpdatedAt = DateTime.UtcNow;
                    await _tokenRepository.UpdateAsync(dbToken);
                    Log.Information("Successfully revoked token for user {UserId}", userId);
                    return true;
                }

                Log.Warning("No active token found for user {UserId}", userId);
                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error revoking token for user {UserId}", userId);
                return false;
            }
        }
    }
}