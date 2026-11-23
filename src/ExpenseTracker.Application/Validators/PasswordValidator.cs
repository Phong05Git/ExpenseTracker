using FluentValidation;

using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Validators;

public static class PasswordValidator
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(8)
            .Must(PasswordByteLimit.IsValid)
            .WithMessage(PasswordByteLimit.ErrorMessage)
            .Matches("[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]")
            .WithMessage("Password must contain at least one digit.")
            .Matches(@"[^a-zA-Z0-9]")
            .WithMessage("Password must contain at least one special character.");
    }
}
