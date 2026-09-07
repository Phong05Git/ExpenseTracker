using ExpenseTracker.Application.DTOs.Transactions;
using FluentValidation;

namespace ExpenseTracker.Application.Validators;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0);

        RuleFor(x => x.Amount)
            .GreaterThan(999)
            .LessThanOrEqualTo(9999999999999.99m);

        RuleFor(x => x.Note)
            .MaximumLength(500);

        RuleFor(x => x.TransactionDate)
            .Must(date => date != default)
            .WithMessage("Transaction date is required.");

        RuleFor(x => x.Source)
            .MaximumLength(100);
    }
}