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
        public async Task<PersonalAccessToken> GenerateTokenAsync(string email, string password)
        {
            var user = await userService.ValidateUserCredentials(email, password);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password.");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name,  user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                configuration["Jwt:SecretKey"] ?? 
                configuration["JwtSettings:SecretKey"] ?? 
                throw new InvalidOperationException("JWT Secret Key is not configured")));
    
            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"] ?? configuration["JwtSettings:Issuer"] ?? "UserService",
                audience: configuration["Jwt:Audience"] ?? configuration["JwtSettings:Audience"] ?? "UserService",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token expires in 7 days
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new PersonalAccessToken
            {
                Token = tokenString,
                UserId = user.Id,
                IsRevoked = false,
            };
        }

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
                    ValidateLifetime = false, // optionally set to true if using expiration
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return await Task.FromResult(validatedToken != null);
            }
            catch (Exception ex)
            {
                Log.Error($"Token validation failed: {ex.Message}");
                return await Task.FromResult(false);
            }
        }

        public async Task<Guid?> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                if (jwtToken == null)
                    return null;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                    return userId;

                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Error parsing token for user ID: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var userId = await GetUserIdFromTokenAsync(token);
            if (userId.HasValue)
                return await tokenRepository.RevokeTokenAsync(userId.Value);

            return false;
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
