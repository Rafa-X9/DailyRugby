using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddChampIdInGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Championships_ChampionshipId",
                table: "Games");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChampionshipId",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ChampionshipId1",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Games_ChampionshipId1",
                table: "Games",
                column: "ChampionshipId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Championships_ChampionshipId",
                table: "Games",
                column: "ChampionshipId",
                principalTable: "Championships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Championships_ChampionshipId1",
                table: "Games",
                column: "ChampionshipId1",
                principalTable: "Championships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_Championships_ChampionshipId",
                table: "Games");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Championships_ChampionshipId1",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_ChampionshipId1",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "ChampionshipId1",
                table: "Games");

            migrationBuilder.AlterColumn<Guid>(
                name: "ChampionshipId",
                table: "Games",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Championships_ChampionshipId",
                table: "Games",
                column: "ChampionshipId",
                principalTable: "Championships",
                principalColumn: "Id");
        }
    }
}
