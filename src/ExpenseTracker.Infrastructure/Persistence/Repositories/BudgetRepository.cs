using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence.Repositories;

public class BudgetRepository(ApplicationDbContext context)
    : Repository<Budget>(context), IBudgetRepository
{
    public async Task<IReadOnlyList<Budget>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ThenBy(x => x.Category.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetWithCategoryByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int userId,
        int categoryId,
        int month,
        int year,
        int? excludeBudgetId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            x => x.UserId == userId &&
                 x.CategoryId == categoryId &&
                 x.Month == month &&
                 x.Year == year &&
                 (!excludeBudgetId.HasValue ||
                  x.Id != excludeBudgetId.Value),
            cancellationToken);
    }
}