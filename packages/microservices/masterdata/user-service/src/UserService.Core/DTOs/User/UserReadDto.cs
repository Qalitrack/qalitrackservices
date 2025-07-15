using System.Text.Json.Serialization;
using UserService.Core.Entities;

namespace UserService.Core.DTOs.User;

public class UserReadDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("isFirstLogin")]
    public bool IsFirstLogin { get; set; } = false;

    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("lastLoginDate")]
    public DateTime? LastLoginDate { get; set; }

    // Navigation properties
    [JsonPropertyName("roles")]
    public ICollection<string> Roles { get; set; } = new List<string>();

    // Helper method to get full name
    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}";
}
