using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Transaction : BaseEntity
{
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public string? Note { get; private set; }
    public DateOnly TransactionDate { get; private set; }
    public string? Source { get; private set; }

    public User User { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    public Transaction() { }

    public Transaction(
        int userId,
        int categoryId,
        decimal amount,
        TransactionType type,
        string? note,
        DateOnly transactionDate,
        string? source)
    {
        if (amount < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount must be at least 1000.");

        UserId = userId;
        CategoryId = categoryId;
        Amount = decimal.Round(
            amount,
            2,
            MidpointRounding.ToEven);
        Type = type;
        Note = note;
        TransactionDate = transactionDate;
        Source = source;
    }

    public void Update(
        int categoryId,
        decimal amount,
        TransactionType type,
        string? note,
        DateOnly transactionDate)
    {
        if (amount < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Amount must be at least 1000.");

        CategoryId = categoryId;
        Amount = decimal.Round(
            amount,
            2,
            MidpointRounding.ToEven);
        Type = type;
        Note = note;
        TransactionDate = transactionDate;

        SetUpdatedAt();
    }
}