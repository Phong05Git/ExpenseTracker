namespace ExpenseTracker.Application.DTOs.Budgets;

public class UpdateBudgetDto
{
    public int CategoryId { get; init; }
    public decimal LimitAmount { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
}