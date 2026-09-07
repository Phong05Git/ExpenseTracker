using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Reports;

public class StatisticsDto
{
    public PeriodType Period { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public IReadOnlyList<StatisticsPointDto> Timeline { get; init; } = [];
    public IReadOnlyList<CategoryBreakdownDto> ExpenseCategoryBreakdown { get; init; } = [];
    public IReadOnlyList<CategoryBreakdownDto> IncomeCategoryBreakdown { get; init; } = [];
}

public class StatisticsPointDto
{
    public DateTime Date { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
}

public class CategoryBreakdownDto
{
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public decimal Percentage { get; init; }
}