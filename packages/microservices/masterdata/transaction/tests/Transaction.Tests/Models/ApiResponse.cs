using System.Text.Json.Serialization;
using Transaction.Core.DTOs;

namespace Transaction.Tests.Models;

public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    
    [JsonPropertyName("errors")]
    public object? Errors { get; set; }
    
    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }
}
