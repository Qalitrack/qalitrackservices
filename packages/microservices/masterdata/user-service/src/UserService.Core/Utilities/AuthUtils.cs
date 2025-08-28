using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace UserService.Core.Utilities
{
    public static class AuthUtils
    {
        private static ILogger? _logger;
        
        // Method to set logger - call this during startup
        public static void SetLogger(ILogger logger)
        {
            _logger = logger;
        }
        
        public static string? GetUserIdFromClaims(ClaimsPrincipal? user)
        {
            
            if (user == null) 
            {
                return null;
            }
            
           var claimTypes = new[]
            {
                JwtRegisteredClaimNames.Sub,      // "sub"
                ClaimTypes.NameIdentifier,        // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
                "sub",
                "nameid",
                ClaimTypes.Upn,                   // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn"
                ClaimTypes.Email,                 // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
                "user_id",
                "uid",
                "id",
                "userId",                         // Common custom claim
                "UserId",                         // Another common variation
                "unique_name",                    // Sometimes used
                ClaimTypes.Name,
                ClaimTypes.Role,
                "role"                       // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            };

            for (int i = 0; i < claimTypes.Length; i++)
            {
                var claimType = claimTypes[i];
                var claim = user.FindFirst(claimType);
                if (claim != null)
                {
                    
                    if (!string.IsNullOrWhiteSpace(claim.Value))
                    {
                        return claim.Value;
                    }
                    else
                    {
                        _logger?.LogWarning("   ⚠️  Claim found but value is null/empty");
                    }
                }
                else
                {
                    _logger?.LogInformation("   ❌ Claim type not found");
                }
            }

            _logger?.LogError("❌ FAILURE: No user ID found in any expected claim types!");
            return null;
        }
    }
}