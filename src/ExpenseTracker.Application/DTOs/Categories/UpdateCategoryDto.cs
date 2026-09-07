using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Application.DTOs.Categories;

public class UpdateCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public TransactionType Type { get; init; }
    public string Icon { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
}