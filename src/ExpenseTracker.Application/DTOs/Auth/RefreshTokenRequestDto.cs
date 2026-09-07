namespace ExpenseTracker.Application.DTOs.Auth;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}