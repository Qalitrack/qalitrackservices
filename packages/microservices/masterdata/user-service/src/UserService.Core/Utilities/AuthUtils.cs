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
            _logger?.LogInformation("🔍 === AuthUtils.GetUserIdFromClaims START ===");
            
            if (user == null) 
            {
                _logger?.LogWarning("❌ ClaimsPrincipal is null");
                return null;
            }

            _logger?.LogInformation("✅ ClaimsPrincipal exists");
            _logger?.LogInformation("🔐 IsAuthenticated: {IsAuthenticated}", user.Identity?.IsAuthenticated ?? false);
            _logger?.LogInformation("👤 Identity Name: {Name}", user.Identity?.Name ?? "NULL");
            _logger?.LogInformation("📋 Total Claims Count: {Count}", user.Claims?.Count() ?? 0);

            // Log all claims first for visibility
            _logger?.LogInformation("📋 All Available Claims:");
            foreach (var claim in user.Claims ?? Enumerable.Empty<Claim>())
            {
                _logger?.LogInformation("   🏷️  Type: '{Type}' | Value: '{Value}'", claim.Type, claim.Value);
            }

            _logger?.LogInformation("🔍 Searching for user ID in expected claim types...");

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
                ClaimTypes.Name                   // "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
            };

            for (int i = 0; i < claimTypes.Length; i++)
            {
                var claimType = claimTypes[i];
                _logger?.LogInformation("🔍 [{Index}] Checking claim type: '{ClaimType}'", i + 1, claimType);
                
                var claim = user.FindFirst(claimType);
                if (claim != null)
                {
                    _logger?.LogInformation("   ✅ Claim found! Value: '{Value}'", claim.Value);
                    
                    if (!string.IsNullOrWhiteSpace(claim.Value))
                    {
                        _logger?.LogInformation("🎉 SUCCESS! Found user ID in claim type '{ClaimType}': {UserId}", claimType, claim.Value);
                        _logger?.LogInformation("🔍 === AuthUtils.GetUserIdFromClaims END (SUCCESS) ===");
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
            _logger?.LogInformation("💡 Suggestion: Check your JWT token generation to ensure it includes a user ID claim");
            _logger?.LogInformation("🔍 === AuthUtils.GetUserIdFromClaims END (FAILURE) ===");

            return null;
        }
    }
}