using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Budgets;

namespace ExpenseTracker.Application.Interfaces;

public interface IBudgetService
{
    Task<Result<IReadOnlyList<BudgetDto>>> GetAllAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<BudgetDto>> CreateAsync(int userId, CreateBudgetDto request, CancellationToken cancellationToken = default);
    Task<Result<BudgetDto>> UpdateAsync(int userId, int budgetId, UpdateBudgetDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int budgetId, CancellationToken cancellationToken = default);
}