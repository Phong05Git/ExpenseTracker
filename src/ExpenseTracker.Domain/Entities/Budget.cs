using ExpenseTracker.Domain.Common;

namespace ExpenseTracker.Domain.Entities;

public class Budget : BaseEntity
{
    public int UserId { get; private set; }
    public int CategoryId { get; private set; }
    public decimal LimitAmount { get; private set; }
    public int Month { get; private set; }
    public int Year { get; private set; }

    public User User { get; private set; } = null!;
    public Category Category { get; private set; } = null!;

    private Budget() { }

    public Budget(
        int userId,
        int categoryId,
        decimal limitAmount,
        int month,
        int year)
    {
        if (limitAmount < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(limitAmount),
                "Limit amount must be at least 1000.");

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(
                nameof(month));

        if (year < 1)
            throw new ArgumentOutOfRangeException(
                nameof(year));

        UserId = userId;
        CategoryId = categoryId;
        LimitAmount = decimal.Round(
            limitAmount,
            2,
            MidpointRounding.ToEven);
        Month = month;
        Year = year;
    }

    public void Update(
        int categoryId,
        decimal limitAmount,
        int month,
        int year)
    {
        if (categoryId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(categoryId));

        if (limitAmount < 1000)
            throw new ArgumentOutOfRangeException(
                nameof(limitAmount),
                "Limit amount must be at least 1000.");

        if (month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(
                nameof(month));

        if (year < 1)
            throw new ArgumentOutOfRangeException(
                nameof(year));

        CategoryId = categoryId;
        LimitAmount = decimal.Round(
            limitAmount,
            2,
            MidpointRounding.ToEven);
        Month = month;
        Year = year;
        SetUpdatedAt();
    }
}