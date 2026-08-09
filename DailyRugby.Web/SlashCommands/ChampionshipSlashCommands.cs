using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class ChampionshipSlashCommands
        (IChampionshipCrudService champService, IGameCrudService gameService)
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
        await DeferAsync();

        bool parsed = Enum.TryParse(season, true, out Seasons enumSeason);
        if (!parsed)
        {
            await FollowupAsync("Invalid season");
            return;
        }

        ChampionshipAddRequest request = new(name, enumSeason);

        var result = await champService.AddAsync(request);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }
        await FollowupAsync($"Success, the championship was created " +
            $"with the Id {result.Item.Id}");
    }

    [SlashCommand("see-championships", "See all created championships")]
    public async Task SeeChampionships()
    {
        await DeferAsync();
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
        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("delete-championship", "Deletes a championship")]
    public async Task DeleteChampionship(
        [Summary("id", "The Id of the championship you want to delete")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string id)
    {
        await DeferAsync();

        bool parsed = Guid.TryParse(id, out Guid guid);
        if (!parsed)
        {
            await FollowupAsync($"The id '{id}' isn't a valid Guid");
            return;
        }

        var result = await champService.DeleteAsync(guid);
        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}");
            return;
        }

        await FollowupAsync("Deleted successfully");
    }

    [SlashCommand("start-championship", "Sets a championship as started and generates its rounds")]
    public async Task StartChampionship(
        [Summary("Championship", "The championship you want to set as started")]
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

        var pairingsResult = await gameService.GenerateRounds(id);
        if (!pairingsResult.IsSuccessful)
        {
            await FollowupAsync($"{pairingsResult.Error}: {pairingsResult.Message}");
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

        await FollowupAsync(sb.ToString());
    }

    [SlashCommand("restart-championship", "Deletes all games in a championship " +
        "and regenerates its pairings")]
    public async Task RestartChampionship(
        [Summary("Championship", "The championship you want to set as started")]
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

        var pairingsResult = await gameService.GenerateRounds(id, true);
        if (!pairingsResult.IsSuccessful)
        {
            await FollowupAsync($"{pairingsResult.Error}: {pairingsResult.Message}");
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

        await FollowupAsync(sb.ToString());
    }
}