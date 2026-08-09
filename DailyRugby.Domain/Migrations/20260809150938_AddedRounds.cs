using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DailyRugby.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddedRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Championships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Championships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChampionshipId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlayerUsername = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    Insight = table.Column<int>(type: "INTEGER", nullable: false),
                    Physique = table.Column<int>(type: "INTEGER", nullable: false),
                    Technique = table.Column<int>(type: "INTEGER", nullable: false),
                    HasInsigthCoach = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasPhysiqueCoach = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTechniqueCoach = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasGeneralCoach = table.Column<bool>(type: "INTEGER", nullable: false),
                    CakesAmount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeamGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Coach = table.Column<int>(type: "INTEGER", nullable: false),
                    Tactic = table.Column<int>(type: "INTEGER", nullable: false),
                    IsUsingCake = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasMoraleBoost = table.Column<bool>(type: "INTEGER", nullable: false),
                    GetsMoraleBoostIfWins = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamGames_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Round = table.Column<int>(type: "INTEGER", nullable: false),
                    ScheduledTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CurrentMinute = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentState = table.Column<int>(type: "INTEGER", nullable: false),
                    TeamAId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeamBId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChampionshipId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Championships_ChampionshipId",
                        column: x => x.ChampionshipId,
                        principalTable: "Championships",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Games_TeamGames_TeamAId",
                        column: x => x.TeamAId,
                        principalTable: "TeamGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_TeamGames_TeamBId",
                        column: x => x.TeamBId,
                        principalTable: "TeamGames",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Games_ChampionshipId",
                table: "Games",
                column: "ChampionshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_TeamAId",
                table: "Games",
                column: "TeamAId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_TeamBId",
                table: "Games",
                column: "TeamBId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamGames_TeamId",
                table: "TeamGames",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ChampionshipId",
                table: "Teams",
                column: "ChampionshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "TeamGames");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Championships");
        }
    }
}
