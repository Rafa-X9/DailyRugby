using DailyRugby.Domain;
using static DailyRugby.Application.Simulators.SeasonThreeGameSimulator;

namespace DailyRugby.Tests;

public class SeasonThreeTeamStatsTests
{
    [Fact]
    public void TeamStats_NoTactic_DoesNotChangeStats()
    {
        const int insight = 30, physique = 40, technique = 30;

        TeamGame team = CreateTeamGame(insight: insight,
            physique: physique,
            technique: technique,
            Coaches.General,
            Tactics.None);

        var stats = new SeasonThreeStats(team, new());

        Assert.Equal(insight, stats.Insight);
        Assert.Equal(physique, stats.Physique);
        Assert.Equal(technique, stats.Technique);
    }

    [Fact]
    public void TeamStats_WithTactic_NotMatchingCoach_IncreasesStatBySix()
    {
        const int insight = 30;
        TeamGame team = CreateTeamGame(insight, 40, 30, Coaches.Physique, Tactics.Insight);

        var stats = new SeasonThreeStats(team, new());

        Assert.Equal(insight, stats.Insight - 6);
    }

    [Fact]
    public void TeamStats_WithTactic_MatchingCoach_IncreasesStatByTen()
    {
        const int insight = 30;
        TeamGame team = CreateTeamGame(insight, 40, 30, Coaches.Insight, Tactics.Insight);

        var stats = new SeasonThreeStats(team, new());

        Assert.Equal(insight, stats.Insight - 10);
    }

    [Fact]
    public void TeamStats_StrongerTactic_GivesBonusOnlyToStrongerTeam()
    {
        const int insight = 30;

        TeamGame stronger = CreateTeamGame(insight, 30, 40, Coaches.Insight, Tactics.Insight);
        TeamGame weaker = CreateTeamGame(insight, 30, 40, Coaches.Physique, Tactics.Physique);

        var strongerStats = new SeasonThreeStats(stronger, weaker);
        var weakerStats = new SeasonThreeStats(weaker, stronger);

        Assert.Equal(insight, strongerStats.Insight - 10);
        Assert.Equal(insight, weakerStats.Insight);
    }

    #region Helpers

    private static TeamGame CreateTeamGame(int insight,
        int physique,
        int technique,
        Coaches coach,
        Tactics tactic)
        => new()
        {
            Team = new()
            {
                Physique = physique,
                Insight = insight,
                Technique = technique
            },
            Coach = coach,
            Tactic = tactic
        };


    #endregion
}
