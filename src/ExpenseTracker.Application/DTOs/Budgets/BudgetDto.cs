namespace ExpenseTracker.Application.DTOs.Budgets;

public class BudgetDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = string.Empty;
    public decimal LimitAmount { get; init; }
    public decimal SpentAmount { get; init; }
    public decimal RemainingAmount { get; init; }
    public int Month { get; init; }
    public int Year { get; init; }
}