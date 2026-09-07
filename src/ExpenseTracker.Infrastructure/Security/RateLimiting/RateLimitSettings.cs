namespace ExpenseTracker.Infrastructure.Security.RateLimiting;

public class RateLimitSettings
{
    public int GlobalPermitLimit { get; set; } = 100;
    public int GlobalWindowMinutes { get; set; } = 1;
    public int AuthPermitLimit { get; set; } = 5;
    public int AuthWindowMinutes { get; set; } = 5;
}