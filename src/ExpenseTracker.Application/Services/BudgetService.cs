using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class BudgetService(
    IBudgetRepository budgetRepository,
    ICategoryRepository categoryRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IBudgetService
{
    public async Task<Result<IReadOnlyList<BudgetDto>>> GetAllAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var budgets = await budgetRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        var result = new List<BudgetDto>();

        foreach (var budget in budgets)
        {
            var spentAmount =
                await transactionRepository.GetSpentAmountAsync(
                    userId,
                    budget.CategoryId,
                    budget.Month,
                    budget.Year,
                    cancellationToken);

            var budgetDto = mapper.Map<BudgetDto>(budget);

            result.Add(new BudgetDto
            {
                Id = budgetDto.Id,
                CategoryId = budgetDto.CategoryId,
                CategoryName = budgetDto.CategoryName,
                CategoryIcon = budgetDto.CategoryIcon,
                CategoryColor = budgetDto.CategoryColor,
                LimitAmount = budgetDto.LimitAmount,
                SpentAmount = spentAmount,
                RemainingAmount = budgetDto.LimitAmount - spentAmount,
                Month = budgetDto.Month,
                Year = budgetDto.Year
            });
        }

        return Result<IReadOnlyList<BudgetDto>>.Success(result);
    }

    public async Task<Result<BudgetDto>> CreateAsync(
        int userId,
        CreateBudgetDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category == null ||
            !category.IsActive ||
            (category.UserId != null && category.UserId != userId))
        {
            return Result<BudgetDto>.Failure(
                "Category not found or inactive.");
        }

        if (category.Type != TransactionType.Expense)
        {
            return Result<BudgetDto>.Failure(
                "Budget can only be created for expense categories.");
        }

        if (await budgetRepository.ExistsAsync(
                userId,
                request.CategoryId,
                request.Month,
                request.Year,
                null,
                cancellationToken))
        {
            return Result<BudgetDto>.Failure(
                "Budget already exists for this category and period.");
        }

        var budget = new Budget(
            userId,
            request.CategoryId,
            request.LimitAmount,
            request.Month,
            request.Year);

        await budgetRepository.AddAsync(
            budget,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdBudget =
            await budgetRepository.GetWithCategoryByIdAsync(
                budget.Id,
                cancellationToken);

        return Result<BudgetDto>.Success(
            mapper.Map<BudgetDto>(createdBudget));
    }

    public async Task<Result<BudgetDto>> UpdateAsync(
        int userId,
        int budgetId,
        UpdateBudgetDto request,
        CancellationToken cancellationToken = default)
    {
        var budget =
            await budgetRepository.GetWithCategoryByIdAsync(
                budgetId,
                cancellationToken);

        if (budget == null)
        {
            return Result<BudgetDto>.Failure(
                "Budget not found.");
        }

        if (budget.UserId != userId)
        {
            return Result<BudgetDto>.Failure(
                "Access denied.");
        }

        var category = await categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category == null ||
            (category.UserId != null && category.UserId != userId))
        {
            return Result<BudgetDto>.Failure(
                "Category not found or access denied.");
        }

        if (!category.IsActive &&
            budget.CategoryId != request.CategoryId)
        {
            return Result<BudgetDto>.Failure(
                "Inactive category cannot be selected.");
        }

        if (category.Type != TransactionType.Expense)
        {
            return Result<BudgetDto>.Failure(
                "Budget can only be used with expense categories.");
        }

        if (await budgetRepository.ExistsAsync(
                userId,
                request.CategoryId,
                request.Month,
                request.Year,
                budgetId,
                cancellationToken))
        {
            return Result<BudgetDto>.Failure(
                "Budget already exists for this category and period.");
        }

        budget.Update(
            request.CategoryId,
            request.LimitAmount,
            request.Month,
            request.Year);

        budgetRepository.Update(budget);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var updatedBudget =
            await budgetRepository.GetWithCategoryByIdAsync(
                budgetId,
                cancellationToken);

        return Result<BudgetDto>.Success(
            mapper.Map<BudgetDto>(updatedBudget));
    }

    public async Task<Result> DeleteAsync(
        int userId,
        int budgetId,
        CancellationToken cancellationToken = default)
    {
        var budget = await budgetRepository.GetByIdAsync(
            budgetId,
            cancellationToken);

        if (budget == null)
        {
            return Result.Failure("Budget not found.");
        }

        if (budget.UserId != userId)
        {
            return Result.Failure("Access denied.");
        }

        budgetRepository.Remove(budget);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}