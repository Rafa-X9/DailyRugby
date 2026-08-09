using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGameTeamsToList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_TeamGames_TeamAId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_TeamGames_TeamBId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_TeamAId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_TeamBId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TeamAId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "TeamBId",
                table: "Games");

            migrationBuilder.CreateIndex(
                name: "IX_TeamGames_GameId",
                table: "TeamGames",
                column: "GameId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamGames_Games_GameId",
                table: "TeamGames",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamGames_Games_GameId",
                table: "TeamGames");

            migrationBuilder.DropIndex(
                name: "IX_TeamGames_GameId",
                table: "TeamGames");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamAId",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TeamBId",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Games_TeamAId",
                table: "Games",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_TeamBId",
                table: "Games",
                column: "TeamBId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_TeamGames_TeamAId",
                table: "Games",
                column: "TeamAId",
                principalTable: "TeamGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_TeamGames_TeamBId",
                table: "Games",
                column: "TeamBId",
                principalTable: "TeamGames",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
