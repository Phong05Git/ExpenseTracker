namespace ExpenseTracker.Application.Interfaces;

public interface IJwtTokenSettings
{
    int AccessTokenMinutes { get; }
    int RefreshTokenDays { get; }
}