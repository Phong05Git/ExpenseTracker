using ExpenseTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

public static class CategorySeedData
{
    private static readonly DateTime SeedDate =
        new(2026, 8, 31, 0, 0, 0, DateTimeKind.Utc);

    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.Category>().HasData(
            new
            {
                Id = 1,
                UserId = (int?)null,
                Name = "Lương",
                Type = TransactionType.Income,
                Icon = "payments",
                Color = "#2E7D32",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 2,
                UserId = (int?)null,
                Name = "Thưởng",
                Type = TransactionType.Income,
                Icon = "card_giftcard",
                Color = "#388E3C",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 3,
                UserId = (int?)null,
                Name = "Đầu tư",
                Type = TransactionType.Income,
                Icon = "trending_up",
                Color = "#43A047",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 4,
                UserId = (int?)null,
                Name = "Thu nhập khác",
                Type = TransactionType.Income,
                Icon = "account_balance_wallet",
                Color = "#66BB6A",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 5,
                UserId = (int?)null,
                Name = "Ăn uống",
                Type = TransactionType.Expense,
                Icon = "restaurant",
                Color = "#FB8C00",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 6,
                UserId = (int?)null,
                Name = "Di chuyển",
                Type = TransactionType.Expense,
                Icon = "directions_car",
                Color = "#1E88E5",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 7,
                UserId = (int?)null,
                Name = "Mua sắm",
                Type = TransactionType.Expense,
                Icon = "shopping_cart",
                Color = "#8E24AA",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 8,
                UserId = (int?)null,
                Name = "Hóa đơn",
                Type = TransactionType.Expense,
                Icon = "receipt_long",
                Color = "#6D4C41",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 9,
                UserId = (int?)null,
                Name = "Nhà ở",
                Type = TransactionType.Expense,
                Icon = "home",
                Color = "#546E7A",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 10,
                UserId = (int?)null,
                Name = "Sức khỏe",
                Type = TransactionType.Expense,
                Icon = "favorite",
                Color = "#D32F2F",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 11,
                UserId = (int?)null,
                Name = "Giải trí",
                Type = TransactionType.Expense,
                Icon = "sports_esports",
                Color = "#5E35B1",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 12,
                UserId = (int?)null,
                Name = "Giáo dục",
                Type = TransactionType.Expense,
                Icon = "school",
                Color = "#00897B",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 13,
                UserId = (int?)null,
                Name = "Du lịch",
                Type = TransactionType.Expense,
                Icon = "flight",
                Color = "#039BE5",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            },
            new
            {
                Id = 14,
                UserId = (int?)null,
                Name = "Chi phí khác",
                Type = TransactionType.Expense,
                Icon = "more_horiz",
                Color = "#757575",
                CreatedAt = SeedDate,
                UpdatedAt = SeedDate
            });
    }
}