using FluentValidation;
using ExpenseTracker.Application.DTOs.Auth;

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
            .MaximumLength(100);
    }
}