using FluentValidation;
using ExpenseTracker.Application.DTOs.Budgets;

namespace ExpenseTracker.Application.Validators;

public class UpdateBudgetValidator : AbstractValidator<UpdateBudgetDto>
{
    public UpdateBudgetValidator()
    {
        RuleFor(x => x.LimitAmount)
            .GreaterThan(999)
            .LessThanOrEqualTo(9999999999999.99m);
    }
}