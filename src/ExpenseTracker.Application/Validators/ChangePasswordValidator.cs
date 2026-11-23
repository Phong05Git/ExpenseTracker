using FluentValidation;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Validators;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .Must(PasswordByteLimit.IsValid)
            .WithMessage(PasswordByteLimit.ErrorMessage);

        PasswordValidator
            .ApplyPasswordRules(RuleFor(x => x.NewPassword));

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password.");
    }
}
