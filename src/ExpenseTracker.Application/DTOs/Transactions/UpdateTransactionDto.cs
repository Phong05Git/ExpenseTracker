using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Transactions;

public class UpdateTransactionDto
{
    public int CategoryId { get; init; }
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public string? Note { get; init; }
    public DateOnly TransactionDate { get; init; }
}