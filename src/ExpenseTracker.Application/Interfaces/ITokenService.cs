using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(
        User user,
        int sessionId,
        DateTime expiresAt);

    RefreshToken GenerateRefreshToken(
        int userId,
        DateTime expiresAt);

    string? GetTokenId(string accessToken);

    DateTime? GetTokenExpiration(string accessToken);
}