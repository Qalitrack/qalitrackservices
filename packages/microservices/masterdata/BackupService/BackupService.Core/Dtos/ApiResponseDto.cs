namespace BackupService.Core.Dtos;

/// <typeparam name="T">Type of data being returned</typeparam>
public class ApiResponseDto<T>
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message providing additional context
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The payload of the response
    /// </summary>
    public T? Data { get; set; }
}