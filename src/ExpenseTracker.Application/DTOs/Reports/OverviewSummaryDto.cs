namespace ExpenseTracker.Application.DTOs.Reports;

public class OverviewSummaryDto
{
    public decimal TotalIncome { get; init; }
    public decimal TotalExpense { get; init; }
    public decimal Balance { get; init; }
}