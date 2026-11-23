using System.Text;

namespace ExpenseTracker.Application.Common;

public static class PasswordByteLimit
{
    private const int MaximumBytes = 72;
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public const string ErrorMessage =
        "Password must be valid Unicode, contain no NUL characters, and use at most 72 UTF-8 bytes.";

    public static bool IsValid(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length > MaximumBytes || password.Contains('\0'))
            return false;

        try
        {
            return StrictUtf8.GetByteCount(password) <= MaximumBytes;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }
}
