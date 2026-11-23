using FluentValidation;
using ExpenseTracker.Application.DTOs.Auth;
using ExpenseTracker.Application.Common;

namespace ExpenseTracker.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.Password)
            .NotEmpty()
            .Must(PasswordByteLimit.IsValid)
            .WithMessage(PasswordByteLimit.ErrorMessage);
    }
}
