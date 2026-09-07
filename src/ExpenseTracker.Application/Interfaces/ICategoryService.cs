using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Categories;

namespace ExpenseTracker.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> CreateAsync(int userId, CreateCategoryDto request, CancellationToken cancellationToken = default);
    Task<Result<CategoryDto>> UpdateAsync(int userId, int categoryId, UpdateCategoryDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int categoryId, CancellationToken cancellationToken = default);
}