using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    Task<IReadOnlyList<Budget>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<Budget?> GetWithCategoryByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int userId,
        int categoryId,
        int month,
        int year,
        int? excludeBudgetId = null,
        CancellationToken cancellationToken = default);
}