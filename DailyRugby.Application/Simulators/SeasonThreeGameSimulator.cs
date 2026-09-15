using DailyRugby.Application.Interfaces;
using DailyRugby.Application.Utilitaries;
using DailyRugby.Domain;

namespace DailyRugby.Application.Simulators;

public class SeasonThreeGameSimulator : ISpecificGameSimulator
{


    public Task SaveGameAsync(GameEvent gameEvent, AppDbContext db)
    {
        throw new NotImplementedException();
    }

    public GameEvent SimulateNextMinute(Game game)
    {
        throw new NotImplementedException();
    }

    public static void CreatePlayers(TeamGame team)
    {
        int minimum = 4
            + (team.HasMoraleBoost ? 1 : 0)
            + (team.IsUsingCake ? 1 : 0);

        int teamTotal = team.Team.Insight + team.Team.Physique + team.Team.Technique;

        if (teamTotal < minimum * 15)
        {
            throw new InvalidOperationException(
                $"The team has {teamTotal} total stats, but at least " +
                $"{minimum * 15} are required.");
        }

        var random = Random.Shared;

        int[] playerTotals = Enumerable.Repeat(minimum, 15).ToArray();

        int extra = teamTotal - (minimum * 15);

        for (int i = 0; i < extra; i++)
        {
            playerTotals[random.Next(15)]++;
        }

        Array.Sort(playerTotals);
        Array.Reverse(playerTotals);

        int[] insight = new int[15];
        int[] physique = new int[15];
        int[] technique = new int[15];

        int remainingInsight = team.Team.Insight;
        int remainingPhysique = team.Team.Physique;
        int remainingTechnique = team.Team.Technique;

        for (int i = 0; i < 15; i++)
        {
            int playerTotal = playerTotals[i];

            int futureCapacity = 0;

            for (int j = i + 1; j < 15; j++)
                futureCapacity += playerTotals[j];

            int minInsight = Math.Max(
                0,
                remainingInsight - futureCapacity);

            int maxInsight = Math.Min(
                playerTotal,
                remainingInsight);

            maxInsight = Math.Min(
                maxInsight,
                playerTotal + futureCapacity
                    - remainingPhysique);

            maxInsight = Math.Min(
                maxInsight,
                playerTotal + futureCapacity
                    - remainingTechnique);

            if (minInsight > maxInsight)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            int playerInsight = random.Next(
                minInsight,
                maxInsight + 1);

            int remainingForPlayer =
                playerTotal - playerInsight;

            remainingInsight -= playerInsight;

            int minPhysique = Math.Max(
                0,
                remainingPhysique - futureCapacity);

            minPhysique = Math.Max(
                minPhysique,
                remainingForPlayer - remainingTechnique);

            int maxPhysique = Math.Min(
                remainingForPlayer,
                remainingPhysique);

            maxPhysique = Math.Min(
                maxPhysique,
                remainingForPlayer + futureCapacity
                    - remainingTechnique);

            if (minPhysique > maxPhysique)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            int playerPhysique = random.Next(
                minPhysique,
                maxPhysique + 1);

            int playerTechnique =
                remainingForPlayer - playerPhysique;

            if (playerTechnique < 0 ||
                playerTechnique > remainingTechnique)
            {
                throw new InvalidOperationException(
                    "Could not generate a valid player distribution.");
            }

            insight[i] = playerInsight;
            physique[i] = playerPhysique;
            technique[i] = playerTechnique;

            remainingPhysique -= playerPhysique;
            remainingTechnique -= playerTechnique;
        }

        for (int i = 0; i < 15; i++)
        {
            team.Players.Add(new Player(
                Guid.NewGuid(),
                i + 1,
                insight[i],
                physique[i],
                technique[i],
                IsOnField: true));
        }

        for (int i = 15; i < 23; i++)
        {
            int playerInsight = random.Next(minimum + 1);

            int remaining = minimum - playerInsight;

            int playerPhysique = random.Next(remaining + 1);

            int playerTechnique =
                remaining - playerPhysique;

            team.Players.Add(new Player(
                Guid.NewGuid(),
                i + 1,
                playerInsight,
                playerPhysique,
                playerTechnique,
                IsOnField: false));
        }
    }

    public sealed record SeasonThreeStats
    {
        public int Insight { get; init; }
        public int Physique { get; init; }
        public int Technique { get; init; }

        public SeasonThreeStats(TeamGame team, TeamGame opponent)
        {
            Insight = team.Team.Insight;
            Physique = team.Team.Physique;
            Technique = team.Team.Technique;

            if (team.Tactic == Tactics.None) return;

            if (team.Tactic == Tactics.General)
            {
                Insight++;
                Physique++;
                Technique++;
                return;
            }

            if (team.Tactic == opponent.Tactic) return;

            if (opponent.Tactic is Tactics.General or Tactics.None
                || team.Tactic.IsStrongerThan(opponent.Tactic))
            {
                if (team.Tactic == Tactics.Physique)
                {
                    Physique += 6;
                    if (team.Coach == Coaches.Physique) Physique += 4;
                }
                else if (team.Tactic == Tactics.Insight)
                {
                    Insight += 6;
                    if (team.Coach == Coaches.Insight) Insight += 4;
                }
                else if (team.Tactic == Tactics.Technique)
                {
                    Technique += 6;
                    if (team.Coach == Coaches.Technique) Technique += 4;
                }
            }
        }
    }
}