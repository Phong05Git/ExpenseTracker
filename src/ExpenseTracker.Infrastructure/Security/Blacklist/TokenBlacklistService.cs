using System.Collections.Concurrent;
using ExpenseTracker.Application.Interfaces;

namespace ExpenseTracker.Infrastructure.Security.Blacklist;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

    public void Revoke(string tokenId, DateTime expiresAt)
    {
        _blacklistedTokens[tokenId] = expiresAt;
    }

    public bool IsRevoked(string tokenId)
    {
        if (!_blacklistedTokens.TryGetValue(tokenId, out var expiresAt))
            return false;

        if (expiresAt <= DateTime.UtcNow)
        {
            _blacklistedTokens.TryRemove(tokenId, out _);
            return false;
        }

        return true;
    }
}