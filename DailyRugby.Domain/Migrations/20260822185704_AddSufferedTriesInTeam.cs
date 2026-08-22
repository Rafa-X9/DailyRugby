using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddSufferedTriesInTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SufferedTriesCount",
                table: "Teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SufferedTriesCount",
                table: "Teams");
        }
    }
}
