using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Text;
using System.Text.Json;

namespace DailyRugby.Web.SlashCommands;

public class ChampionshipSlashCommands
    (IChampionshipCrudService champService,
    IGameCrudService gameService,
    IScheduleGetter scheduleGetter)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add-championship", "Creates a championship")]
    public async Task AddChampionship(
        [Summary("name", "The name of the championship")]
        string name,

        [Summary("season", "The season whose rules the championship will abide by")]
        [Autocomplete(typeof(SeasonAutoComplete))]
        string season)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }

        await DeferAsync(ephemeral: true);

        bool parsed = Enum.TryParse(season, true, out Seasons enumSeason);
        if (!parsed)
        {
            await FollowupAsync("Invalid season", ephemeral: true);
            return;
        }

        ChampionshipAddRequest request = new(name, enumSeason);

        var result = await champService.AddAsync(request);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }
        await FollowupAsync($"Success, the championship was created " +
            $"with the Id {result.Item.Id}", ephemeral: true);
    }

    [SlashCommand("see-championships", "See all created championships")]
    public async Task SeeChampionships(
        [Summary("Private", "Whether the response of this command should be private")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);
        var list = (await champService.GetAllAsync()).OrderByDescending(temp => temp.Id);
        StringBuilder sb = new();
        sb.AppendLine("These are all championships registered:");
        foreach (var response in list)
        {
            sb.AppendLine($"- {response.Name}, " +
                $"State = {response.State}, " +
                $"Season = {response.Season}, " +
                $"Id = {response.Id}");
        }
        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("delete-championship", "Deletes a championship")]
    public async Task DeleteChampionship(
        [Summary("id", "The Id of the championship you want to delete")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string id)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool parsed = Guid.TryParse(id, out Guid guid);
        if (!parsed)
        {
            await FollowupAsync($"The id '{id}' isn't a valid Guid", ephemeral: true);
            return;
        }

        var result = await champService.DeleteAsync(guid);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync("Deleted successfully", ephemeral: true);
    }

    [SlashCommand("start-championship", "Sets a championship as started and generates its rounds")]
    public async Task StartChampionship(
        [Summary("Championship", "The championship you want to set as started")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var pairingsResult = await gameService.GenerateRounds(id);
        if (!pairingsResult.IsSuccessful)
        {
            await FollowupAsync($"{pairingsResult.Error}: {pairingsResult.Message}", ephemeral: true);
            return;
        }

        var rounds = pairingsResult.Item
            .GroupBy(temp => temp.Round)
            .OrderBy(temp => temp.Key);
        StringBuilder sb = new();

        sb.AppendLine("These are the rounds generated:");
        sb.AppendLine();
        foreach (var round in rounds)
        {
            sb.AppendLine($"**ROUND {round.Key}**");
            foreach (var game in round)
            {
                sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country}");
            }
            sb.AppendLine();
        }

        await FollowupAsync(sb.ToString(), ephemeral: true);
    }

    [SlashCommand("restart-championship", "Deletes all games in a championship " +
        "and regenerates its pairings")]
    public async Task RestartChampionship(
        [Summary("Championship", "The championship you want to set as started")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var pairingsResult = await gameService.GenerateRounds(id, true);
        if (!pairingsResult.IsSuccessful)
        {
            await FollowupAsync($"{pairingsResult.Error}: {pairingsResult.Message}", ephemeral: true);
            return;
        }

        var rounds = pairingsResult.Item
            .GroupBy(temp => temp.Round)
            .OrderBy(temp => temp.Key);
        StringBuilder sb = new();

        sb.AppendLine("These are the rounds generated:");
        sb.AppendLine();
        foreach (var round in rounds)
        {
            sb.AppendLine($"**ROUND {round.Key}**");
            foreach (var game in round)
            {
                sb.AppendLine($"- {game.TeamA.Team.Country} vs {game.TeamB.Team.Country}");
            }
            sb.AppendLine();
        }

        await FollowupAsync(sb.ToString(), ephemeral: true);
    }

    [SlashCommand("set-as-main", "Sets a championship as the main one")]
    public async Task SetAsMainChampionship(
        [Summary("Championship", "The championship to set as the main one")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var result = await champService.SetAsMainAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"{result.Item.Name} successfully set as the main championship", ephemeral: true);
    }

    [SlashCommand("unset-as-main", "Removes the main championship from its spot")]
    public async Task UnsetAsMainChampionship(
        [Summary("Championship", "The championship to unset as the main one")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var result = await champService.UnsetAsMainAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"{result.Item.Name} successfully unset as the main championship", ephemeral: true);
    }

    [SlashCommand("see-standings", "Shows the standings of a championship")]
    public async Task SeeStandings(
        [Summary("Championship", "The championship to get the standings from")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var standings = await champService.GetStandingsAsync(id);

        StringBuilder sb = new();

        sb.AppendLine("Here are the standings:");
        foreach (var pair in standings)
        {
            int wins = pair.Value.WinCount,
                ties = pair.Value.TieCount,
                loss = pair.Value.LossCount;

            sb.AppendLine($"{pair.Key}: {pair.Value.Country} ({wins}-{ties}-{loss})");
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("see-schedules", "Shows the schedules of the current round")]
    public async Task SeeSchedules(
        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        var result = await scheduleGetter.GetSchedulesAsync();

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine("These are the schedules of the current round:");

        foreach (var schedule in result.Item)
        {
            long timestamp = new DateTimeOffset(schedule.Date, TimeSpan.Zero).ToUnixTimeSeconds();

            sb.AppendLine($"- {schedule.TeamA} vs {schedule.TeamB} at " +
                $"<t:{timestamp}> (<t:{timestamp}:R>)");
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("see-full-leaderboard-json", "Get the full leaderboard as JSON")]
    public async Task SeeFullLeaderboardJson(
        [Summary("Championship", "The championship to get the standings from")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId)
    {
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var champResult = await champService.GetByIdAsync(id);
        if (!champResult.IsSuccessful)
        {
            await FollowupAsync("Id was not found", ephemeral: true);
            return;
        }

        var teams = await champService.GetStandingsAsync(id);

        List<object> list = [];

        int roundCount = champResult.Item.Games.Max(game => game.Round);

        foreach (var pair in teams)
        {
            TeamResponse team = pair.Value;
            int gamesPlayedCount = team.WinCount + team.TieCount + team.LossCount;

            List<string> teamsBeaten = [];
            var gamesPlayed = champResult.Item.Games
                .Where(game => game.TeamA.Team.Id == team.Id
                    || game.TeamB.Team.Id == team.Id);

            foreach (var gamePlayed in gamesPlayed)
            {
                if (gamePlayed.TeamA.Team.Id == team.Id
                    && gamePlayed.TeamAScore > gamePlayed.TeamBScore)
                {
                    teamsBeaten.Add(gamePlayed.TeamB.Team.Country);
                }
                else if (gamePlayed.TeamB.Team.Id == team.Id
                    && gamePlayed.TeamBScore > gamePlayed.TeamAScore)
                {
                    teamsBeaten.Add(gamePlayed.TeamA.Team.Country);
                }
            }

            list.Add(new
            {
                country = team.Country,
                wins = team.WinCount,
                ties = team.TieCount,
                losses = team.LossCount,
                hasBeaten = teamsBeaten,
                pointBalance = team.PointsScored - team.PointsTaken,
                tryBalance = team.ScoredTriesCount - team.SufferedTriesCount,
                tries = team.ScoredTriesCount,
                points = team.PointsScored,
                matchesLeft = roundCount - gamesPlayedCount
            });
        }

        var json = JsonSerializer.Serialize(list);
        await FollowupAsync(json, ephemeral: true);
    }
}