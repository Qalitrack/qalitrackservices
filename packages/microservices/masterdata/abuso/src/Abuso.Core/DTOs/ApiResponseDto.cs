namespace Abuso.Core.DTOs;

public class ApiResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public int StatusCode { get; set; }
}

public class ApiResponseDto<T> : ApiResponseDto
{
    public T? Data { get; set; }
}