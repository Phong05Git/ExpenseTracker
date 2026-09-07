using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Transactions;

public class TransactionDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategoryIcon { get; init; } = string.Empty;
    public string CategoryColor { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public TransactionType Type { get; init; }
    public string? Note { get; init; }
    public DateOnly TransactionDate { get; init; }
    public string? Source { get; init; }
}