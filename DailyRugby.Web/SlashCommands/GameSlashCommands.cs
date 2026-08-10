using DailyRugby.Application.Interfaces;
using DailyRugby.Web.AutoCompletes;
using Discord.Interactions;
using System.Text;

namespace DailyRugby.Web.SlashCommands;

public class GameSlashCommands(IGameCrudService gameService)
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
}