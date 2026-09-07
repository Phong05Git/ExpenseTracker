using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "Icon", "Name", "Type", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { 1, "#2E7D32", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "payments", "Lương", 1, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 2, "#388E3C", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "card_giftcard", "Thưởng", 1, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 3, "#43A047", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "trending_up", "Đầu tư", 1, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 4, "#66BB6A", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "account_balance_wallet", "Thu nhập khác", 1, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 5, "#FB8C00", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "restaurant", "Ăn uống", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 6, "#1E88E5", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "directions_car", "Di chuyển", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 7, "#8E24AA", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "shopping_cart", "Mua sắm", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 8, "#6D4C41", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "receipt_long", "Hóa đơn", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 9, "#546E7A", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "home", "Nhà ở", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 10, "#D32F2F", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "favorite", "Sức khỏe", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 11, "#5E35B1", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "sports_esports", "Giải trí", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 12, "#00897B", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "school", "Giáo dục", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 13, "#039BE5", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "flight", "Du lịch", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null },
                    { 14, "#757575", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), "more_horiz", "Chi phí khác", 2, new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Utc), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 14);
        }
    }
}
