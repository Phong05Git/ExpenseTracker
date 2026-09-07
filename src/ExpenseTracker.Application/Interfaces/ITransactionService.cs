using ExpenseTracker.Application.Common;
using ExpenseTracker.Application.DTOs.Common;
using ExpenseTracker.Application.DTOs.Transactions;

namespace ExpenseTracker.Application.Interfaces;

public interface ITransactionService
{
    Task<Result<TransactionDto>> CreateAsync(int userId, CreateTransactionDto request, CancellationToken cancellationToken = default);
    Task<Result<TransactionDto>> UpdateAsync(int userId, int transactionId, UpdateTransactionDto request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int transactionId, CancellationToken cancellationToken = default);
    Task<Result<PagedResultDto<TransactionDto>>> FilterAsync(int userId, TransactionFilterDto filter, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<TransactionDto>>> GetByDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default);
}