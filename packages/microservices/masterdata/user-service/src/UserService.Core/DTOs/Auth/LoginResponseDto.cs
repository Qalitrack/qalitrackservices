using System.Text.Json.Serialization;
using UserService.Core.Entities;

namespace UserService.Core.DTOs.Auth;

public class LoginResponseDto
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    public required string Email { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }
    
    public virtual List<string> UserRoles { get; set; } = new List<string>();
}