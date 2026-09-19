using DailyRugby.Application.DTOs;
using DailyRugby.Application.Interfaces;
using DailyRugby.Domain;
using DailyRugby.Web.AutoCompletes;
using DailyRugby.Web.BotServices;
using Discord.Interactions;
using System.Text;
using System.Text.Json;

namespace DailyRugby.Web.SlashCommands;

public class TeamSlashCommands(ITeamCrudService teamService)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("add-team", "Adds a team")]
    public async Task AddTeam(
        [Summary("championship", "The championship to add the team to")]
        [Autocomplete(typeof(ChampionshipAutoComplete))]
        string champId,
        [Summary("playerUsername", "The team's player")]
        string playerUsername,
        [Summary("country", "The team's country")]
        string country,
        int insight, int physique, int technique,
        [Summary("initialCoach", "The team's initial coach")]
        [Autocomplete(typeof(CoachAutoComplete))]
        string initialCoach)
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

        bool coachParsed = Enum.TryParse(initialCoach, true, out Coaches coach);
        if (!coachParsed)
        {
            await FollowupAsync("Invalid coach", ephemeral: true);
            return;
        }

        TeamAddRequest request = new(id,
            playerUsername,
            country,
            insight,
            physique,
            technique,
            coach);

        var addResult = await teamService.AddAsync(request);

        if (!addResult.IsSuccessful)
        {
            await FollowupAsync($"{addResult.Error}: {addResult.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"Created successfully with the id {addResult.Item.Id}", ephemeral: true);
    }

    [SlashCommand("see-teams", "Shows all teams in a championship")]
    public async Task SeeTeams(
        [Summary("championship", "The championship to see the teams from")]
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

        var allTeams = (await teamService.GetAllAsync(id)).OrderByDescending(team => team.Id);
        
        StringBuilder sb = new();
        sb.AppendLine("Teams in the championship:");
        foreach (var team in allTeams)
        {
            sb.AppendLine($"- {team.Country} by {team.PlayerUsername}; " +
                $"I = {team.Insight}, T = {team.Technique}, P = {team.Physique}, " +
                $"Coaches: {string.Join(", ", team.Coaches)}");
        }
        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("delete-team", "Deletes a team from a championship")]
    public async Task DeleteTeam(
        [Summary("team", "The team to delete")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var result = await teamService.DeleteAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync("Deleted successfully", ephemeral: true);
    }

    [SlashCommand("see-team-stats", "Shows the stats of a team")]
    public async Task SeeTeamStats(
        [Summary("team", "The team to see the stats from")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId,

        [Summary("Private", "Whether the response should be sent privately")]
        bool @private = true)
    {
        await DeferAsync(ephemeral: @private);

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        var result = await teamService.GetByIdAsync(id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        StringBuilder sb = new();
        sb.AppendLine($"{result.Item.Country} stats:");
        sb.AppendLine($"- {result.Item.WinCount} wins");
        sb.AppendLine($"- {result.Item.TieCount} ties");
        sb.AppendLine($"- {result.Item.LossCount} losses");
        sb.AppendLine();
        sb.AppendLine($"- {result.Item.PointsScored - result.Item.PointsTaken} point balance");
        sb.AppendLine($"- {result.Item.PointsScored} points scored");
        sb.AppendLine($"- {result.Item.PointsTaken} points suffered");
        sb.AppendLine();
        sb.AppendLine($"- {result.Item.ScoredTriesCount - result.Item.SufferedTriesCount} try balance");
        sb.AppendLine($"- {result.Item.ScoredTriesCount} tries scored");
        sb.AppendLine($"- {result.Item.SufferedTriesCount} tries suffered");

        await FollowupAsync(sb.ToString(), ephemeral: @private);
    }

    [SlashCommand("add-to-stat", "Add to a team's stat")]
    public async Task AddToStat(
        [Summary("team", "The team to add a stat to")]
        [Autocomplete(typeof(TeamAutoComplete))]
        string teamId,

        [Summary("stat", "The stat to add to")]
        [Autocomplete(typeof(TeamStatAutocomplete))]
        string teamStat,

        [Summary("amount", "The amount to add, can be positive or negative")]
        int amount)
    {
        if (!this.CheckRolePermission())
        {
            await RespondAsync(this.UnauthorizedMessage, ephemeral: true);
            return;
        }
        await DeferAsync(ephemeral: true);

        bool idParsed = Guid.TryParse(teamId, out Guid id);
        if (!idParsed)
        {
            await FollowupAsync("Id isn't a valid Guid", ephemeral: true);
            return;
        }

        bool enumParsed = Enum.TryParse(teamStat, true, out TeamStats stat);
        if (!enumParsed)
        {
            await FollowupAsync("Invalid stat", ephemeral: true);
            return;
        }

        var result = await teamService.AddToStatAsync(amount, stat, id);

        if (!result.IsSuccessful)
        {
            await FollowupAsync($"{result.Error}: {result.Message}", ephemeral: true);
            return;
        }

        await FollowupAsync($"Added {amount} points to {result.Item.Country}'s {teamStat}. " +
            $"Its stats are now: I = {result.Item.Insight}, P = {result.Item.Physique}, " +
            $"T = {result.Item.Technique}", ephemeral: true);
    }

    [SlashCommand("see-teams-json", "Get the teams from a championship as JSON")]
    public async Task SeeTeamsJson(
        [Summary("Championship", "The championship to get the teams from")]
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

        var teams = await teamService.GetAllAsync(id);

        var list = new List<object>();

        foreach (var team in teams)
        {
            list.Add(new
            {
                country = team.Country,
                username = team.PlayerUsername,
                technique = team.Technique,
                insight = team.Insight,
                physique = team.Physique,
                coaches = team.Coaches.Select(temp => temp.ToString())
            });
        }

        string json = JsonSerializer.Serialize(list);

        await FollowupAsync(json, ephemeral: true);
    }
}