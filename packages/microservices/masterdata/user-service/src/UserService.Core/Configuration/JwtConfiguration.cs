using System.ComponentModel.DataAnnotations;

namespace UserService.Core.Configuration;

public class JwtConfiguration
{
    [Required]
    [MinLength(32, ErrorMessage = "JWT Secret Key must be at least 32 characters long")]
    public string SecretKey { get; set; } = string.Empty;
    
    [Required]
    public string Issuer { get; set; } = "UserService";
    
    [Required] 
    public string Audience { get; set; } = "UserService";
    
    [Range(1, 525600, ErrorMessage = "Expiration minutes must be between 1 and 525600 (1 year)")]
    public int ExpirationMinutes { get; set; } = 10080; // 7 days default
    
    [Range(1, 8760, ErrorMessage = "Refresh token expiration hours must be between 1 and 8760 (1 year)")]
    public int RefreshTokenExpirationHours { get; set; } = 168; // 7 days default
    
    public TimeSpan TokenExpiration => TimeSpan.FromMinutes(ExpirationMinutes);
    public TimeSpan RefreshTokenExpiration => TimeSpan.FromHours(RefreshTokenExpirationHours);
}