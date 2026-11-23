using ExpenseTracker.Application.Interfaces;

using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Infrastructure.Identity;

public class PasswordHasherService : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        if (!PasswordByteLimit.IsValid(password))
            throw new ArgumentException(PasswordByteLimit.ErrorMessage, nameof(password));

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hashedPassword)
    {
        return PasswordByteLimit.IsValid(password) &&
            BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
