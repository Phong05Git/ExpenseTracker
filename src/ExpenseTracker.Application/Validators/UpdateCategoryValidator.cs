using FluentValidation;
using ExpenseTracker.Application.DTOs.Categories;

namespace ExpenseTracker.Application.Validators;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryDto>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Category name is required.");

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Icon)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Category icon is required.");

        RuleFor(x => x.Color)
            .NotEmpty()
            .MaximumLength(20)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Category color is required.");
    }
}