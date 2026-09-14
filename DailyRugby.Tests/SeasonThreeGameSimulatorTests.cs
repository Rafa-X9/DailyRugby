using DailyRugby.Application.Simulators;
using DailyRugby.Domain;
using Xunit.Abstractions;

namespace DailyRugby.Tests;

public class SeasonThreeGameSimulatorTests(ITestOutputHelper output)
{
    private readonly SeasonThreeGameSimulator _simulator = new();
    private readonly ITestOutputHelper _output = output;

    #region Generating players

    [Fact]
    public void GeneratePlayers_MainTeamStatsSumUpToActualTeamStats()
    {
        TeamGame team = CreateTeamGame();

        SeasonThreeGameSimulator.CreatePlayers(team);
        var players = team.Players;

        _output.WriteLine($"Generated {players.Count} players");
        foreach (var player in players)
        {
            _output.WriteLine($"#{player.Number}: " +
                $"T: {player.Technique}, " +
                $"P: {player.Physique}, " +
                $"I: {player.Insight}");
        }

        int mainTeamInsight = 0;
        int mainTeamPhysique = 0;
        int mainTeamTechnique = 0;

        foreach (var player in players)
        {
            if (player.Number > 15) break;
            mainTeamInsight += player.Insight;
            mainTeamPhysique += player.Physique;
            mainTeamTechnique += player.Technique;
        }

        Assert.Equal(team.Team.Technique, mainTeamTechnique);
        Assert.Equal(team.Team.Physique, mainTeamPhysique);
        Assert.Equal(team.Team.Insight, mainTeamInsight);
    }

    [Fact]
    public void GeneratePlayers_MainTeamStatsSumUpToAtLeastFour()
    {
        TeamGame team = CreateTeamGame();

        SeasonThreeGameSimulator.CreatePlayers(team);
        var players = team.Players;

        for (int i = 0; i < 15; i++)
        {
            var player = players[i];
            int statSum = player.Insight + player.Physique + player.Technique;
            bool check = statSum >= 4;

            if (!check)
            {
                _output.WriteLine($"Player #{i + 1} violated minimum 4 sum; " +
                    $"their stats sum up to {statSum}");
            }

            Assert.True(check);
        }
    }

    private static TeamGame CreateTeamGame()
        => new()
        {
            Id = Guid.NewGuid(),
            Coach = Coaches.Technique,
            GameId = Guid.NewGuid(),
            GetsMoraleBoostIfWins = false,
            HasMoraleBoost = false,
            IsUsingCake = false,
            Players = [],
            Tactic = Tactics.Insight,
            Team = new()
            {
                Insight = 30,
                Physique = 40,
                Technique = 30
            }
        };

    #endregion
}
