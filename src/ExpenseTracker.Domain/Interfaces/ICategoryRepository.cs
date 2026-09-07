using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetAvailableForUserAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(
        int categoryId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        int userId,
        string name,
        int? excludeCategoryId = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasTransactionsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> HasBudgetsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);
}