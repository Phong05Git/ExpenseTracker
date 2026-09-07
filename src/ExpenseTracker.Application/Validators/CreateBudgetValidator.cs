using FluentValidation;
using ExpenseTracker.Application.DTOs.Budgets;

namespace ExpenseTracker.Application.Validators;

public class CreateBudgetValidator : AbstractValidator<CreateBudgetDto>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.LimitAmount)
            .GreaterThan(999)
            .LessThanOrEqualTo(9999999999999.99m);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12);

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 9999);
    }
}