using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Masterdata.Core.Utils;

public class TokenExtractionService : ITokenExtractionService
{
    public Guid? GetUserIdFromToken(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
            
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
            
        return null;
    }

    public string? GetUserEmailFromToken(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value 
               ?? user.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
    }

    public List<string> GetUserRolesFromToken(ClaimsPrincipal user)
    {

        return user.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();
    }

    public Dictionary<string, string> GetAllClaimsFromToken(ClaimsPrincipal user)
    {

        return user.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(c => c.Value)));
    }

    public bool HasRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }

    public bool HasAnyRole(ClaimsPrincipal user, params string[] roles)
    {
        return roles.Any(role => user.IsInRole(role));
    }

    public bool HasAllRoles(ClaimsPrincipal user, params string[] roles)
    {

        return roles.All(role => user.IsInRole(role));
    }
    
}