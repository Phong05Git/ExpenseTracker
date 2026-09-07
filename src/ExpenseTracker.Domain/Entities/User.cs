using System.Net.NetworkInformation;
using System.Transactions;
using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class User : BaseEntity
{
    public ICollection<Category> Categories { get; private set; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; private set; } = new List<Budget>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public int FailedAttempts { get; private set; }
    public DateTime? LockUntil { get; private set; }
    
    private User() { }

    public User(string username, string passwordHash, string fullName, string email)
    {
        Username = username;
        PasswordHash = passwordHash;
        FullName = fullName;
        Email = email;
        FailedAttempts = 0;
    }

    public void UpdateProfile(string fullName, string email)
    {
        FullName = fullName;
        Email = email;
        SetUpdatedAt();
    }
    
    public void UpdatePasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        SetUpdatedAt();
    }

    public void IncreaseFailedAttempts()
    {
        FailedAttempts++;
        SetUpdatedAt();
    }

    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        LockUntil = null;
        SetUpdatedAt();
    }

    public void Lock(DateTime lockUntil)
    {
        LockUntil = lockUntil;
        SetUpdatedAt();
    }
}