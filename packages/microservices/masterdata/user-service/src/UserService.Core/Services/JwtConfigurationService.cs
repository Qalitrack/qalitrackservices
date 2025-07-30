using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserService.Core.Configuration;

namespace UserService.Core.Services;

public interface IJwtConfigurationService
{
    JwtConfiguration GetConfiguration();
    string GetSecretKey();
    string GetIssuer();
    string GetAudience();
    TimeSpan GetTokenExpiration();
    TimeSpan GetRefreshTokenExpiration();
}

public class JwtConfigurationService : IJwtConfigurationService
{
    private readonly JwtConfiguration _jwtConfig;
    private readonly ILogger<JwtConfigurationService> _logger;

    public JwtConfigurationService(IOptions<JwtConfiguration> jwtOptions, ILogger<JwtConfigurationService> logger)
    {
        _jwtConfig = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Set default values from environment variables if not in config
        _jwtConfig.SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? _jwtConfig.SecretKey;
        _jwtConfig.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _jwtConfig.Issuer;
        _jwtConfig.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _jwtConfig.Audience;
        
        // Set default expiration if not set
        if (_jwtConfig.ExpirationMinutes <= 0)
            _jwtConfig.ExpirationMinutes = 30;
        if (_jwtConfig.RefreshTokenExpirationHours <= 0)
            _jwtConfig.RefreshTokenExpirationHours = 24;
            
        ValidateConfiguration();
    }


    public JwtConfiguration GetConfiguration() => _jwtConfig;

    public string GetSecretKey()
    {
        if (string.IsNullOrWhiteSpace(_jwtConfig.SecretKey))
        {
            _logger.LogError("JWT Secret Key is not configured or is empty");
            throw new InvalidOperationException("JWT Secret Key is not configured");
        }
        
        if (_jwtConfig.SecretKey.Length < 32)
        {
            _logger.LogError("JWT Secret Key is too short. Must be at least 32 characters");
            throw new InvalidOperationException("JWT Secret Key must be at least 32 characters long");
        }
        
        return _jwtConfig.SecretKey;
    }

    public string GetIssuer() => _jwtConfig.Issuer;
    public string GetAudience() => _jwtConfig.Audience;
    public TimeSpan GetTokenExpiration() => _jwtConfig.TokenExpiration;
    public TimeSpan GetRefreshTokenExpiration() => _jwtConfig.RefreshTokenExpiration;

    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_jwtConfig.SecretKey))
            errors.Add("JWT Secret Key is required");
        else if (_jwtConfig.SecretKey.Length < 32)
            errors.Add("JWT Secret Key must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(_jwtConfig.Issuer))
            errors.Add("JWT Issuer is required");

        if (string.IsNullOrWhiteSpace(_jwtConfig.Audience))
            errors.Add("JWT Audience is required");

        if (_jwtConfig.ExpirationMinutes <= 0 || _jwtConfig.ExpirationMinutes > 525600)
            errors.Add("JWT Expiration minutes must be between 1 and 525600");

        if (_jwtConfig.RefreshTokenExpirationHours <= 0 || _jwtConfig.RefreshTokenExpirationHours > 8760)
            errors.Add("JWT Refresh token expiration hours must be between 1 and 8760");

        if (errors.Any())
        {
            var errorMessage = "JWT Configuration validation failed: " + string.Join(", ", errors);
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogInformation("JWT Configuration validated successfully. Issuer: {Issuer}, Audience: {Audience}, Token Expiry: {TokenExpiry} minutes", 
            _jwtConfig.Issuer, _jwtConfig.Audience, _jwtConfig.ExpirationMinutes);
    }
}