using System.Text.Json.Serialization;
using UserService.Core.Entities;

namespace UserService.Core.DTOs.Auth;

public class LoginResponseDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public string Email { get; set; }
    
    public string FirstName { get; set; }
    
    public string LastName { get; set; }
    
    public virtual List<string> UserRoles { get; set; } = new List<string>();
}