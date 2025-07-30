using System.Text.Json.Serialization;

namespace UserService.Core.DTOs.Auth;

public class TwoFactorResponseDto
{
    [JsonPropertyName("requires2FA")]
    public bool Requires2FA { get; set; }
    
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
}