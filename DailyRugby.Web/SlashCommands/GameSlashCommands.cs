using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class GameSlashCommands(IGameCrudService gameService, IGameSimulatorManager simulator)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("see-games", "Shows all games from a championship")]
    public async Task SeeGames(
        [Summary("championship", "The championship to show the games from")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        var list = (await gameService.GetAllAsync(id))
            .OrderBy(temp => temp.Round)
            .ToList();

        if (list.Count == 0)
        {
            await FollowupAsync("That championship has no games");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("All games in the championship:");
        foreach (var game in list)
        {
            sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country} " +
                $"in Round {game.Round}");
        }

        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("see-teams-games", "Sees all the games of a specific team")]
    public async Task SeeTeamsGames(
        [Summary("team", "The team to see the games")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        var gamesResult = await gameService.GetByTeamIdAsync(id);

        if (!gamesResult.IsSuccessful)
        {
            await FollowupAsync($"{gamesResult.Error}: {gamesResult.Message}");
            return;
        }

        var sortedGames = gamesResult.Item.OrderBy(temp => temp.Round).ToList();

        var requestedTeam = gamesResult.Item
            .SelectMany(temp => new List<TeamResponse> { temp.TeamA.Team, temp.TeamB.Team })
            .First(temp => temp.Id == id);

        StringBuilder sb = new();
        sb.AppendLine($"These are all {requestedTeam.Country}'s games:");

        foreach (var game in sortedGames)
        {
            if (game.TeamA.Team.Id == id)
            {
                sb.AppendLine($"- vs {game.TeamB.Team.Country} on round {game.Round}");
            }
            else
            {
                sb.AppendLine($"- vs {game.TeamA.Team.Country} on round {game.Round}");
            }
        }

        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("schedule-game", "Schedules a game")]
    public async Task ScheduleGame(
        [Summary("game", "The game to schedule")]
        [Autocomplete(typeof(GameAutocomplete))]
        string gameId,
        int yearUtc,
        int monthUtc,
        int dayUtc,
        int hourUtc,
        int minuteUtc)
    {
        await DeferAsync();

        bool idParsed = Guid.TryParse(gameId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid");
            return;
        }

        DateTime dateTime = new(yearUtc, monthUtc, dayUtc, hourUtc, minuteUtc, 0);

        var result = await simulator.ScheduleGameAsync(id, dateTime);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        await FollowupAsync("Scheduled successfully");
    }

    [SlashCommand("see-current-round", "Shows all games from the current round")]
    public async Task SeeCurrentRound()
    {
        await DeferAsync();

        var result = await gameService.GetCurrentRoundAsync();

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"**ROUND {result.Item.First().Round}**");
        foreach (var game in result.Item)
        {
            sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country}");
        }

        await FollowupAsync(sb.ToString());
    }
}