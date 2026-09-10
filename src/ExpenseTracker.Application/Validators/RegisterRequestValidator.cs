using FluentValidation;
using ExpenseTracker.Application.DTOs.Auth;

namespace ExpenseTracker.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Username is required.");

        PasswordValidator
            .ApplyPasswordRules(RuleFor(x => x.Password));

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Full name is required.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
    }
}