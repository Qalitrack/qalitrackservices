using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserModule.Data;
using UserModule.Services;

namespace UserModule.Authentication
{
    public class SanctumAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ITokenService _tokenService;
        private readonly ILogger<SanctumAuthenticationHandler> _logger;

        public SanctumAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ITokenService tokenService)
            : base(options, logger, encoder)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _logger = logger.CreateLogger<SanctumAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                _logger.LogWarning("Missing Authorization header");
                return AuthenticateResult.NoResult();
            }

            var authHeader = Request.Headers["Authorization"].ToString();
            
            // Accept Bearer tokens and treat them as Sanctum tokens (since browsers send Bearer by default)
            string token = null;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader["Bearer ".Length..];
            }
            else if (authHeader.StartsWith("Sanctum ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader["Sanctum ".Length..];
            }
            else
            {
                _logger.LogWarning("Invalid Authorization header format. Expected 'Bearer ' or 'Sanctum ' prefix");
                return AuthenticateResult.NoResult();
            }

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Missing token in Authorization header");
                return AuthenticateResult.NoResult();
            }

            var (isValid, userId) = await _tokenService.ValidateTokenAsync(token);
            if (!isValid)
            {
                _logger.LogWarning("Token validation failed for token");
                return AuthenticateResult.Fail("Invalid token");
            }

            var user = await Context.RequestServices.GetRequiredService<AppDbContext>()
                .Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return AuthenticateResult.Fail("User not found");
            }

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            }.ToList();

            var userRoles = user.UserRoles
                .Where(ur => ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name))
                .Select(ur => new Claim(ClaimTypes.Role, ur.Role.Name));
            claims.AddRange(userRoles);

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}