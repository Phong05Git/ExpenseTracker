using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class TransactionRepository(ApplicationDbContext context)
    : Repository<Transaction>(context), ITransactionRepository
{
    public async Task<Transaction?> GetWithCategoryByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByFilterAsync(
        int userId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int? categoryId,
        TransactionType? type,
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = BuildFilterQuery(
            userId,
            fromDate,
            toDate,
            categoryId,
            type,
            keyword);

        return await query
            .AsNoTracking()
            .Include(x => x.Category)
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByFilterAsync(
        int userId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int? categoryId,
        TransactionType? type,
        string? keyword,
        CancellationToken cancellationToken = default)
    {
        return await BuildFilterQuery(
                userId,
                fromDate,
                toDate,
                categoryId,
                type,
                keyword)
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByDateAsync(
        int userId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x =>
                x.UserId == userId &&
                x.TransactionDate == date)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetSpentAmountAsync(
        int userId,
        int categoryId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        var fromDate = new DateOnly(year, month, 1);
        var toDate = fromDate.AddMonths(1);

        return await DbSet
            .Where(x =>
                x.UserId == userId &&
                x.CategoryId == categoryId &&
                x.Type == TransactionType.Expense &&
                x.TransactionDate >= fromDate &&
                x.TransactionDate < toDate)
            .SumAsync(
                x => x.Amount,
                cancellationToken);
    }

    private IQueryable<Transaction> BuildFilterQuery(
        int userId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int? categoryId,
        TransactionType? type,
        string? keyword)
    {
        var query = DbSet.Where(
            x => x.UserId == userId);

        if (fromDate.HasValue)
        {
            DateOnly fromDateOnly =
                DateOnly.FromDateTime(
                    fromDate.Value.Date);

            query = query.Where(
                x => x.TransactionDate >= fromDateOnly);
        }

        if (toDate.HasValue)
        {
            DateOnly toDateOnly =
                DateOnly.FromDateTime(
                    toDate.Value.Date);

            query = query.Where(
                x => x.TransactionDate < toDateOnly);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(
                x => x.CategoryId == categoryId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(
                x => x.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(
                x =>
                    x.Note != null &&
                    EF.Functions.ILike(
                        x.Note,
                        $"%{keyword.Trim()}%"));
        }

        return query;
    }
}