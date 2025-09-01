using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace UserService.Core.Utilities
{
    public static class AuthUtils
    {
        public static string? GetUserIdFromClaims(ClaimsPrincipal? user)
        {
            if (user == null) 
            {
                return null;
            }
            
            var claimTypes = new[]
            {
                JwtRegisteredClaimNames.Sub,
                ClaimTypes.NameIdentifier,
                "sub",
                "nameid",
                ClaimTypes.Upn,
                ClaimTypes.Email,
                "user_id",
                "uid",
                "id",
                "userId",
                "UserId",
                "unique_name",
                ClaimTypes.Name,
                ClaimTypes.Role,
                "role"
            };

            foreach (var claimType in claimTypes)
            {
                var claim = user.FindFirst(claimType);
                if (claim != null && !string.IsNullOrWhiteSpace(claim.Value))
                {
                    return claim.Value;
                }
            }

            return null;
        }
    }
}