using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetWithCategoryByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByFilterAsync(
        int userId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int? categoryId,
        TransactionType? type,
        string? keyword,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountByFilterAsync(
        int userId,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int? categoryId,
        TransactionType? type,
        string? keyword,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByDateAsync(
        int userId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<decimal> GetSpentAmountAsync(
        int userId,
        int categoryId,
        int month,
        int year,
        CancellationToken cancellationToken = default);
}