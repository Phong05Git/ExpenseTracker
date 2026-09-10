using FluentValidation;
using ExpenseTracker.Application.DTOs.Users;

namespace ExpenseTracker.Application.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
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