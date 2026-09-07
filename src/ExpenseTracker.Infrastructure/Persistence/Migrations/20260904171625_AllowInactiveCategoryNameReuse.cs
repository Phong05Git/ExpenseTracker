using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpenseTracker.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AllowInactiveCategoryNameReuse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId_Name_Type",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId_Name_Type",
                table: "Categories",
                columns: new[] { "UserId", "Name", "Type" },
                unique: true,
                filter: "\"UserId\" IS NOT NULL AND \"IsActive\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId_Name_Type",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId_Name_Type",
                table: "Categories",
                columns: new[] { "UserId", "Name", "Type" },
                unique: true,
                filter: "\"UserId\" IS NOT NULL");
        }
    }
}
