using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class StatisticsRepository(ApplicationDbContext context)
    : IStatisticsRepository
{
    public async Task<(decimal TotalIncome, decimal TotalExpense)> GetTotalsAsync(
        int userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var startDateOnly =
            DateOnly.FromDateTime(startDate);

        var endDateOnly =
            DateOnly.FromDateTime(endDate);

        var totals = await context.Transactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.TransactionDate >= startDateOnly &&
                x.TransactionDate < endDateOnly)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalIncome = g
                    .Where(x => x.Type == TransactionType.Income)
                    .Sum(x => (decimal?)x.Amount) ?? 0m,

                TotalExpense = g
                    .Where(x => x.Type == TransactionType.Expense)
                    .Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        return totals == null
            ? (0m, 0m)
            : (totals.TotalIncome, totals.TotalExpense);
    }

    public async Task<IReadOnlyList<(int CategoryId, string CategoryName, string CategoryColor, decimal Amount)>>
        GetCategoryBreakdownAsync(
            int userId,
            DateTime startDate,
            DateTime endDate,
            TransactionType type,
            CancellationToken cancellationToken = default)
    {
        var startDateOnly =
            DateOnly.FromDateTime(startDate);

        var endDateOnly =
            DateOnly.FromDateTime(endDate);

        var result = await context.Transactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Type == type &&
                x.TransactionDate >= startDateOnly &&
                x.TransactionDate < endDateOnly)
            .GroupBy(x => new
            {
                x.CategoryId,
                CategoryName = x.Category.Name,
                CategoryColor = x.Category.Color
            })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Key.CategoryColor,
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(cancellationToken);

        return result
            .Select(x => (
                x.CategoryId,
                x.CategoryName,
                x.CategoryColor,
                x.Amount))
            .ToList();
    }

    public async Task<IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)>>
        GetDailyStatisticsAsync(
            int userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
    {
        var startDateOnly =
            DateOnly.FromDateTime(startDate);

        var endDateOnly =
            DateOnly.FromDateTime(endDate);

        var result = await context.Transactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.TransactionDate >= startDateOnly &&
                x.TransactionDate < endDateOnly)
            .GroupBy(x => x.TransactionDate)
            .Select(g => new
            {
                Date = g.Key,
                Income = g
                    .Where(x => x.Type == TransactionType.Income)
                    .Sum(x => (decimal?)x.Amount) ?? 0m,

                Expense = g
                    .Where(x => x.Type == TransactionType.Expense)
                    .Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return result
            .Select(x => (
                x.Date.ToDateTime(TimeOnly.MinValue),
                x.Income,
                x.Expense))
            .ToList();
    }

    public async Task<IReadOnlyList<(DateTime Date, decimal Income, decimal Expense)>>
        GetMonthlyStatisticsAsync(
            int userId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
    {
        var startDateOnly =
            DateOnly.FromDateTime(startDate);

        var endDateOnly =
            DateOnly.FromDateTime(endDate);

        var result = await context.Transactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.TransactionDate >= startDateOnly &&
                x.TransactionDate < endDateOnly)
            .GroupBy(x => new
            {
                x.TransactionDate.Year,
                x.TransactionDate.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,

                Income = g
                    .Where(x => x.Type == TransactionType.Income)
                    .Sum(x => (decimal?)x.Amount) ?? 0m,

                Expense = g
                    .Where(x => x.Type == TransactionType.Expense)
                    .Sum(x => (decimal?)x.Amount) ?? 0m
            })
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        return result
            .Select(x => (
                new DateTime(
                    x.Year,
                    x.Month,
                    1),
                x.Income,
                x.Expense))
            .ToList();
    }
}