using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class CategoryRepository(ApplicationDbContext context)
    : Repository<Category>(context), ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAvailableForUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.IsActive && (x.UserId == null || x.UserId == userId))
            .OrderBy(x => x.Type)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsForUserAsync(
        int categoryId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.Id == categoryId &&
                 x.IsActive &&
                 (x.UserId == null || x.UserId == userId),
            cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(
        int userId,
        string name,
        int? excludeCategoryId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLower();

        return await DbSet.AnyAsync(
            x => x.IsActive &&
                 (x.UserId == null || x.UserId == userId) &&
                 x.Name.ToLower() == normalizedName &&
                 (!excludeCategoryId.HasValue ||
                  x.Id != excludeCategoryId.Value),
            cancellationToken);
    }

    public async Task<bool> HasTransactionsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Transactions.AnyAsync(
            x => x.CategoryId == categoryId,
            cancellationToken);
    }

    public async Task<bool> HasBudgetsAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        return await Context.Budgets.AnyAsync(
            x => x.CategoryId == categoryId,
            cancellationToken);
    }
}