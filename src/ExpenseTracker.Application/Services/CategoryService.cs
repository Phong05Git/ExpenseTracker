using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Categories;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : ICategoryService
{
    public async Task<Result<IReadOnlyList<CategoryDto>>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAvailableForUserAsync(
            userId,
            cancellationToken);

        var result = mapper.Map<IReadOnlyList<CategoryDto>>(categories);

        return Result<IReadOnlyList<CategoryDto>>.Success(result);
    }

    public async Task<Result<CategoryDto>> CreateAsync(
        int userId,
        CreateCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();

        var exists = await categoryRepository.ExistsByNameAsync(
            userId,
            name,
            null,
            cancellationToken);

        if (exists)
        {
            return Result<CategoryDto>.Failure(
                "Category name already exists.");
        }

        var category = new Category(
            userId,
            name,
            request.Type,
            request.Icon.Trim(),
            request.Color.Trim());

        await categoryRepository.AddAsync(
            category,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(
            mapper.Map<CategoryDto>(category));
    }

    public async Task<Result<CategoryDto>> UpdateAsync(
        int userId,
        int categoryId,
        UpdateCategoryDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(
            categoryId,
            cancellationToken);

        if (category == null)
            return Result<CategoryDto>.Failure("Category not found.");

        if (category.UserId != userId)
            return Result<CategoryDto>.Failure("Access denied.");

        if (!category.IsActive)
            return Result<CategoryDto>.Failure(
                "Category is inactive.");

        var name = request.Name.Trim();

        var exists = await categoryRepository.ExistsByNameAsync(
            userId,
            name,
            categoryId,
            cancellationToken);

        if (exists)
        {
            return Result<CategoryDto>.Failure(
                "Category name already exists.");
        }

        var hasTransactions = await categoryRepository.HasTransactionsAsync(
            categoryId,
            cancellationToken);

        if (hasTransactions && category.Type != request.Type)
        {
            return Result<CategoryDto>.Failure(
                "Category type cannot be changed because the category is already used by transactions.");
        }

        category.Update(
            name,
            request.Type,
            request.Icon.Trim(),
            request.Color.Trim());

        categoryRepository.Update(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Success(
            mapper.Map<CategoryDto>(category));
    }

    public async Task<Result> DeleteAsync(
        int userId,
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(
            categoryId,
            cancellationToken);

        if (category == null)
            return Result.Failure("Category not found.");

        if (category.UserId != userId)
            return Result.Failure("Access denied.");

        if (!category.IsActive)
            return Result.Failure("Category is already inactive.");

        var hasTransactions = await categoryRepository.HasTransactionsAsync(
            categoryId,
            cancellationToken);

        var hasBudgets = await categoryRepository.HasBudgetsAsync(
            categoryId,
            cancellationToken);

        if (hasTransactions || hasBudgets)
        {
            category.Deactivate();
            categoryRepository.Update(category);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }

        categoryRepository.Remove(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}