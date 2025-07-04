using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserService.Core.DTOs;
using UserService.Core.Interfaces;

namespace UserService.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? "UserService";
        _audience = _configuration["Jwt:Audience"] ?? "UserService";
        _accessTokenExpirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
        _refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<string> GenerateAccessTokenAsync(JwtClaimsDto claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwtClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, claims.UserId),
            new(ClaimTypes.Name, claims.Username),
            new(ClaimTypes.Email, claims.Email),
            new(ClaimTypes.GivenName, claims.FirstName),
            new(ClaimTypes.Surname, claims.LastName),
            new("user_id", claims.UserId),
            new("username", claims.Username),
            new("email", claims.Email),
            new("first_name", claims.FirstName),
            new("last_name", claims.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add roles
        foreach (var role in claims.Roles)
        {
            jwtClaims.Add(new Claim(ClaimTypes.Role, role));
            jwtClaims.Add(new Claim("role", role));
        }

        // Add permissions
        foreach (var permission in claims.Permissions)
        {
            jwtClaims.Add(new Claim("permission", permission));
        }

        // Add organizations
        foreach (var organization in claims.Organizations)
        {
            jwtClaims.Add(new Claim("organization", organization));
        }

        // Add current organization if specified
        if (!string.IsNullOrEmpty(claims.CurrentOrganization))
        {
            jwtClaims.Add(new Claim("current_organization", claims.CurrentOrganization));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: jwtClaims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
            signingCredentials: creds
        );

        return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }

    public async Task<string> GenerateRefreshTokenAsync()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return await Task.FromResult(Convert.ToBase64String(randomNumber));
    }

    public async Task<JwtClaimsDto?> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var claims = new JwtClaimsDto
            {
                UserId = principal.FindFirst("user_id")?.Value ?? string.Empty,
                Username = principal.FindFirst("username")?.Value ?? string.Empty,
                Email = principal.FindFirst("email")?.Value ?? string.Empty,
                FirstName = principal.FindFirst("first_name")?.Value ?? string.Empty,
                LastName = principal.FindFirst("last_name")?.Value ?? string.Empty,
                Roles = principal.FindAll("role").Select(c => c.Value).ToList(),
                Permissions = principal.FindAll("permission").Select(c => c.Value).ToList(),
                Organizations = principal.FindAll("organization").Select(c => c.Value).ToList(),
                CurrentOrganization = principal.FindFirst("current_organization")?.Value
            };

            return await Task.FromResult(claims);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> IsTokenValidAsync(string token)
    {
        var claims = await ValidateTokenAsync(token);
        return claims != null;
    }

    public async Task<bool> IsRefreshTokenValidAsync(string refreshToken)
    {
        // Basic validation - in a real implementation, you might want to check against stored refresh tokens
        try
        {
            var bytes = Convert.FromBase64String(refreshToken);
            return await Task.FromResult(bytes.Length == 64);
        }
        catch
        {
            return false;
        }
    }

    public async Task<DateTime> GetTokenExpirationAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return await Task.FromResult(jwtToken.ValidTo);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    public async Task<string?> GetUserIdFromTokenAsync(string token)
    {
        var claims = await ValidateTokenAsync(token);
        return claims?.UserId;
    }

    public async Task<IEnumerable<string>> GetRolesFromTokenAsync(string token)
    {
        var claims = await ValidateTokenAsync(token);
        return claims?.Roles ?? new List<string>();
    }

    public async Task<IEnumerable<string>> GetPermissionsFromTokenAsync(string token)
    {
        var claims = await ValidateTokenAsync(token);
        return claims?.Permissions ?? new List<string>();
    }

    public async Task<string?> GetClaimFromTokenAsync(string token, string claimType)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);
            return await Task.FromResult(claim?.Value);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        // In a real implementation, you would add the token to a blacklist or revocation list
        // For now, we'll just return true to indicate the operation was successful
        return await Task.FromResult(true);
    }
}