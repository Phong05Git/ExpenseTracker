namespace ExpenseTracker.Application.Interfaces;

public interface ITokenBlacklistService
{
    void Revoke(string tokenId, DateTime expiresAt);
    bool IsRevoked(string tokenId);
}