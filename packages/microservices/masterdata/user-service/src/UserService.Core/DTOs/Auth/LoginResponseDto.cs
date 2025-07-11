using System.Text.Json.Serialization;

namespace UserService.Core.DTOs.Auth;

public class LoginResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
    
    [JsonPropertyName("role")]
    public string Role { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }  
    
}