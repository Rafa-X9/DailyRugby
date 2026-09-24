using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace DailyRugby.Web.SlashCommands;

public class ChampionshipSlashCommands
    (IChampionshipCrudService champService,
    IGameCrudService gameService,
    IScheduleGetter scheduleGetter,
    IJsonGetter jsonGetter,
    IGameOddsCalculator gameOddsCalculator,
    IChampionshipOddsCalculator champOddsCalculator)
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

        var dataResult = await jsonGetter.GetLeaderBoardAsync(id);

        if (!dataResult.IsSuccessful)
        {
            await FollowupAsync($"{dataResult.Error}: {dataResult.Message}", ephemeral: true);
            return;
        }

        var json = JsonSerializer.Serialize(dataResult.Item);
        await FollowupAsync(json, ephemeral: true);
    }

    [SlashCommand("see-full-schedules-json", "Shows the schedules of the entire championship as JSON")]
    public async Task SeeFullSchedulesJson(
        [Summary("Championship", "The championship to get the schedules from")]
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

        var dataResult = await jsonGetter.GetSchedulesAsync(id);

        if (!dataResult.IsSuccessful)
        {
            await FollowupAsync($"{dataResult.Error}: {dataResult.Message}", ephemeral: true);
            return;
        }

        string json = JsonSerializer.Serialize(dataResult.Item);
        await FollowupAsync(json, ephemeral: true);
    }

    [SlashCommand("see-championship-json", "Get a JSON file containing all the championship's data")]
    public async Task SeeChampionshipJson(
        [Summary("Championship", "The championship to get the data from")]
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

        var data = new
        {
            previousRound = (await jsonGetter.GetPreviousRoundAsync()).Item,
            currentRound = (await jsonGetter.GetCurrentRoundAsync()).Item,
            leaderboard = (await jsonGetter.GetLeaderBoardAsync(id)).Item,
            teams = (await jsonGetter.GetTeamsAsync(id)).Item,
            schedules = (await jsonGetter.GetSchedulesAsync(id)).Item
        };

        string json = JsonSerializer.Serialize(data);

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        await FollowupWithFileAsync(memoryStream, "championship.json", "Here is the JSON file");
    }

    [SlashCommand("calculate-all-odds", "Calculates the odds for all games in a championship")]
    public async Task CalculateAllOdds(
        [Summary("Championship", "The championship to calculate all odds")]
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

        var champResult = await champService.GetByIdAsync(id);

        if (!champResult.IsSuccessful)
        {
            await FollowupAsync($"{champResult.Error}: {champResult.Message}", ephemeral: true);
            return;
        }

        int count = 0;
        foreach (var game in champResult.Item.Games)
        {
            var result = await gameOddsCalculator.GetOddsAsync(game.Id, false);

            if (!result.IsSuccessful)
            {
                await FollowupAsync($"Getting odds for {game.TeamA.Team.Country} vs " +
                    $"{game.TeamB.Team.Country} for round {game.Round} failed ({result.Error}: " +
                    $"{result.Message}). {count} game odds have already been successfully done.",
                    ephemeral: true);
                return;
            }

            count++;
        }

        await FollowupAsync("Done", ephemeral: true);
    }

    [SlashCommand("see-championship-odds", "Get the odds for a championship")]
    public async Task SeeChampionshipOdds(
        [Summary("Championship", "The championship to calculate all odds")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId,

        [Summary("Private", "Whether the reply should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(champId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var oddResult = await champOddsCalculator.GetOddsAsync(id);

        if (!oddResult.IsSuccessful)
        {
            await FollowupAsync($"{oddResult.Error}: {oddResult.Message}", ephemeral: true);
            return;
        }

        StringBuilder sb = new();
        CultureInfo ci = CultureInfo.InvariantCulture;

        sb.AppendLine("First place odds:");
        foreach (var firstPlaceOdd in oddResult.Item.FirstPlaceOdds)
        {
            sb.AppendLine($"- {firstPlaceOdd.Country}: {firstPlaceOdd.Chance.ToString("F2", ci)}");
        }

        sb.AppendLine("Last place odds:");
        foreach (var lastPlaceOdd in oddResult.Item.LastPlaceOdds)
        {
            sb.AppendLine($"- {lastPlaceOdd.Country}: {lastPlaceOdd.Chance.ToString("F2", ci)}");
        }

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }
}