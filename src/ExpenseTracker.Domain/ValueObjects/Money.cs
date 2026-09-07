namespace ExpenseTracker.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Amount cannot be negative.");
        
        Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);
    }
    
    public static Money Zero => new(0);

    public Money Add(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return new Money(Amount + other.Amount);
    }

    public Money Subtract(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);
        
        if (other.Amount > Amount)
            throw new InvalidOperationException("Money amount cannot be negative.");
        
        return new Money(Amount - other.Amount);
    }
}