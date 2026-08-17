using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPointsCountInTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PointsScored",
                table: "Teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PointsTaken",
                table: "Teams",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PointsScored",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PointsTaken",
                table: "Teams");
        }
    }
}
