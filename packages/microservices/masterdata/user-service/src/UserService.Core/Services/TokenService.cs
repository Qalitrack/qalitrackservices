using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Core.Interfaces;
using Serilog;

namespace UserService.Core.Services
{
    public class TokenService(IUserService userService, IConfiguration configuration, ITokenRepository tokenRepository)
        : ITokenService
    {
        public async Task<PersonalAccessToken> GenerateTokenAsync(string email, string password)
        {
            var user = await userService.ValidateUserCredentials(email, password);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var jti = Guid.NewGuid().ToString(); // Generate unique token ID

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["Jwt:SecretKey"] ??
                configuration["JwtSettings:SecretKey"] ??
                throw new InvalidOperationException("JWT Secret Key is not configured")));

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"] ?? "UserService",
                audience: configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"] ?? "UserService",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var personalAccessToken = new PersonalAccessToken
            {
                Token = tokenString,
                UserId = user.Id.ToString(),
                Jti = jti,
                IsRevoked = false,
            };

            try
            {
                var savedToken = await tokenRepository.CreateAsync(personalAccessToken);
                Log.Information("Token created and saved for user {UserId}", user.Id);
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
                    Log.Warning("Invalid token format: token does not have 3 parts");
                    return false;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    configuration["Jwt:SecretKey"] ??
                    configuration["JwtSettings:SecretKey"] ??
                    throw new InvalidOperationException("JWT Secret Key is not configured")));

                var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"] ?? "UserService",
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"] ?? "UserService",
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

        public async Task<Guid?> GetUserIdFromTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            try
            {
                token = token.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase).Trim();

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    return userId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error parsing token for user ID");
            }

            return null;
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

        public async Task<bool> RevokeAndDeleteTokenAsync(Guid userId)
        {
            var token = await tokenRepository.GetTokenByUserIdAsync(userId);

            if (token != null)
            {
                token.IsRevoked = true;
                await tokenRepository.UpdateAsync(token);
                return await tokenRepository.DeleteTokenAsync(userId);
            }

            return false;
        }
    }
}
