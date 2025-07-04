namespace DriverService.Core.DTOs;

public class ApiResponseDto<T>
{
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponseDto<T> SuccessResult(T data, string message = "")
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponseDto<T> ErrorResult(List<string> errors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = "Validation errors occurred",
            Errors = errors
        };
    }
}