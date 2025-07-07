using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserModule.Data;
using UserModule.Models;

namespace UserModule.Services
{
    public class TokenService(IConfiguration configuration, AppDbContext dbContext, ILogger<TokenService> logger)
        : ITokenService
    {
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly AppDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly ILogger<TokenService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly TimeSpan _tokenExpiration = TimeSpan.FromHours(24);

        public string GenerateToken(User user)
        {
            if (user == null)
            {
                _logger.LogWarning("Attempted to generate token for null user");
                throw new ArgumentNullException(nameof(user));
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted to generate token for inactive user {UserId}", user.Id);
                throw new InvalidOperationException("Cannot generate token for inactive user");
            }

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role != null && !string.IsNullOrEmpty(userRole.Role.Name))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
                    }
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.Add(_tokenExpiration),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool IsValid, Guid UserId)> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdString = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (!Guid.TryParse(userIdString, out var userId))
                {
                    _logger.LogWarning("Invalid user ID format in token: {TokenId}", jwtToken.Id);
                    return (false, Guid.Empty);
                }

                // Check if user is active
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Token validation failed: User {UserId} is inactive or does not exist", userId);
                    return (false, userId);
                }

                // Hash the token for comparison
                var tokenHash = ComputeSha256Hash(token);
                var personalAccessToken = await _dbContext.PersonalAccessTokens
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Token == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);
                if (personalAccessToken == null)
                {
                    _logger.LogWarning("Token validation failed: Token for user {UserId} is revoked or expired", userId);
                    return (false, userId);
                }

                return (true, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return (false, Guid.Empty);
            }
        }

        public async Task<bool> HasRoleAsync(string token, string role)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdString = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (!Guid.TryParse(userIdString, out var userId))
                {
                    return false;
                }

                var user = await _dbContext.Users
                    .AsNoTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                {
                    return false;
                }

                return user.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == role);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string ComputeSha256Hash(string rawData)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}