using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserService.Core.DTOs.Auth;

public class TwoFactorRequestDto
{
    [Required]
    [JsonPropertyName("sessionId")]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(6, MinimumLength = 6)]
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}