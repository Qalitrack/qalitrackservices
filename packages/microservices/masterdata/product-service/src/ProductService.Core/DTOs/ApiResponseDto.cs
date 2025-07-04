namespace ProductService.Core.DTOs;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponseDto<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

public class ApiResponseDto : ApiResponseDto<object>
{
    public static ApiResponseDto SuccessResponse(string message = "Success")
    {
        return new ApiResponseDto
        {
            Success = true,
            Message = message
        };
    }

    public new static ApiResponseDto ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponseDto
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}