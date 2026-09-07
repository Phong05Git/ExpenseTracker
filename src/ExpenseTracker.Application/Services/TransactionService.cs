using AutoMapper;
using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Interfaces;

namespace ExpenseTracker.Application.Services;

public class TransactionService(
    ITransactionRepository transactionRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : ITransactionService
{
    public async Task<Result<TransactionDto>> CreateAsync(
        int userId,
        CreateTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var category = await categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category == null ||
            !category.IsActive ||
            (category.UserId != null && category.UserId != userId))
        {
            return Result<TransactionDto>.Failure(
                "Category not found or inactive.");
        }

        if (category.Type != request.Type)
        {
            return Result<TransactionDto>.Failure(
                "Transaction type does not match category type.");
        }

        var transaction = new Transaction(
            userId,
            request.CategoryId,
            request.Amount,
            request.Type,
            request.Note?.Trim(),
            request.TransactionDate,
            request.Source?.Trim());

        await transactionRepository.AddAsync(
            transaction,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var createdTransaction =
            await transactionRepository.GetWithCategoryByIdAsync(
                transaction.Id,
                cancellationToken);

        return Result<TransactionDto>.Success(
            mapper.Map<TransactionDto>(createdTransaction));
    }

    public async Task<Result<TransactionDto>> UpdateAsync(
        int userId,
        int transactionId,
        UpdateTransactionDto request,
        CancellationToken cancellationToken = default)
    {
        var transaction =
            await transactionRepository.GetWithCategoryByIdAsync(
                transactionId,
                cancellationToken);

        if (transaction == null)
        {
            return Result<TransactionDto>.Failure(
                "Transaction not found.");
        }

        if (transaction.UserId != userId)
        {
            return Result<TransactionDto>.Failure(
                "Access denied.");
        }

        var category = await categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category == null ||
            (category.UserId != null && category.UserId != userId))
        {
            return Result<TransactionDto>.Failure(
                "Category not found or access denied.");
        }

        if (!category.IsActive &&
            transaction.CategoryId != request.CategoryId)
        {
            return Result<TransactionDto>.Failure(
                "Inactive category cannot be selected.");
        }

        if (category.Type != request.Type)
        {
            return Result<TransactionDto>.Failure(
                "Transaction type does not match category type.");
        }

        transaction.Update(
            request.CategoryId,
            request.Amount,
            request.Type,
            request.Note?.Trim(),
            request.TransactionDate);

        transactionRepository.Update(transaction);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        var updatedTransaction =
            await transactionRepository.GetWithCategoryByIdAsync(
                transactionId,
                cancellationToken);

        return Result<TransactionDto>.Success(
            mapper.Map<TransactionDto>(updatedTransaction));
    }

    public async Task<Result> DeleteAsync(
        int userId,
        int transactionId,
        CancellationToken cancellationToken = default)
    {
        var transaction = await transactionRepository.GetByIdAsync(
            transactionId,
            cancellationToken);

        if (transaction == null)
        {
            return Result.Failure(
                "Transaction not found.");
        }

        if (transaction.UserId != userId)
        {
            return Result.Failure(
                "Access denied.");
        }

        transactionRepository.Remove(transaction);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<PagedResultDto<TransactionDto>>> FilterAsync(
        int userId,
        TransactionFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(filter.Page, 1);
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await transactionRepository.GetByFilterAsync(
            userId,
            filter.FromDate,
            filter.ToDate,
            filter.CategoryId,
            filter.Type,
            filter.Keyword,
            page,
            pageSize,
            cancellationToken);

        var totalItems = await transactionRepository.CountByFilterAsync(
            userId,
            filter.FromDate,
            filter.ToDate,
            filter.CategoryId,
            filter.Type,
            filter.Keyword,
            cancellationToken);

        var result = new PagedResultDto<TransactionDto>
        {
            Items = mapper.Map<IReadOnlyList<TransactionDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                (double)totalItems / pageSize)
        };

        return Result<PagedResultDto<TransactionDto>>.Success(result);
    }

    public async Task<Result<IReadOnlyList<TransactionDto>>> GetByDateAsync(
        int userId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var transactions =
            await transactionRepository.GetByDateAsync(
                userId,
                date,
                cancellationToken);

        return Result<IReadOnlyList<TransactionDto>>.Success(
            mapper.Map<IReadOnlyList<TransactionDto>>(transactions));
    }
}