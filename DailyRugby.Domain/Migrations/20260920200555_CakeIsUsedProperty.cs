using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class CakeIsUsedProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "Cakes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "Cakes");
        }
    }
}
