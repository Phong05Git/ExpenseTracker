using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Interfaces;

public interface IStatisticsRepository
{
    Task<(decimal TotalIncome, decimal TotalExpense)> GetTotalsAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int CategoryId, string CategoryName, string CategoryColor, decimal Amount)>> GetCategoryBreakdownAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        TransactionType type,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)>> GetDailyStatisticsAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)>> GetMonthlyStatisticsAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}