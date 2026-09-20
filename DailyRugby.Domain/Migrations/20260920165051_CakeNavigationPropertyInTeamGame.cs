using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class CakeNavigationPropertyInTeamGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUsingCake",
                table: "TeamGames");

            migrationBuilder.AddColumn<Guid>(
                name: "CakeId",
                table: "TeamGames",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamGames_CakeId",
                table: "TeamGames",
                column: "CakeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamGames_Cakes_CakeId",
                table: "TeamGames",
                column: "CakeId",
                principalTable: "Cakes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamGames_Cakes_CakeId",
                table: "TeamGames");

            migrationBuilder.DropIndex(
                name: "IX_TeamGames_CakeId",
                table: "TeamGames");

            migrationBuilder.DropColumn(
                name: "CakeId",
                table: "TeamGames");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsingCake",
                table: "TeamGames",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
