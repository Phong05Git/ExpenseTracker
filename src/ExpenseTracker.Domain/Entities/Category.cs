using ExpenseTracker.Domain.Common;
using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class Category : BaseEntity
{
    public int? UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public string Icon { get; private set; } = string.Empty;
    public string Color { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public User? User { get; private set; }
    public ICollection<Transaction> Transactions { get; private set; } = new List<Transaction>();
    public ICollection<Budget> Budgets { get; private set; } = new List<Budget>();

    private Category()
    {
    }

    public Category(
        int? userId,
        string name,
        TransactionType type,
        string icon,
        string color)
    {
        UserId = userId;
        Name = name;
        Type = type;
        Icon = icon;
        Color = color;
    }

    public void Update(
        string name,
        TransactionType type,
        string icon,
        string color)
    {
        Name = name;
        Type = type;
        Icon = icon;
        Color = color;
        SetUpdatedAt();
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
}