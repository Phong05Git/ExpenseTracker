using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public int UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }

    public User User { get; private set; } = null!;

    private RefreshToken()
    {
    }

    public RefreshToken(
        int userId,
        string token,
        DateTime expiresAt)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = DateTime.SpecifyKind(
            expiresAt,
            DateTimeKind.Utc);

        LastActivityAt = DateTime.UtcNow;
    }

    public bool IsActive =>
        RevokedAt == null &&
        ExpiresAt > DateTime.UtcNow;

    public void Revoke()
    {
        if (RevokedAt == null)
            RevokedAt = DateTime.UtcNow;
    }

    public void UpdateLastActivity(DateTime activityAt)
    {
        LastActivityAt = DateTime.SpecifyKind(
            activityAt,
            DateTimeKind.Utc);
    }
}