namespace ExpenseTracker.Application.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public T? Data { get; init; }

    public static ApiResponseDto<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponseDto<T> Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}